using MvCamCtrl.NET;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Common.Exceptions;
using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1.Infrastructure.Hardware.Camera
{
    public sealed class CameraManager : ICameraService
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public const Int32 CUSTOMER_PIXEL_FORMAT = unchecked((Int32)0x80000000);

        // Return value to use when a managed wrapper-level error occurs (SDK lacks MV_ERR)
        private const int MV_ERROR = -1;

        public MyCamera.MV_CC_DEVICE_INFO_LIST DeviceList { get; } = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        private readonly MyCamera _cam = new MyCamera();
        public readonly ConcurrentQueue<byte[]> Queue = new ConcurrentQueue<byte[]>();
        private readonly int _maxQueue;
        private Thread _grabThread;
        private volatile bool _grabbing;
        private IntPtr _driverBuf = IntPtr.Zero;
        private uint _driverBufSize;

        public event Action<byte[]> FrameEncoded;
        public event Action<string> Error;
        public event Action<Bitmap> FramePreview;
        private IntPtr _displayHandle = IntPtr.Zero;
        private IntPtr _annotatedBuf = IntPtr.Zero;
        private uint _annotatedSize = 0;
        public event Action<byte[]> FrameReadyForGrpc;

        // Safety flags/lock
        private readonly object _disposeLock = new object();
        private volatile bool _isClosing;
        private volatile bool _isClosed;
        private volatile bool _deviceOpened;
        private volatile bool _cameraSdkAvailable = true;
        public event Action<Bitmap> FrameReady;

        public CameraManager(ConcurrentQueue<byte[]> queue, int maxQueue = 5)
        {
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _maxQueue = maxQueue;

            // Check if camera SDK is available
            _cameraSdkAvailable = CheckCameraSdkAvailability();
        }

        public IntPtr DisplayHandle
        {
            get => _displayHandle;
            set => _displayHandle = value;
        }

        private bool CheckCameraSdkAvailability()
        {
            try
            {
                // Try to call a simple SDK function to check if it's available
                // This will throw an exception if the DLL is not found or functions are missing
                var dummyList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE, ref dummyList);
                return true;
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("[Camera] Camera SDK DLL not found");
                return false;
            }
            catch (EntryPointNotFoundException)
            {
                Console.WriteLine("[Camera] Camera SDK function not found");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Camera] Camera SDK check failed: {ex.Message}");
                return false;
            }
        }

        public bool IsCameraSdkAvailable => _cameraSdkAvailable;

        public bool Open(MyCamera.MV_CC_DEVICE_INFO device)
        {
            if (!_cameraSdkAvailable)
            {
                var ex = new CameraException("Camera SDK is not available!");
                Error?.Invoke(ex.Message);
                throw ex;
            }

            lock (_disposeLock)
            {
                if (_isClosing || _isClosed) return false;
                if (_deviceOpened) return true;

                Console.WriteLine($"[CAM] Creating device...");
                int rCreate = _cam.MV_CC_CreateDevice_NET(ref device);
                Console.WriteLine($"[CAM] CreateDevice result = 0x{rCreate:X}");
                if (rCreate != MyCamera.MV_OK)
                {
                    var ex = new CameraException($"Create device fail 0x{rCreate:X}");
                    Error?.Invoke(ex.Message);
                    throw ex;
                }

                Console.WriteLine($"[CAM] Opening device...");
                int rOpen = _cam.MV_CC_OpenDevice_NET();
                Console.WriteLine($"[CAM] OpenDevice result = 0x{rOpen:X}");
                if (rOpen != MyCamera.MV_OK)
                {
                    try { _cam.MV_CC_DestroyDevice_NET(); } catch { }
                    var ex = new CameraException($"Open device fail 0x{rOpen:X}");
                    Error?.Invoke(ex.Message);
                }

                try
                {
                    _cam.MV_CC_SetEnumValue_NET("AcquisitionMode",
                        (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                    _cam.MV_CC_SetEnumValue_NET("TriggerMode",
                        (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Camera] Warning setting params: {ex.Message}");
                }

                _deviceOpened = true;
                Console.WriteLine("[CAM] Device opened & ready");
                return true;
            }
        }

        public Task<bool> OpenAsync()
        {
            // This is a simplified implementation - in a real scenario, you would need to pass the device info
            // For now, we'll just return a completed task with false
            return Task.FromResult(false);
        }

        /// <summary>
        /// Starts grab thread and returns SDK StartGrabbing result or MV_ERROR.
        /// </summary>
        public int Start()
        {
            // Check if camera SDK is available
            if (!_cameraSdkAvailable)
            {
                return MV_ERROR;
            }

            lock (_disposeLock)
            {
                if (_isClosing || _isClosed) return MV_ERROR;
                Console.WriteLine(_deviceOpened);
                if (!_deviceOpened) return MV_ERROR;

                _grabbing = true;
                _grabThread = new Thread(GrabLoop) { IsBackground = true };
                _grabThread.Start();

                try
                {
                    return _cam.MV_CC_StartGrabbing_NET();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Camera] StartGrabbing exception: {ex.Message}");
                    return MV_ERROR;
                }
            }
        }

        public Task<int> StartAsync()
        {
            return Task.FromResult(Start());
        }

        /// <summary>
        /// Stops the grab loop and joins the thread (safe to call from UI thread).
        /// </summary>
        public void Stop()
        {
            _grabbing = false;

            try
            {
                if (_grabThread != null && _grabThread.IsAlive)
                {
                    try { _cam.MV_CC_StopGrabbing_NET(); } catch { } // best-effort
                    _grabThread.Join(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Camera] Stop join error: {ex.Message}");
            }
        }

        public Task StopAsync()
        {
            Stop();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Public helper to stop grabbing safely from FormClosing.
        /// </summary>
        public void StopGrabLoop()
        {
            Stop();
            // ensure thread reference cleaned
            _grabThread = null;
        }

        private void GrabLoop()
        {
            var frameOut = new MyCamera.MV_FRAME_OUT();
            var conv = new MyCamera.MV_PIXEL_CONVERT_PARAM();
            MyCamera.MV_DISPLAY_FRAME_INFO stDisplayInfo = new MyCamera.MV_DISPLAY_FRAME_INFO();

            try
            {
                while (_grabbing)
                {
                    if (_isClosing || _isClosed) break;

                    int r = _cam.MV_CC_GetImageBuffer_NET(ref frameOut, 1000);
                    if (r != MyCamera.MV_OK) continue;

                    try
                    {
                        int w = frameOut.stFrameInfo.nWidth;
                        int h = frameOut.stFrameInfo.nHeight;
                        uint need = (uint)(w * h * 3);

                        if (need > _annotatedSize)
                        {
                            if (_annotatedBuf != IntPtr.Zero) Marshal.FreeHGlobal(_annotatedBuf);
                            _annotatedBuf = Marshal.AllocHGlobal((int)need);
                            _annotatedSize = need;
                        }

                        // Convert pixel type
                        conv.nWidth = (ushort)w;
                        conv.nHeight = (ushort)h;
                        conv.enSrcPixelType = frameOut.stFrameInfo.enPixelType;
                        conv.pSrcData = frameOut.pBufAddr;
                        conv.nSrcDataLen = frameOut.stFrameInfo.nFrameLen;
                        conv.pDstBuffer = _annotatedBuf;
                        conv.nDstBufferSize = _annotatedSize;
                        conv.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
                        _cam.MV_CC_ConvertPixelType_NET(ref conv);

                        byte[] bmp;
                        try
                        {
                            bmp = BuildBmpFrame(_annotatedBuf, w, h);
                        }
                        catch (Exception exEnc)
                        {
                            Console.WriteLine($"[Camera] JPEG encode error: {exEnc.Message}");
                            bmp = Array.Empty<byte>();
                        }

                        // enqueue & preview
                        if (bmp.Length > 0)
                        {
                            if (Queue.Count >= _maxQueue) Queue.TryDequeue(out _);
                            Queue.Enqueue(bmp);

                            // Use using so GDI+ resources are freed quickly
                            using (var ms = new MemoryStream(bmp))
                            using (var bmpFrame = new Bitmap(ms))
                            {
                                FramePreview?.Invoke((Bitmap)bmpFrame.Clone());
                            }

                            FrameEncoded?.Invoke(bmp);
                            FrameReadyForGrpc?.Invoke(bmp);
                        }

                        // Display using SDK (best effort)
                        stDisplayInfo.hWnd = _displayHandle;
                        stDisplayInfo.pData = _annotatedBuf;
                        stDisplayInfo.nDataLen = (uint)(w * h * 3);
                        stDisplayInfo.nWidth = (ushort)w;
                        stDisplayInfo.nHeight = (ushort)h;
                        stDisplayInfo.enPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
                        try { _cam.MV_CC_DisplayOneFrame_NET(ref stDisplayInfo); } catch { }
                    }
                    finally
                    {
                        // Must release SDK buffer
                        try { _cam.MV_CC_FreeImageBuffer_NET(ref frameOut); } catch { }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Camera] GrabLoop exception: {ex.Message}");
            }
        }

        private byte[] BuildBmpFrame(IntPtr rgbPtr, int width, int height)
        {
            if (rgbPtr == IntPtr.Zero || width <= 0 || height <= 0)
                return null;

            try
            {
                // Buat Mat dari pointer ke data RGB
                using (var mat = Mat.FromPixelData(height, width, MatType.CV_8UC3, rgbPtr))
                using (var bgr = new Mat())
                {
                    // Konversi RGB → BGR agar warna benar
                    Cv2.CvtColor(mat, bgr, ColorConversionCodes.RGB2BGR);

                    // Encode hasil ke JPEG (tanpa bounding box)
                    Cv2.ImEncode(".bmp", bgr, out byte[] bmpData);
                    return bmpData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BuildBmpFrame] Error: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_isClosed || _isClosing) return;
                _isClosing = true;
            }

            try
            {
                // stop capture loop first
                Stop();

                // ask SDK to stop grabbing (best-effort)
                if (_deviceOpened)
                {
                    try { _cam.MV_CC_StopGrabbing_NET(); } catch { }
                }

                // close/destroy only if opened
                if (_deviceOpened)
                {
                    try { _cam.MV_CC_CloseDevice_NET(); }
                    catch (Exception ex) { Console.WriteLine($"[Camera] CloseDevice error: {ex.Message}"); }

                    try { _cam.MV_CC_DestroyDevice_NET(); }
                    catch (Exception ex) { Console.WriteLine($"[Camera] DestroyDevice error: {ex.Message}"); }

                    _deviceOpened = false;
                }

                // free driver buffer if allocated
                if (_driverBuf != IntPtr.Zero)
                {
                    try { Marshal.FreeHGlobal(_driverBuf); }
                    catch (Exception ex) { Console.WriteLine($"[Camera] Free driverBuf error: {ex.Message}"); }
                    finally { _driverBuf = IntPtr.Zero; _driverBufSize = 0; }
                }

                // free annotated buffer
                if (_annotatedBuf != IntPtr.Zero)
                {
                    try { Marshal.FreeHGlobal(_annotatedBuf); }
                    catch (Exception ex) { Console.WriteLine($"[Camera] Free annotatedBuf error: {ex.Message}"); }
                    finally { _annotatedBuf = IntPtr.Zero; _annotatedSize = 0; }
                }
            }
            finally
            {
                _isClosed = true;
                _isClosing = false;
            }
        }


        public void Close()
        {
            lock (_disposeLock)
            {
                if (_isClosing || _isClosed) return;

                // 1. Hentikan loop grab
                Stop();

                // 2. Stop grabbing SDK
                if (_deviceOpened)
                    try { _cam.MV_CC_StopGrabbing_NET(); } catch { }

                // 3. Tutup handle
                if (_deviceOpened)
                {
                    try { _cam.MV_CC_CloseDevice_NET(); } catch { }
                    try { _cam.MV_CC_DestroyDevice_NET(); } catch { }
                    _deviceOpened = false;
                }

                // 4. Bersihkan buffer & event
                if (_annotatedBuf != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_annotatedBuf);
                    _annotatedBuf = IntPtr.Zero;
                    _annotatedSize = 0;
                }
            }
        }
        public Task CloseAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }

        // SDK wrappers with safe error fallback
        public int GetEnumValue(string key, ref MyCamera.MVCC_ENUMVALUE value)
        {
            try { return _cam.MV_CC_GetEnumValue_NET(key, ref value); }
            catch (Exception ex) { Console.WriteLine($"[Camera] GetEnumValue error: {ex.Message}"); return MV_ERROR; }
        }

        public int GetIntValueEx(string key, ref MyCamera.MVCC_INTVALUE_EX value)
        {
            try { return _cam.MV_CC_GetIntValueEx_NET(key, ref value); }
            catch (Exception ex) { Console.WriteLine($"[Camera] GetIntValueEx error: {ex.Message}"); return MV_ERROR; }
        }

        public void SetDisplayHandle(IntPtr handle)
        {
            _displayHandle = handle;
        }

        public int GetFloatValue(string key, ref MyCamera.MVCC_FLOATVALUE val)
        {
            try { return _cam.MV_CC_GetFloatValue_NET(key, ref val); }
            catch (Exception ex) { Console.WriteLine($"[Camera] GetFloatValue error: {ex.Message}"); return MV_ERROR; }
        }

        public int SetFloatValue(string key, float value)
        {
            try { return _cam.MV_CC_SetFloatValue_NET(key, value); }
            catch (Exception ex) { Console.WriteLine($"[Camera] SetFloatValue error: {ex.Message}"); return MV_ERROR; }
        }

        public int SetEnumValue(string key, uint value)
        {
            try { return _cam.MV_CC_SetEnumValue_NET(key, value); }
            catch (Exception ex) { Console.WriteLine($"[Camera] SetEnumValue error: {ex.Message}"); return MV_ERROR; }
        }
    }
}
