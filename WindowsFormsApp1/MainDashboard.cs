using Api;
using Microsoft.Extensions.DependencyInjection;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WindowsFormsApp1.Infrastructure.Hardware.Camera;
using Label = System.Windows.Forms.Label;
using Panel = System.Windows.Forms.Panel;
using WindowsFormsApp1.Core.Common.Helpers;
using WindowsFormsApp1.Core.Domain.Flow.Engine;
using WindowsFormsApp1.Core.Entities.Models;
using static WindowsFormsApp1.Core.Common.Helpers.VideoGrabber;
using WindowsFormsApp1.Core.Domain.Flow.Dag;
using WindowsFormsApp1.Presentation.Flow;

namespace WindowsFormsApp1
{
    public partial class MainDashboard : Form
    {
        // --------- Services / State ----------
        private readonly GrpcService _grpc = new GrpcService();
        private readonly ImageManipulationUtils _imageUtil = new ImageManipulationUtils();
        private readonly ImageGrabber _grabber = new ImageGrabber();
        //private readonly FileUtils _fileUtils = new FileUtils();
        //private ImageDbOperation _imageDb => Program.DbHelper;

        // Hik SDK discovery store
        private MyCamera.MV_CC_DEVICE_INFO_LIST _deviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();

        // Buffering for SDK conversions (kept as in original)
        private IntPtr _convertDstBuf = IntPtr.Zero;
        private uint _convertDstBufLen = 0;
        private Bitmap _bitmap = null;
        private PixelFormat _bitmapPixelFormat = PixelFormat.DontCare;

        // UI/flow flags
        private bool _grabbing;
        private bool _sidebarExpand;
        private CancellationTokenSource _ctsPreview;
        private readonly CancellationTokenSource _ctsForward = new CancellationTokenSource();

        // Sidebar animation constants
        private const float CollapsedWidth = 15f;
        private const float Step = 10f;
        private float _expandedWidth;

        // Reconnect
        private System.Timers.Timer _reconnectTimer;
        private IServiceProvider _provider;
        private IFlowContext _ctx;
        private CameraManager _cam;
        private const int RECONNECT_INTERVAL_MS = 3000;
        private FileUtils _fileUtils;

        private DagExecutor _executor;
        private CancellationTokenSource _flowCts = new CancellationTokenSource();
        // Add field to track DAG execution task
        private Task _dagExecutionTask;

        public MainDashboard(IServiceProvider provider)
        {
            InitializeComponent();
            _provider = provider;
            _ctx = provider.GetRequiredService<IFlowContext>();
            _fileUtils = provider.GetRequiredService<FileUtils>();
            _cam = provider.GetRequiredService<CameraManager>(); // instance A

            _cam.Error += msg => MessageBox.Show(msg);
            _grabber.FramePreview += OnFramePreview;
            _grpc.OnDisconnected += ScheduleReconnect;

            // Safe timer init
            _reconnectTimer = new System.Timers.Timer(RECONNECT_INTERVAL_MS)
            {
                AutoReset = true
            };

            _reconnectTimer.Elapsed += (s, e) => TryGrpcReconnect();
        }

        // ------------- Form lifecycle -------------

        private async void Form1_Load(object sender, EventArgs e)
        {
            // UI perf improvements
            TryEnableDoubleBuffer(tableLayoutPanel9);

            DeviceListAcq();

            // gRPC startup
            var ok = await _grpc.StartAsync();
            if (!ok)
            {
                MessageBox.Show("Gagal koneksi ke Python server 50052");
                return;
            }

            showSidebarBtn.Visible = true;
            showSidebarBtn.BringToFront();

            _ctx.DisplayHandle = pictureBox5.Handle;   // di MainDashboard setelah handle tersedia
        }

        private async void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop timers
            if (_reconnectTimer != null)
            {
                _reconnectTimer.Stop();
                _reconnectTimer.Dispose();
                _reconnectTimer = null;
            }

            // Cancel tasks
            if (_ctsPreview != null) _ctsPreview.Cancel();
            _ctsForward.Cancel();

            // Dispose devices
            if (_cam != null) _cam.Dispose();
            if (_grpc != null) await _grpc.DisposeAsync();
        }

        // ------------- UI helpers -------------

        private static void TryEnableDoubleBuffer(Control c)
        {
            try
            {
                var prop = c.GetType().GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
                );
                if (prop != null) prop.SetValue(c, true, null);
            }
            catch { /* ignore */ }
        }

        private void SafeInvoke(Action a)
        {
            if (InvokeRequired) BeginInvoke(a);
            else a();
        }

        private void ShowErrorMsg(string message, int error = 0)
        {
            string errorMsg = (error == 0) ? message : (message + ": Error =" + string.Format("{0:X}", error));
            switch (error)
            {
                case MyCamera.MV_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: errorMsg += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg += " Precondition error or environment changed "; break;
                case MyCamera.MV_E_VERSION: errorMsg += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MyCamera.MV_E_BUSY: errorMsg += " Device busy or network disconnected "; break;
                case MyCamera.MV_E_NETER: errorMsg += " Network error "; break;
            }
            MessageBox.Show(errorMsg, "PROMPT");
        }

        // ------------- Sidebar -------------

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            _sidebarExpand = !_sidebarExpand;
            tableLayoutPanel1.ColumnStyles[1].Width = 50;
            sidebarTimer.Start();
        }

        private void showSidebarBtn_Click(object sender, EventArgs e)
        {
            _sidebarExpand = !_sidebarExpand;
            tableLayoutPanel1.ColumnStyles[1].Width = 0;
            sidebarTimer.Start();
        }

        private void sidebarTimer_Tick(object sender, EventArgs e)
        {
            float current = tableLayoutPanel1.ColumnStyles[0].Width;
            float target = _sidebarExpand ? _expandedWidth : CollapsedWidth;

            if (Math.Abs(current - target) < Step)
            {
                tableLayoutPanel1.ColumnStyles[0].Width = target;
                sidebarTimer.Stop();
            }
            else
            {
                tableLayoutPanel1.ColumnStyles[0].Width += (current < target) ? Step : -Step;
            }
        }

        // ------------- Camera / Device -------------

        private void DeviceListAcq()
        {
            try
            {
                GC.Collect();
                cbDeviceList.Items.Clear();
                _deviceList.nDeviceNum = 0;

                int nRet = MyCamera.MV_CC_EnumDevices_NET(
                    MyCamera.MV_GIGE_DEVICE |
                    MyCamera.MV_USB_DEVICE |
                    MyCamera.MV_GENTL_GIGE_DEVICE |
                    MyCamera.MV_GENTL_CAMERALINK_DEVICE |
                    MyCamera.MV_GENTL_CXP_DEVICE |
                    MyCamera.MV_GENTL_XOF_DEVICE,
                    ref _deviceList);

                if (nRet != 0)
                {
                    ShowErrorMsg("Enumerate devices fail!", 0);
                    return;
                }

                for (int i = 0; i < _deviceList.nDeviceNum; i++)
                {
                    var device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(
                        _deviceList.pDeviceInfo[i],
                        typeof(MyCamera.MV_CC_DEVICE_INFO));

                    AppendDeviceToCombo(device);
                }

                if (_deviceList.nDeviceNum != 0)
                    cbDeviceList.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                ShowErrorMsg("Device enumeration error: " + ex.Message, 0);
            }
        }

        private void AppendDeviceToCombo(MyCamera.MV_CC_DEVICE_INFO device)
        {
            string item = null;

            if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                var gige = (MyCamera.MV_GIGE_DEVICE_INFO_EX)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO_EX));
                item = "GEV: " + GetDeviceDisplayName(gige.chUserDefinedName, gige.chManufacturerName, gige.chModelName) + " (" + gige.chSerialNumber + ")";
            }
            else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
            {
                var usb = (MyCamera.MV_USB3_DEVICE_INFO_EX)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO_EX));
                item = "U3V: " + GetDeviceDisplayName(usb.chUserDefinedName, usb.chManufacturerName, usb.chModelName) + " (" + usb.chSerialNumber + ")";
            }
            else if (device.nTLayerType == MyCamera.MV_GENTL_CAMERALINK_DEVICE)
            {
                var cml = (MyCamera.MV_CML_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stCMLInfo, typeof(MyCamera.MV_CML_DEVICE_INFO));
                item = "CML: " + GetDeviceDisplayName(cml.chUserDefinedName, cml.chManufacturerInfo, cml.chModelName) + " (" + cml.chSerialNumber + ")";
            }
            else if (device.nTLayerType == MyCamera.MV_GENTL_CXP_DEVICE)
            {
                var cxp = (MyCamera.MV_CXP_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stCXPInfo, typeof(MyCamera.MV_CXP_DEVICE_INFO));
                item = "CXP: " + GetDeviceDisplayName(cxp.chUserDefinedName, cxp.chManufacturerInfo, cxp.chModelName) + " (" + cxp.chSerialNumber + ")";
            }
            else if (device.nTLayerType == MyCamera.MV_GENTL_XOF_DEVICE)
            {
                var xof = (MyCamera.MV_XOF_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stXoFInfo, typeof(MyCamera.MV_XOF_DEVICE_INFO));
                item = "XOF: " + GetDeviceDisplayName(xof.chUserDefinedName, xof.chManufacturerInfo, xof.chModelName) + " (" + xof.chSerialNumber + ")";
            }

            if (!string.IsNullOrEmpty(item))
                cbDeviceList.Items.Add(item);
        }

        private string GetDeviceDisplayName(byte[] userDefinedNameBytes, string manufacturer, string model)
        {
            string udf = "";
            if (userDefinedNameBytes != null && userDefinedNameBytes.Length > 0 && userDefinedNameBytes[0] != '\0')
            {
                udf = MyCamera.IsTextUTF8(userDefinedNameBytes)
                    ? Encoding.UTF8.GetString(userDefinedNameBytes).TrimEnd('\0')
                    : Encoding.Default.GetString(userDefinedNameBytes).TrimEnd('\0');

                udf = DeleteTail(udf);
            }
            return string.IsNullOrEmpty(udf) ? (manufacturer + " " + model) : udf;
        }

        private string DeleteTail(string s)
        {
            s = Regex.Unescape(s);
            int idx = s.IndexOf("\0");
            return (idx >= 0) ? s.Remove(idx) : s;
        }

        private bool IsMono(uint pixelType)
        {
            switch (pixelType)
            {
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono1p:
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono2p:
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono4p:
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8_Signed:
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono14:
                case (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono16:
                    return true;
                default:
                    return false;
            }
        }

        public Int32 NecessaryOperBeforeGrab()
        {
            if (isDebug.Checked)
            {
                Console.WriteLine("[DEBUG] Skipping SDK read, using simulated parameters...");

                UInt32 fakeWidth = 1920;
                UInt32 fakeHeight = 1080;
                UInt32 fakePixelType = (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;

                _bitmapPixelFormat = PixelFormat.Format8bppIndexed;
                _convertDstBufLen = fakeWidth * fakeHeight;
                _convertDstBuf = Marshal.AllocHGlobal((Int32)_convertDstBufLen);
                _bitmap = new Bitmap((Int32)fakeWidth, (Int32)fakeHeight, _bitmapPixelFormat);

                // Apply grayscale palette
                ColorPalette palette = _bitmap.Palette;
                for (int i = 0; i < palette.Entries.Length; i++)
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                _bitmap.Palette = palette;

                Console.WriteLine($"[DEBUG] Fake buffer created {fakeWidth}x{fakeHeight}, Mono8");
                return MyCamera.MV_OK;
            }
            // ch:取图像宽 | en:Get Image Width
            MyCamera.MVCC_INTVALUE_EX stWidth = new MyCamera.MVCC_INTVALUE_EX();
            int nRet = _cam.GetIntValueEx("Width", ref stWidth);
            if (MyCamera.MV_OK != nRet)
            {
                Error?.Invoke("Get Width Info Fail!");
                return nRet;
            }

            // ch:取图像高 | en:Get Image Height
            MyCamera.MVCC_INTVALUE_EX stHeight = new MyCamera.MVCC_INTVALUE_EX();
            nRet = _cam.GetIntValueEx("Height", ref stHeight);
            if (MyCamera.MV_OK != nRet)
            {
                Error?.Invoke("Get Height Info Fail!");
                return nRet;
            }

            // ch:取像素格式 | en:Get Pixel Format
            MyCamera.MVCC_ENUMVALUE stPixelFormat = new MyCamera.MVCC_ENUMVALUE();
            nRet = _cam.GetEnumValue("PixelFormat", ref stPixelFormat);
            if (MyCamera.MV_OK != nRet)
            {
                Error?.Invoke("Get Pixel Format Fail!");
                return nRet;
            }

            // ch:设置bitmap像素格式，申请相应大小内存 | en:Set Bitmap Pixel Format, alloc memory
            if ((Int32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined == (Int32)stPixelFormat.nCurValue)
            {
                Error?.Invoke("Unknown Pixel Format!");
                return MyCamera.MV_E_UNKNOW;
            }
            else if (IsMono(stPixelFormat.nCurValue))
            {
                _bitmapPixelFormat = PixelFormat.Format8bppIndexed;

                if (IntPtr.Zero != _convertDstBuf)
                {
                    Marshal.Release(_convertDstBuf);
                    _convertDstBuf = IntPtr.Zero;
                }

                // Mono8为单通道
                _convertDstBufLen = (UInt32)(stWidth.nCurValue * stHeight.nCurValue);
                _convertDstBuf = Marshal.AllocHGlobal((Int32)_convertDstBufLen);
                if (IntPtr.Zero == _convertDstBuf)
                {
                    Error?.Invoke("Malloc Memory Fail!");
                    return MyCamera.MV_E_RESOURCE;
                }
            }
            else
            {
                _bitmapPixelFormat = PixelFormat.Format24bppRgb;

                if (IntPtr.Zero != _convertDstBuf)
                {
                    Marshal.FreeHGlobal(_convertDstBuf);
                    _convertDstBuf = IntPtr.Zero;
                }

                // RGB为三通道
                _convertDstBufLen = (UInt32)(3 * stWidth.nCurValue * stHeight.nCurValue);
                _convertDstBuf = Marshal.AllocHGlobal((Int32)_convertDstBufLen);
                if (IntPtr.Zero == _convertDstBuf)
                {
                    Error?.Invoke("Malloc Memory Fail!");
                    return MyCamera.MV_E_RESOURCE;
                }
            }

            // 确保释放保存了旧图像数据的bitmap实例，用新图像宽高等信息new一个新的bitmap实例
            if (null != _bitmap)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
            _bitmap = new Bitmap((Int32)stWidth.nCurValue, (Int32)stHeight.nCurValue, _bitmapPixelFormat);

            // ch:Mono8格式，设置为标准调色板 | en:Set Standard Palette in Mono8 Format
            if (PixelFormat.Format8bppIndexed == _bitmapPixelFormat)
            {
                ColorPalette palette = _bitmap.Palette;
                for (int i = 0; i < palette.Entries.Length; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
                _bitmap.Palette = palette;
            }

            return MyCamera.MV_OK;
        }

        private void OnError(string msg)
        {
            var handler = Error;
            if (handler != null) handler(msg);
        }

        // Event to signal when DAG execution is ready to receive triggers
        private TaskCompletionSource<bool> _dagReadyTcs = new TaskCompletionSource<bool>();
        
        // Add method to restart DAG execution
        private async Task RestartDagExecutionAsync()
        {
            try
            {
                // Cancel any existing flow execution
                _flowCts?.Cancel();
                
                // Create new cancellation token source
                _flowCts = new CancellationTokenSource();
                
                // Reset the TaskCompletionSource for this execution
                var newTcs = new TaskCompletionSource<bool>();
                var oldTcs = Interlocked.Exchange(ref _dagReadyTcs, newTcs);
                oldTcs?.TrySetCanceled(); // Cancel any pending waits
                
                // Load DAG definition
                var dag = DagFlowLoader.LoadJson("inspectionflow.json");
                var registry = _provider.GetRequiredService<IActionRegistry>();
                
                // Create new executor
                _executor = new DagExecutor(registry, _ctx);
                
                // Start DAG execution in background
                _dagExecutionTask = Task.Run(async () =>
                {
                    try
                    {
                        await _executor.RunAsync(dag, _flowCts.Token, 4);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancellation is requested
                        _dagReadyTcs.TrySetCanceled();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in DAG execution: {ex.Message}");
                        _dagReadyTcs.TrySetException(ex);
                    }
                });
                
                // Give the DAG execution a moment to start
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restarting DAG execution: {ex.Message}");
                throw;
            }
        }

        // ------------- Buttons / Menu -------------

        private async void openCamera_Click(object sender, EventArgs e)
        {
            if (_deviceList.nDeviceNum == 0 || cbDeviceList.SelectedIndex == -1)
            {
                ShowErrorMsg("No device, please select", 0);
                return;
            }

            var device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(
                _deviceList.pDeviceInfo[cbDeviceList.SelectedIndex],
                typeof(MyCamera.MV_CC_DEVICE_INFO));

            // Buka baru (handle fresh)
            if (!_cam.Open(device))
            {
                ShowErrorMsg("Failed to open device", 0);
                return;
            }
        }

        private async void btnStartGrab_Click(object sender, EventArgs e)
        {
            int nRet = NecessaryOperBeforeGrab();
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Necessary operations before grab failed", nRet);
                return;
            }

            _grabbing = true;

            tableLayoutPanel9.RowCount = 1;
            pictureBox5.Dock = DockStyle.Fill;
            pictureBox5.Margin = new Padding(0);
            pictureBox5.Padding = new Padding(0);
            labelCameraInspection.Visible = false;

            if (!isDebug.Checked)
            {
                //await Host.FireAsync("SignalRead");
                //SubscribePlcSignal();
            }

            if (MyCamera.MV_OK != nRet)
            {
                //pictureBox5.Visible = true;
                labelCameraInspection.Visible = true;
                _grabbing = false;
                ShowErrorMsg("Start Grabbing Fail!", nRet);
            }

            // Restart DAG execution when starting camera
            await RestartDagExecutionAsync();

            // Small delay to ensure DAG execution has started and is waiting for trigger
            await Task.Delay(100);

            _ctx.Trigger = "CAMERA_STARTED";
            pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void stopCamera_Click(object sender, EventArgs e)
        {
            _grabbing = false;          // flag UI
            _cam.Close();               // <-- TUTUP TOTAL

            // Cancel DAG execution
            _flowCts?.Cancel();

            tableLayoutPanel9.RowCount = 2;
            labelCameraInspection.Visible = true;
        }


        private void cameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new Form2()) { dlg.StartPosition = FormStartPosition.CenterParent; dlg.ShowDialog(this); }
        }

        private void mESToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new MESDialog()) { dlg.StartPosition = FormStartPosition.CenterParent; dlg.ShowDialog(this); }
        }

        private void pathLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new PathLogDialog()) { dlg.StartPosition = FormStartPosition.CenterParent; dlg.ShowDialog(this); }
        }

        private void gRPCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new DialogDebugMenuGrpc().Show();
        }

        private void pLCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new DialogDebugMenuPlc().Show();
        }

        private void databaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new DatabaseDialog(_provider);
            dialog.Show();
        }

        private void pLCToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var dialog = new PlcDialog(_provider);
            dialog.Show();
        }

        // ------------- Preview / Debug -------------

        public void OnFramePreview(Bitmap bmp)
        {
            SafeInvoke(delegate { pictureBox5.Image = bmp; });
        }

        private void StartDebugPreviewLoop()
        {
            NecessaryOperBeforeGrab();

            labelCameraInspection.Visible = false;
            tableLayoutPanel12.RowStyles[1].Height = 0;
            tableLayoutPanel12.Margin = new Padding(0);
            pictureBox5.Dock = DockStyle.Fill;
            pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox5.Margin = new Padding(0);
            pictureBox5.Padding = new Padding(0);

            if (_ctsPreview != null)
            {
                _ctsPreview.Cancel();
                _ctsPreview.Dispose();
                _ctsPreview = null;
                return;
            }

            _ctsPreview = new CancellationTokenSource();
            var grab = VideoGrabberService.Instance;

            grab.FramePreview += b => SafeInvoke(delegate { pictureBox5.Image = (Bitmap)b.Clone(); });

            _ = Task.Run(async delegate
            {
                try
                {
                    while (!_ctsPreview.IsCancellationRequested)
                    {
                        if (GrpcResponseStore.LastResponse != null)
                        {
                            RenderComponentsUI(GrpcResponseStore.LastResponse.Result, flowLayoutPanel1);
                        }
                        resultInspectionLabel1.Text = "Debug Preview";
                        tableLayoutPanel9.RowCount = 1;
                        await Task.Delay(500, _ctsPreview.Token);
                    }
                }
                catch (OperationCanceledException) { }
            });
        }
        private async Task CaptureAndProcessOnceAsync()
        {
            // Wait one frame from CameraManager
            var tcs = new TaskCompletionSource<byte[]>();
            System.Action<byte[]> handler = null;
            handler = (b) => { if (b != null && b.Length > 0) tcs.TrySetResult(b); };
            _cam.FrameReadyForGrpc += handler;

            byte[] frameBytes = null;
            try { frameBytes = await tcs.Task; }
            finally { _cam.FrameReadyForGrpc -= handler; }

            // Persist & index
            string imageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            string fileName = "frame_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + imageId + ".bmp";
            //await _fileUtils.SaveFrameToDiskAsync(frameBytes, fileName, imageId);
            //_imageDb.InsertImage(fileName, imageId);

            // Process via gRPC → render
            await _grpc.ProcessImageAsync(frameBytes, default(System.Threading.CancellationToken), imageId);
            if (GrpcResponseStore.LastResponse != null)
                RenderComponentsUI(GrpcResponseStore.LastResponse.Result, flowLayoutPanel1);

            if (!isDebug.Checked) MoveToFrame();
        }

        private void MoveToFrame()
        {
            if (GrpcResponseStore.LastResponse == null ||
                string.IsNullOrEmpty(GrpcResponseStore.LastResponse.Result)) return;

            var result = JsonConvert.DeserializeObject<Root>(GrpcResponseStore.LastResponse.Result);
            if (result == null) return;

            //if (result.FinalLabel)
            //    PlcDialog.PlcHelper.SendCommand(WritePLCAddress.FAIL);
            //else
            //    PlcDialog.PlcHelper.SendCommand(WritePLCAddress.PASS);
        }

        // ------------- Reconnect -------------

        private void ScheduleReconnect()
        {
            if (_reconnectTimer != null && !_reconnectTimer.Enabled)
                _reconnectTimer.Start();
        }

        private async void TryGrpcReconnect()
        {
            try
            {
                if (_grpc == null) return;
                var ok = await _grpc.StartAsync();
                if (ok && _reconnectTimer != null) _reconnectTimer.Stop();
            }
            catch { /* keep retrying */ }
        }

        // ------------- UI Rendering (unchanged logic, cleaned) -------------

        public void RenderComponentsUI(string json, Control parent)
        {
            var result = JsonConvert.DeserializeObject<Root>(GrpcResponseStore.LastResponse.Result);
            if (result == null || result.Components == null) return;

            if (componentResultInspectionRadioButton.Checked)
            {
                BuildUiFirstTimeComponentInspection(result, flowLayoutPanel1);
                UpdateUiInspectionResultComponentInspection(result, flowLayoutPanel1);
            }
            else
            {
                BuildUiFirstTime(result, flowLayoutPanel1);
                UpdateUiInspectionResult(result, flowLayoutPanel1);
            }
        }

        private void UpdateUiInspectionResult(Root data, Control parent)
        {
            if (data == null || data.Components == null) return;
            parent.SuspendLayout();
            Console.WriteLine("update ui");

            foreach (var kv in data.Components)
            {
                Console.WriteLine(kv.Key);
                string key = kv.Key;

                var imgBox = parent.Controls.Find("imgBox_" + key, true).FirstOrDefault() as PictureBox;
                var resultLbl = parent.Controls.Find("resultLbl_" + key, true).FirstOrDefault() as Label;
                var parrot = parent.Controls.Find("parrot_" + key, true).FirstOrDefault() as ReaLTaiizor.Controls.ParrotWidgetPanel;
                var innerTable = parent.Controls.Find("innerTable_" + key, true).FirstOrDefault() as TableLayoutPanel;

                if (imgBox == null || resultLbl == null || parrot == null || innerTable == null) continue;

                var item = kv.Value.FirstOrDefault();
                if (item == null) continue;

                var fileImage = _fileUtils.GetLatestImage();
                Console.WriteLine(fileImage);
                if (string.IsNullOrEmpty(fileImage) || !File.Exists(fileImage)) continue;

                Bitmap srcBmp;
                using (var ms = new MemoryStream(File.ReadAllBytes(fileImage)))
                    srcBmp = new Bitmap(ms);

                Bitmap boxed = null;
                var all = new List<int>();
                foreach (var v in kv.Value)
                {
                    if (v.Boxes != null && v.Boxes.Count % 4 == 0) all.AddRange(v.Boxes);
                }

                if (all.Count > 0)
                {
                    try { boxed = _imageUtil.DrawBoundingBoxMultiple(srcBmp, all); }
                    catch (Exception ex) { Console.WriteLine("[DrawBoxes] " + ex.Message); }
                }

                imgBox.SizeMode = PictureBoxSizeMode.StretchImage;
                imgBox.Margin = new Padding(0);
                imgBox.Image = boxed;
                innerTable.RowCount = 1;

                srcBmp.Dispose();

                resultLbl.Text = item.Label ? "NG" : "Good";
                parrot.BackColor = (!item.Label) ? Color.FromArgb(4, 194, 55) : Color.FromArgb(192, 0, 0);

                var dummy = innerTable.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Result Inspection");
                if (dummy != null && dummy.Visible) dummy.Visible = false;

                var valLbl = innerTable.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "valLbl_" + key);
                if (string.IsNullOrEmpty(item.Value))
                {
                    if (valLbl != null) valLbl.Visible = false;
                }
                else
                {
                    if (valLbl == null)
                    {
                        valLbl = new Label { Name = "valLbl_" + key, ForeColor = Color.White, AutoSize = true };
                        innerTable.Controls.Add(valLbl);
                    }
                    valLbl.Text = "Value: " + item.Value;
                    valLbl.Visible = true;
                }
            }

            var final = parent.Controls.Find("finalLabel", true).FirstOrDefault() as Label;
            if (final != null) final.Text = "Final Label: " + data.FinalLabel;

            parent.ResumeLayout(true);
        }

        private void UpdateUiInspectionResultComponentInspection(Root data, Control parent)
        {
            if (data == null || data.Components == null) return;
            parent.SuspendLayout();
            int index = 1;

            foreach (var kv in data.Components)
            {
                foreach (var value in kv.Value)
                {
                    int key = index;

                    var imgBox = parent.Controls.Find("imgBox_" + key, true).FirstOrDefault() as PictureBox;
                    var resultLbl = parent.Controls.Find("resultLbl_" + key, true).FirstOrDefault() as Label;
                    var parrot = parent.Controls.Find("parrot_" + key, true).FirstOrDefault() as ReaLTaiizor.Controls.ParrotWidgetPanel;
                    var innerTable = parent.Controls.Find("innerTable_" + key, true).FirstOrDefault() as TableLayoutPanel;

                    if (imgBox == null || resultLbl == null || parrot == null || innerTable == null) { index++; continue; }

                    var item = kv.Value.FirstOrDefault();
                    if (item == null) { index++; continue; }

                    var fileImage = _fileUtils.GetLatestImage();
                    if (string.IsNullOrEmpty(fileImage) || !File.Exists(fileImage)) { index++; continue; }

                    Bitmap srcBmp;
                    using (var ms = new MemoryStream(File.ReadAllBytes(fileImage)))
                        srcBmp = new Bitmap(ms);

                    Bitmap boxed = _imageUtil.CropAndDrawBoundingBoxes(srcBmp, value.Boxes);

                    imgBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    imgBox.Margin = new Padding(0);
                    imgBox.Image = boxed;
                    innerTable.RowCount = 1;

                    srcBmp.Dispose();

                    resultLbl.Text = item.Label ? "NG" : "Good";
                    parrot.BackColor = (!item.Label) ? Color.FromArgb(4, 194, 55) : Color.FromArgb(192, 0, 0);

                    var dummy = innerTable.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Result Inspection");
                    if (dummy != null && dummy.Visible) dummy.Visible = false;

                    var valLbl = innerTable.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "valLbl_" + key);
                    if (string.IsNullOrEmpty(item.Value))
                    {
                        if (valLbl != null) valLbl.Visible = false;
                    }
                    else
                    {
                        if (valLbl == null)
                        {
                            valLbl = new Label { Name = "valLbl_" + key, ForeColor = Color.White, AutoSize = true };
                            innerTable.Controls.Add(valLbl);
                        }
                        valLbl.Text = "Value: " + item.Value;
                        valLbl.Visible = true;
                    }

                    index++;
                }
            }

            var final = parent.Controls.Find("finalLabel", true).FirstOrDefault() as Label;
            if (final != null) final.Text = "Final Label: " + data.FinalLabel;

            parent.ResumeLayout(true);
        }

        private void BuildUiFirstTimeComponentInspection(Root data, FlowLayoutPanel parent)
        {
            parent.Controls.Clear();
            int index = 1;

            foreach (var kv in data.Components)
            {
                foreach (var _ in kv.Value)
                {
                    int key = index;

                    var panel = new Panel
                    {
                        Name = "panel_" + key,
                        Size = new System.Drawing.Size(185, 186),
                        BackColor = Color.Transparent,
                        Margin = new Padding(0, 0, 20, 20)
                    };

                    var outer = new TableLayoutPanel
                    {
                        ColumnCount = 1,
                        RowCount = 2,
                        Dock = DockStyle.Fill,
                        Padding = new Padding(5),
                        BackColor = Color.FromArgb(117, 120, 123)
                    };
                    outer.RowStyles.Add(new RowStyle(SizeType.Percent, 82.38636F));
                    outer.RowStyles.Add(new RowStyle(SizeType.Percent, 17.61364F));
                    outer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 169F));
                    panel.Controls.Add(outer);

                    var inner = new TableLayoutPanel
                    {
                        Name = "innerTable_" + key,
                        ColumnCount = 1,
                        RowCount = 2,
                        Dock = DockStyle.Fill,
                        BackColor = Color.Black
                    };
                    inner.RowStyles.Add(new RowStyle(SizeType.Percent, 75F));
                    inner.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

                    var imgBox = new PictureBox
                    {
                        Name = "imgBox_" + key,
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Image = Properties.Resources.NoImage
                    };
                    inner.Controls.Add(imgBox, 0, 0);

                    var labelDummy = new Label
                    {
                        Text = "Result Inspection",
                        ForeColor = Color.White,
                        Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold),
                        Anchor = AnchorStyles.None,
                        AutoSize = true
                    };
                    inner.Controls.Add(labelDummy, 0, 1);

                    outer.Controls.Add(inner, 0, 0);

                    var parrot = new ReaLTaiizor.Controls.ParrotWidgetPanel
                    {
                        Name = "parrot_" + key,
                        Dock = DockStyle.Fill,
                        BackColor = Color.FromArgb(192, 0, 0),
                        ControlsAsWidgets = false
                    };
                    outer.Controls.Add(parrot, 0, 1);

                    var resultTable = new TableLayoutPanel
                    {
                        ColumnCount = 1,
                        RowCount = 1,
                        Dock = DockStyle.Fill,
                        AutoSize = false
                    };
                    resultTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    resultTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                    parrot.Controls.Add(resultTable);

                    var resultLabel = new Label
                    {
                        Name = "resultLbl_" + key,
                        Text = "Result Inspection",
                        Dock = DockStyle.Fill,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    resultTable.Controls.Add(resultLabel);

                    parent.Controls.Add(panel);
                    index++;
                }
            }

            var finalLabel = new Label
            {
                Name = "finalLabel",
                Text = "Final Label: " + data.FinalLabel,
                Font = new Font("Consolas", 9, FontStyle.Italic),
                ForeColor = Color.Blue,
                AutoSize = true
            };
            parent.Controls.Add(finalLabel);
        }

        private void BuildUiFirstTime(Root data, FlowLayoutPanel parent)
        {
            Console.WriteLine("build ui firsttime");
            parent.Controls.Clear();

            foreach (var kv in data.Components)
            {
                string key = kv.Key;
                Console.WriteLine(key);
                var panel = new Panel
                {
                    Name = "panel_" + key,
                    Size = new System.Drawing.Size(185, 186),
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 20, 20)
                };

                var outer = new TableLayoutPanel
                {
                    ColumnCount = 1,
                    RowCount = 2,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(5),
                    BackColor = Color.FromArgb(117, 120, 123)
                };
                outer.RowStyles.Add(new RowStyle(SizeType.Percent, 82.38636F));
                outer.RowStyles.Add(new RowStyle(SizeType.Percent, 17.61364F));
                outer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 169F));
                panel.Controls.Add(outer);

                var inner = new TableLayoutPanel
                {
                    Name = "innerTable_" + key,
                    ColumnCount = 1,
                    RowCount = 2,
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black
                };
                inner.RowStyles.Add(new RowStyle(SizeType.Percent, 75F));
                inner.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

                var imgBox = new PictureBox
                {
                    Name = "imgBox_" + key,
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Properties.Resources.NoImage
                };
                inner.Controls.Add(imgBox, 0, 0);

                var labelDummy = new Label
                {
                    Text = "Result Inspection",
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold),
                    Anchor = AnchorStyles.None,
                    AutoSize = true
                };
                inner.Controls.Add(labelDummy, 0, 1);

                outer.Controls.Add(inner, 0, 0);

                var parrot = new ReaLTaiizor.Controls.ParrotWidgetPanel
                {
                    Name = "parrot_" + key,
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(192, 0, 0),
                    ControlsAsWidgets = false
                };
                outer.Controls.Add(parrot, 0, 1);

                var resultTable = new TableLayoutPanel
                {
                    ColumnCount = 1,
                    RowCount = 1,
                    Dock = DockStyle.Fill,
                    AutoSize = false
                };
                resultTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                resultTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                parrot.Controls.Add(resultTable);

                var resultLabel = new Label
                    {
                        Name = "resultLbl_" + key,
                        Text = "Result Inspection",
                        Dock = DockStyle.Fill,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    resultTable.Controls.Add(resultLabel);

                parent.Controls.Add(panel);
            }

            var final = new Label
            {
                Name = "finalLabel",
                Text = "Final Label: " + data.FinalLabel,
                Font = new Font("Consolas", 9, FontStyle.Italic),
                ForeColor = Color.Blue,
                AutoSize = true
            };
            parent.Controls.Add(final);
        }

        // ------------- Misc buttons -------------

        private void button2_Click(object sender, EventArgs e)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Logs");
            string filePath = Path.Combine(folder, "Inspection_20251010_113839.json");
            LoadJsonToTable(filePath);
        }

        private void LoadJsonToTable(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("File JSON tidak ditemukan: " + filePath);
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var summary = JsonConvert.DeserializeObject<CycleTimeSummary>(json);

                if (summary == null || summary.Logs == null || summary.Logs.Count == 0)
                {
                    MessageBox.Show("Tidak ada log untuk ditampilkan.");
                    return;
                }

                dataGridView1.DataSource = summary.Logs;
                dataGridView1.Columns["TransactionId"].HeaderText = "ID Transaksi";
                dataGridView1.Columns["Timestamp"].HeaderText = "Waktu";
                dataGridView1.Columns["CycleTimeMs"].HeaderText = "Cycle Time (ms)";
                dataGridView1.Columns["RawResponse"].HeaderText = "Response";
                dataGridView1.Columns["Pass"].HeaderText = "Pass/Fail";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load JSON: " + ex.Message);
            }
        }

        private void onShowGeneralMenu(object sender, EventArgs e) { if (parrotWidgetPanel2.Visible) { pictureBox1.Image = Properties.Resources.chevron_right; } else { pictureBox1.Image = Properties.Resources.chevron_down; } parrotWidgetPanel2.Visible = !parrotWidgetPanel2.Visible; }
        private void onShowCameraMenu(object sender, EventArgs e) { if (parrotWidgetPanel4.Visible) { pictureBox7.Image = Properties.Resources.chevron_right; } else { pictureBox7.Image = Properties.Resources.chevron_down; } parrotWidgetPanel4.Visible = !parrotWidgetPanel4.Visible; }

        private async void panel8_Click(object sender, EventArgs e)
        {
            await CaptureAndProcessOnceAsync();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await CaptureAndProcessOnceAsync();
        }

        // Events required by original code
        public event Action<byte[]> FrameEncoded;
        public event Action<string> Error;

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var dialog = new DiagramConfigurationForm();

            dialog.Show();
        }
    }
}
