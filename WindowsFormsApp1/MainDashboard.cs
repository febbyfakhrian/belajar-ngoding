using Api;
using AutoInspectionPlatform;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using PLCCommunication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Helpers;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Services;
using static System.Net.Mime.MediaTypeNames;
using static WindowsFormsApp1.VideoGrabber;
using Label = System.Windows.Forms.Label;
using Panel = System.Windows.Forms.Panel;

namespace WindowsFormsApp1
{
    public partial class MainDashboard : Form
    {
        private ConcurrentQueue<byte[]> _frameQueue => _cam.Queue;
        private readonly CameraManager _cam = new CameraManager(new ConcurrentQueue<byte[]>());
        private readonly GrpcService _grpc = new GrpcService();

        // SDK objects kept only for enum / info
        private MyCamera.MV_CC_DEVICE_INFO_LIST _deviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        private MyCamera _sdkCamera = new MyCamera();
        private bool _grabbing = false;
        private Thread _grabThread;

        public const Int32 CUSTOMER_PIXEL_FORMAT = unchecked((Int32)0x80000000);

        private IntPtr _convertDstBuf = IntPtr.Zero;
        private uint _convertDstBufLen = 0;
        Bitmap _bitmap = null;
        private PixelFormat _bitmapPixelFormat = PixelFormat.DontCare;

        public event Action<byte[]> FrameEncoded;
        public event Action<string> Error;
        private CancellationTokenSource _ctsForward = new CancellationTokenSource();
        private System.Timers.Timer _reconnectTimer;
        private const int RECONNECT_INTERVAL_MS = 3000;
        bool sidebarExpand;

        private const float CollapsedWidth = 15f;   // ukuran kecil (collapsed)
        private float ExpandedWidth;                // akan diisi saat form load
        private const float Step = 10f;
        private CancellationTokenSource _cts;
        private bool _uiBuilt = false;
        private FileUtils fileUtils = new FileUtils();
        private ImageDbOperation _imageDbOperation => Program.DbHelper;
        private ImageManipulationUtils imageManipulationUtils = new ImageManipulationUtils();

        private static int indexFrame = 0;

        public MainDashboard()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string dbPath = Path.Combine(folder, "inspection_platform.db");

            var _connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");

            _cam.Error += msg => MessageBox.Show(msg);
            //_cam.FrameEncoded += _ => Task.Run(ForwardLoop);
            //_grpc.BoxesReceived += json =>
            //{
            //    var data = System.Text.Json.JsonSerializer.Deserialize<ResultDTO>(json);

            //    // Clear old boxes
            //    _cam.Boxes.Clear();

            //    if (data?.Boxes != null)
            //    {
            //        Console.WriteLine("=== Raw Boxes ===");
            //        foreach (var box in data.Boxes)
            //        {
            //            Console.WriteLine($"[{box[0]}, {box[1]}, {box[2]}, {box[3]}]");

            //            _cam.Boxes.Add(new SKRect(
            //                box[0], box[1],           // left, top
            //                box[2], box[3]));         // right, bottom
            //        }
            //    }
            //};

            _grpc.OnDisconnected += ScheduleReconnect;
            _reconnectTimer = new System.Timers.Timer(RECONNECT_INTERVAL_MS)
            {
                AutoReset = false
            };
            _reconnectTimer.Elapsed += async (_, __) => await TryReconnectAsync();

            _ = Task.Run(() => _cam.Start());
        }

        private void ScheduleReconnect()
        {
            if (!_reconnectTimer.Enabled)
                _reconnectTimer.Start();
        }

        private async Task TryReconnectAsync()
        {
            Console.WriteLine("[gRPC] reconnecting...");
            //await _grpc.ReconnectAsync();
        }


        private async Task ForwardLoop()
        {
            while (true)
            {
                if (_frameQueue.TryDequeue(out var jpeg))
                {
                    await _grpc.ProcessImageAsync(jpeg); // akan return cepat kalau _streamAlive=false
                }
                else
                {
                    await Task.Delay(5);
                }
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            DeviceListAcq();

            bool ok = await _grpc.StartAsync();
            if (!ok)
            {
                MessageBox.Show("Gagal koneksi ke Python server 50052");
                return;
            }

            // 1. Kirim frame ke gRPC
            showSidebarBtn.Visible = true;
            showSidebarBtn.BringToFront();

            tableLayoutPanel9.GetType()
              .GetProperty("DoubleBuffered",
                            System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.NonPublic)
              .SetValue(tableLayoutPanel9, true);

            _ = Task.Run(() => _cam.Start());
        }


        //private void button1_Click(object sender, EventArgs e)
        //{
        //    panelSubmenu.Visible = !panelSubmenu.Visible;
        //}

        //private void customizeDesign()
        //{
        //    panelSubmenu.Visible = false;
        //}

        private void onShowGeneralMenu(object sender, EventArgs e)
        {
            if (parrotWidgetPanel2.Visible)
            {
                pictureBox1.Image = Properties.Resources.chevron_right;
            }
            else
            {
                pictureBox1.Image = Properties.Resources.chevron_down;
            }

            parrotWidgetPanel2.Visible = !parrotWidgetPanel2.Visible;
        }

        private void onShowCameraMenu(object sender, EventArgs e)
        {
            if (parrotWidgetPanel4.Visible)
            {
                pictureBox7.Image = Properties.Resources.chevron_right;
            }
            else
            {
                pictureBox7.Image = Properties.Resources.chevron_down;
            }
            parrotWidgetPanel4.Visible = !parrotWidgetPanel4.Visible;
        }

        private void cameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new Form2())
            {
                dlg.StartPosition = FormStartPosition.CenterParent;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // Retrieve data from dlg and update parent UI
                }
            }
        }

        private void mESToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new MESDialog())
            {
                dlg.StartPosition = FormStartPosition.CenterParent;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {

                }
            }
        }

        private void pathLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new PathLogDialog())
            {
                dlg.StartPosition = FormStartPosition.CenterParent;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {

                }
            }
        }

        private string DeleteTail(string strUserDefinedName)
        {
            strUserDefinedName = Regex.Unescape(strUserDefinedName);
            int nIndex = strUserDefinedName.IndexOf("\0");
            if (nIndex >= 0)
            {
                strUserDefinedName = strUserDefinedName.Remove(nIndex);
            }

            return strUserDefinedName;
        }

        private void DeviceListAcq()
        {
            //ch: 创建设备列表 | en:Create Device List
            System.GC.Collect();
            cbDeviceList.Items.Clear();
            _deviceList.nDeviceNum = 0;
            //这里枚举了所有类型，根据实际情况，选择合适的枚举类型即可
            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE | MyCamera.MV_GENTL_GIGE_DEVICE
                | MyCamera.MV_GENTL_CAMERALINK_DEVICE | MyCamera.MV_GENTL_CXP_DEVICE | MyCamera.MV_GENTL_XOF_DEVICE, ref _deviceList);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!", 0);
                return;
            }

            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < _deviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(_deviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                string strUserDefinedName = "";
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO_EX gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO_EX)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO_EX));

                    if ((gigeInfo.chUserDefinedName.Length > 0) && (gigeInfo.chUserDefinedName[0] != '\0'))
                    {
                        if (MyCamera.IsTextUTF8(gigeInfo.chUserDefinedName))
                        {
                            strUserDefinedName = Encoding.UTF8.GetString(gigeInfo.chUserDefinedName).TrimEnd('\0');
                        }
                        else
                        {
                            strUserDefinedName = Encoding.Default.GetString(gigeInfo.chUserDefinedName).TrimEnd('\0');
                        }
                        cbDeviceList.Items.Add("GEV: " + DeleteTail(strUserDefinedName) + " (" + gigeInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbDeviceList.Items.Add("GEV: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_USB3_DEVICE_INFO_EX usbInfo = (MyCamera.MV_USB3_DEVICE_INFO_EX)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO_EX));

                    if ((usbInfo.chUserDefinedName.Length > 0) && (usbInfo.chUserDefinedName[0] != '\0'))
                    {
                        if (MyCamera.IsTextUTF8(usbInfo.chUserDefinedName))
                        {
                            strUserDefinedName = Encoding.UTF8.GetString(usbInfo.chUserDefinedName).TrimEnd('\0');
                        }
                        else
                        {
                            strUserDefinedName = Encoding.Default.GetString(usbInfo.chUserDefinedName).TrimEnd('\0');
                        }
                        cbDeviceList.Items.Add("U3V: " + DeleteTail(strUserDefinedName) + " (" + usbInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbDeviceList.Items.Add("U3V: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_GENTL_CAMERALINK_DEVICE)
                {
                    MyCamera.MV_CML_DEVICE_INFO CMLInfo = (MyCamera.MV_CML_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stCMLInfo, typeof(MyCamera.MV_CML_DEVICE_INFO));

                    if ((CMLInfo.chUserDefinedName.Length > 0) && (CMLInfo.chUserDefinedName[0] != '\0'))
                    {
                        if (MyCamera.IsTextUTF8(CMLInfo.chUserDefinedName))
                        {
                            strUserDefinedName = Encoding.UTF8.GetString(CMLInfo.chUserDefinedName).TrimEnd('\0');
                        }
                        else
                        {
                            strUserDefinedName = Encoding.Default.GetString(CMLInfo.chUserDefinedName).TrimEnd('\0');
                        }
                        cbDeviceList.Items.Add("CML: " + DeleteTail(strUserDefinedName) + " (" + CMLInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbDeviceList.Items.Add("CML: " + CMLInfo.chManufacturerInfo + " " + CMLInfo.chModelName + " (" + CMLInfo.chSerialNumber + ")");
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_GENTL_CXP_DEVICE)
                {
                    MyCamera.MV_CXP_DEVICE_INFO CXPInfo = (MyCamera.MV_CXP_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stCXPInfo, typeof(MyCamera.MV_CXP_DEVICE_INFO));

                    if ((CXPInfo.chUserDefinedName.Length > 0) && (CXPInfo.chUserDefinedName[0] != '\0'))
                    {
                        if (MyCamera.IsTextUTF8(CXPInfo.chUserDefinedName))
                        {
                            strUserDefinedName = Encoding.UTF8.GetString(CXPInfo.chUserDefinedName).TrimEnd('\0');
                        }
                        else
                        {
                            strUserDefinedName = Encoding.Default.GetString(CXPInfo.chUserDefinedName).TrimEnd('\0');
                        }
                        cbDeviceList.Items.Add("CXP: " + DeleteTail(strUserDefinedName) + " (" + CXPInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbDeviceList.Items.Add("CXP: " + CXPInfo.chManufacturerInfo + " " + CXPInfo.chModelName + " (" + CXPInfo.chSerialNumber + ")");
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_GENTL_XOF_DEVICE)
                {
                    MyCamera.MV_XOF_DEVICE_INFO XOFInfo = (MyCamera.MV_XOF_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stXoFInfo, typeof(MyCamera.MV_XOF_DEVICE_INFO));

                    if ((XOFInfo.chUserDefinedName.Length > 0) && (XOFInfo.chUserDefinedName[0] != '\0'))
                    {
                        if (MyCamera.IsTextUTF8(XOFInfo.chUserDefinedName))
                        {
                            strUserDefinedName = Encoding.UTF8.GetString(XOFInfo.chUserDefinedName).TrimEnd('\0');
                        }
                        else
                        {
                            strUserDefinedName = Encoding.Default.GetString(XOFInfo.chUserDefinedName).TrimEnd('\0');
                        }
                        cbDeviceList.Items.Add("XOF: " + DeleteTail(strUserDefinedName) + " (" + XOFInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbDeviceList.Items.Add("XOF: " + XOFInfo.chManufacturerInfo + " " + XOFInfo.chModelName + " (" + XOFInfo.chSerialNumber + ")");
                    }
                }
            }

            // ch:选择第一项 | en:Select the first item
            if (_deviceList.nDeviceNum != 0)
            {
                cbDeviceList.SelectedIndex = 0;
            }
        }


        // ch:显示错误信息 | en:Show error message
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            string errorMsg;
            if (nErrorNum == 0)
            {
                errorMsg = csMessage;
            }
            else
            {
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: errorMsg += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg += " Precondition error, or running environment changed "; break;
                case MyCamera.MV_E_VERSION: errorMsg += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MyCamera.MV_E_BUSY: errorMsg += " Device is busy, or network disconnected "; break;
                case MyCamera.MV_E_NETER: errorMsg += " Network error "; break;
            }

            MessageBox.Show(errorMsg, "PROMPT");
        }

        // ch:像素类型是否为Mono格式 | en:If Pixel Type is Mono 
        private Boolean IsMono(UInt32 enPixelType)
        {
            switch (enPixelType)
            {
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono1p:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono2p:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono4p:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8_Signed:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono14:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono16:
                    return true;
                default:
                    return false;
            }
        }


        // ch:取图前的必要操作步骤 | en:Necessary operation before grab
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

        private async void openCamera_Click(object sender, EventArgs e)
        {
            if (isDebug.Checked)
            {
                _cam.SetDisplayHandle(tableLayoutPanel9.Handle);
                NecessaryOperBeforeGrab();
                //pictureBox5.Visible = false;
                labelCameraInspection.Visible = false;
                //_cam.StartDebug();
                tableLayoutPanel12.RowStyles[1].Height = 0;
                tableLayoutPanel12.Margin = new Padding(0);
                pictureBox5.Dock = DockStyle.Fill;
                pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox5.Margin = new Padding(0);
                pictureBox5.Padding = new Padding(0);

                if (_cts != null) // sedang jalan → stop
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                    return;
                }

                var grab = VideoGrabberService.Instance;

                grab.FramePreview += bmp => pictureBox5.Image = (Bitmap)bmp.Clone();
                //grab.FrameEncoded += bytes => Console.WriteLine($"Encoded frame: {bytes.Length} bytes");

                _cts = new CancellationTokenSource();
                try
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        RenderComponentsUI(GrpcResponseStore.LastResponse.Result, flowLayoutPanel1);
                        resultInspectionLabel1.Text = "asdas" ?? string.Empty;
                        tableLayoutPanel9.RowCount = 1;


                        //if (!grabber._queue.TryDequeue(out byte[] jpegBytes))
                        //{
                        //    MessageBox.Show("Frame not found");
                        //    //return;
                        //}
                        //Console.WriteLine("run");

                        Console.WriteLine(GrpcResponseStore.LastResponse);
                        //var bmp = GenerateRandomBitmap(500, 500);

                        //// tentukan ukuran bounding box (contoh: 50% dari ukuran gambar)
                        //int boxW = (int)(bmp.Width * 0.5f);
                        //int boxH = (int)(bmp.Height * 0.5f);

                        //// gambar bounding box di tengah menggunakan SkiaSharp
                        ////Bitmap boxed = DrawBoundingBox(jpegBytes, boxW, boxW, boxH, boxH, new Scalar(50, 205, 50), strokeWidth: 4);

                        //// UpdateUI sekarang akan men-set ke PictureBox dan dispose 'boxed'
                        //UpdateUIV2(jpegBytes, GenerateRandomText(8));

                        await Task.Delay(500, _cts.Token); // kecepatan refresh
                    }
                }
                catch (OperationCanceledException) { /* normal exit */ }
            }
            else
            {
                if (_deviceList.nDeviceNum == 0 || cbDeviceList.SelectedIndex == -1)
                {
                    ShowErrorMsg("No device, please select", 0);
                    return;
                }

                var device = (MyCamera.MV_CC_DEVICE_INFO)
                    Marshal.PtrToStructure(_deviceList.pDeviceInfo[cbDeviceList.SelectedIndex],
                                           typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (!_cam.Open(device))
                {
                    ShowErrorMsg("Failed to open device", 0);
                }

                _cam.SetDisplayHandle(tableLayoutPanel9.Handle);
            }

        }

        private void UpdateUIV2(byte[] jpegBytes, string text)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => UpdateUIV2(jpegBytes, text)));
                return;
            }

            // update label
            resultInspectionLabel1.Text = text ?? string.Empty;
            tableLayoutPanel9.RowCount = 1;

            // safely dispose previous image
            if (pictureBox5.Image != null)
            {
                try
                {
                    var old = pictureBox5.Image;
                    pictureBox5.Image = null;
                    old.Dispose();
                }
                catch { /* ignore */ }
            }

            if (jpegBytes == null || jpegBytes.Length == 0)
                return;

            try
            {
                using (var ms = new MemoryStream(jpegBytes))
                using (var bmp = new Bitmap(ms))
                {
                    // clone image so we can dispose memory stream safely
                    pictureBox5.Image = (Bitmap)bmp.Clone();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Cannot load image from bytes: {ex.Message}");
            }
        }

        private void handleSignalFromPLC()
        {
            PlcDialog.PlcHelper.LineReceived += lineRaw => BeginInvoke(new Action(() =>
            {
                Console.WriteLine(lineRaw);
                if (string.IsNullOrWhiteSpace(lineRaw)) return;

                // normalisasi: hapus control-char, trim, upper
                string cmd = Encoding.ASCII.GetString(WritePLCAddress.READ).TrimEnd('\r', '\n');
                lineRaw = lineRaw.Replace("\\r", "\r").Replace("\\n", "\n");
                string lineClean = lineRaw.TrimEnd('\r', '\n');

                if (cmd.ToUpper().Equals(lineClean.ToUpper(), StringComparison.Ordinal))
                {
                    inspectFrame();
                }
            }));
        }

        private void MoveToFrame()
        {
            Root result = JsonConvert.DeserializeObject<Root>(GrpcResponseStore.LastResponse.Result);

            if (result.FinalLabel)
            {
                PlcDialog.PlcHelper.SendCommand(WritePLCAddress.FAIL);
            }
            else
            {
                PlcDialog.PlcHelper.SendCommand(WritePLCAddress.PASS);
            }
        }

        private async void inspectFrame()
        {
            do
            {
                var tcs = new TaskCompletionSource<byte[]>();

                _cam.FrameReadyForGrpc += bmp =>
                {
                    if (bmp != null && bmp.Length > 0)
                        tcs.TrySetResult(bmp);
                };

                // Tunggu sampai event menghasilkan frame
                byte[] frameBytes = await tcs.Task;
                string imageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                string fileNameFormat = $"frame_{DateTime.Now:yyyyMMdd_HHmmss}_{imageId}.bmp";

                await fileUtils.SaveFrameToDiskAsync(frameBytes, fileNameFormat, imageId);
                _imageDbOperation.InsertImage(fileNameFormat, imageId);

                // Kirim lewat gRPC
                ImageResponse resp = await _grpc.ProcessImageAsync(frameBytes, default, imageId);

                //var resultJson = JsonConvert.DeserializeObject<Root>(resp.Result);
                RenderComponentsUI(GrpcResponseStore.LastResponse.Result, flowLayoutPanel1);
            } while (indexFrame < 6);

            MoveToFrame();

            return;
        }


        private void btnStartGrab_Click(object sender, EventArgs e)
        {
            int nRet = NecessaryOperBeforeGrab();
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Necessary operations before grab failed", nRet);
                return;
            }

            IntPtr displayHandle = tableLayoutPanel9.Handle;

            // ch:标志位置true | en:Set position bit true
            _grabbing = true;

            // ch:开始采集 | en:Start Grabbing
            nRet = _cam.Start();

            pictureBox5.Visible = false;
            labelCameraInspection.Visible = false;

            handleSignalFromPLC();

            if (MyCamera.MV_OK != nRet)
            {
                pictureBox5.Visible = true;
                labelCameraInspection.Visible = true;
                _grabbing = false;
                ShowErrorMsg("Start Grabbing Fail!", nRet);
                return;
            }
        }

        private void stopCamera_Click(object sender, EventArgs e)
        {
            _grabbing = false;
            _cam.Stop();
        }

        private async void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop and dispose the reconnect timer if it exists
            _reconnectTimer?.Stop();
            _reconnectTimer?.Dispose();

            // Cancel the CancellationTokenSource if it exists
            _ctsForward?.Cancel();

            // Dispose the camera if it exists
            _cam?.Dispose();

            if (_grpc != null)
                await _grpc.DisposeAsync();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            sidebarExpand = !sidebarExpand;
            tableLayoutPanel1.ColumnStyles[1].Width = 50;
            sidebarTimer.Start();
        }

        private void sidebarTimer_Tick(object sender, EventArgs e)
        {
            float current = tableLayoutPanel1.ColumnStyles[0].Width;
            float target = sidebarExpand ? ExpandedWidth : CollapsedWidth;

            if (Math.Abs(current - target) < Step)
            {
                // snap ke target & stop
                tableLayoutPanel1.ColumnStyles[0].Width = target;
                sidebarTimer.Stop();
            }
            else
            {
                // animasi kecil/membesar
                if (current < target)
                    tableLayoutPanel1.ColumnStyles[0].Width += Step;
                else
                    tableLayoutPanel1.ColumnStyles[0].Width -= Step;
            }
        }

        private void showSidebarBtn_Click(object sender, EventArgs e)
        {
            sidebarExpand = !sidebarExpand;
            tableLayoutPanel1.ColumnStyles[1].Width = 0;
            sidebarTimer.Start();
        }

        /* ---------- UPDATE UI (thread-safe) ---------- */
        private void UpdateUI(Bitmap bmp, string text)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => UpdateUI(bmp, text)));
                return;
            }

            // update label
            resultInspectionLabel1.Text = text ?? string.Empty;
            tableLayoutPanel9.RowCount = 1;

            // dispose previous PictureBox image safely
            var old = pictureBox5.Image;
            if (old != null)
            {
                try { pictureBox5.Image = null; old.Dispose(); }
                catch { /* ignore */ }
            }

            if (bmp != null)
            {
                // clone bitmap into PictureBox so we can safely dispose the source
                try
                {
                    pictureBox5.Image = (Bitmap)bmp.Clone();
                }
                catch
                {
                    // fallback: assign directly if clone fails
                    pictureBox5.Image = bmp;
                    bmp = null; // avoid double-dispose below
                }
                finally
                {
                    // dispose the source (if still not null)
                    bmp?.Dispose();
                }
            }
        }

        public void RenderComponentsUI(string json, Control parent)
        {
            Root result = JsonConvert.DeserializeObject<Root>(json);
            if (result?.Components == null) return;

            indexFrame = result.StepIndex;

            if (!_uiBuilt)
            {
                BuildUiFirstTime(result, flowLayoutPanel1);
                _uiBuilt = true;

                UpdateUiInspectionResult(result, flowLayoutPanel1);
            }
            else
            {
                UpdateUiInspectionResult(result, flowLayoutPanel1);
            }
        }


        /// <summary>
        /// Update UI tanpa recreate control. Dipakai setelah BuildUiFirstTime.
        /// </summary>
        void UpdateUiInspectionResult(Root data, Control parent)
        {
            if (data?.Components == null) return;

            parent.SuspendLayout(); // 1. cegah redraw intermediate

            //foreach (var kvp in data.Components)
            //{
            //    foreach (var item in kvp.Value)
            //    {
            //        if (item.Boxes != null && item.Boxes.Count >= 4)
            //        {
            //            Console.WriteLine($"{kvp.Key}: {string.Join(", ", item.Boxes ?? new List<int>())}");
            //        }
            //    }
            //}

            foreach (var kv in data.Components)
            {
                string key = kv.Key;

                var imgBox = parent.Controls.Find($"imgBox_{key}", true).FirstOrDefault() as PictureBox;
                var resultLbl = parent.Controls.Find($"resultLbl_{key}", true).FirstOrDefault() as Label;
                var parrot = parent.Controls.Find($"parrot_{key}", true).FirstOrDefault() as ReaLTaiizor.Controls.ParrotWidgetPanel;
                var innerTable = parent.Controls.Find($"innerTable_{key}", true).FirstOrDefault() as TableLayoutPanel;

                if (imgBox == null || resultLbl == null || parrot == null || innerTable == null) continue;

                var item = kv.Value.FirstOrDefault();
                if (item == null) continue;

                var fileImage = fileUtils.FindById(item.ImageId);

                if (string.IsNullOrEmpty(fileImage) || !File.Exists(fileImage))
                {
                    Console.WriteLine($"[LoadImage] File belum ada: {item.ImageId}");
                    return;
                }

                /*---------- Gambar + Bounding Box ----------*/
                byte[] fileImageBytes = File.ReadAllBytes(fileImage);
                Bitmap srcBmp;
                Bitmap boxed;
                using (var ms = new MemoryStream(fileImageBytes))
                    srcBmp = new Bitmap(ms);   // bitmap independen

                // Inisialisasi supaya pasti punya nilai
                var allBoxes = new List<int>();

                foreach (var value in kv.Value)
                {
                    if (value.Boxes != null && value.Boxes.Count % 4 == 0)
                        allBoxes.AddRange(value.Boxes);   // gabung semua
                }

                if (allBoxes.Count > 0)
                {
                    try
                    {
                        boxed = imageManipulationUtils.DrawBoundingBoxMultiple(srcBmp, allBoxes);
                        // 3. Pakai hasil gambar
                        imgBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        imgBox.Margin = new Padding(0);
                        imgBox.Image = boxed;
                        innerTable.RowCount = 1;

                        // 4. Bersihkan bitmap sumber (optional)
                        srcBmp.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DrawBoxes] Error: {ex.Message}");
                    }
                }

                /*---------- Label & Warna ----------*/
                resultLbl.Text = item.Label ? "NG" : "Good";
                parrot.BackColor = !item.Label ? Color.FromArgb(4, 194, 55)
                                              : Color.FromArgb(192, 0, 0);

                /*---------- Sembunyikan dummy label (sekali saja) ----------*/
                var dummy = innerTable.Controls.OfType<Label>()
                                     .FirstOrDefault(l => l.Text == "Result Inspection");
                if (dummy != null && dummy.Visible) dummy.Visible = false;

                /*---------- Value Label: create once, update text ----------*/
                var valLbl = innerTable.Controls.OfType<Label>()
                                      .FirstOrDefault(l => l.Name == $"valLbl_{key}");
                if (string.IsNullOrEmpty(item.Value))
                {
                    if (valLbl != null) valLbl.Visible = false;
                }
                else
                {
                    if (valLbl == null)
                    {
                        valLbl = new Label
                        {
                            Name = $"valLbl_{key}",
                            ForeColor = Color.White,
                            AutoSize = true
                        };
                        innerTable.Controls.Add(valLbl);
                    }
                    valLbl.Text = $"Value: {item.Value}";
                    valLbl.Visible = true;
                }
            }

            /*---------- Final Label ----------*/
            var final = parent.Controls.Find("finalLabel", true).FirstOrDefault() as Label;
            if (final != null) final.Text = $"Final Label: {data.FinalLabel}";

            parent.ResumeLayout(true); // 2. terapkan semua perubahan sekaligus
        }


        // dibuat sekali saja, dipanggil ketika JSON pertama kali masuk
        void BuildUiFirstTime(Root data, FlowLayoutPanel parent)
        {
            parent.Controls.Clear(); // boleh, karena memang pertama kali

            foreach (var kv in data.Components)
            {
                string key = kv.Key;

                var panel = new Panel
                {
                    Name = $"panel_{key}",
                    Size = new System.Drawing.Size(185, 186),
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 20, 20)
                };

                var outerTable = new TableLayoutPanel
                {
                    ColumnCount = 1,
                    RowCount = 2,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(5),
                    BackColor = Color.FromArgb(117, 120, 123)
                };
                outerTable.RowStyles.Add(new RowStyle(SizeType.Percent, 82.38636F));
                outerTable.RowStyles.Add(new RowStyle(SizeType.Percent, 17.61364F));
                outerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 169F));
                panel.Controls.Add(outerTable);

                var innerTable = new TableLayoutPanel
                {
                    Name = $"innerTable_{key}",
                    ColumnCount = 1,
                    RowCount = 2,
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black
                };
                innerTable.RowStyles.Add(new RowStyle(SizeType.Percent, 75F));
                innerTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

                var imgBox = new PictureBox
                {
                    Name = $"imgBox_{key}",        // <— penting untuk update
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Properties.Resources.NoImage
                };
                innerTable.Controls.Add(imgBox, 0, 0);

                var labelDummy = new Label
                {
                    Text = "Result Inspection",
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold),
                    Anchor = AnchorStyles.None,
                    AutoSize = true
                };
                innerTable.Controls.Add(labelDummy, 0, 1);

                outerTable.Controls.Add(innerTable, 0, 0);

                var parrotPanel = new ReaLTaiizor.Controls.ParrotWidgetPanel
                {
                    Name = $"parrot_{key}",        // <— penting untuk update
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(192, 0, 0),
                    ControlsAsWidgets = false
                };
                outerTable.Controls.Add(parrotPanel, 0, 1);

                // === Result Table di ParrotWidgetPanel ===
                var resultTable = new TableLayoutPanel
                {
                    ColumnCount = 1,
                    RowCount = 1,
                    Dock = DockStyle.Fill,
                    Location = new System.Drawing.Point(0, 0),
                    Size = new System.Drawing.Size(169, 26),
                    AutoSize = false
                };
                resultTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                resultTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
                resultTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                parrotPanel.Controls.Add(resultTable);

                #region 7. Result label di dalam parrotPanel
                var resultLabel = new Label
                {
                    Name = $"resultLbl_{key}",     // <— penting untuk update
                    Text = "Result Inspection",
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                resultTable.Controls.Add(resultLabel);
                #endregion

                parent.Controls.Add(panel);
            }

            var finalLabel = new Label
            {
                Name = "finalLabel",
                Text = $"Final Label: {data.FinalLabel}",
                Font = new Font("Consolas", 9, FontStyle.Italic),
                ForeColor = Color.Blue,
                AutoSize = true
            };
            parent.Controls.Add(finalLabel);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Logs"
            );

            string filePath = Path.Combine(folderPath, "Inspection_20251010_113839.json"); // sesuaikan nama file
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

                // Deserialize JSON ke object CycleTimeSummary
                CycleTimeSummary summary = JsonConvert.DeserializeObject<CycleTimeSummary>(json);

                if (summary?.Logs == null || summary.Logs.Count == 0)
                {
                    MessageBox.Show("Tidak ada log untuk ditampilkan.");
                    return;
                }

                // Bind ke DataGridView
                dataGridView1.DataSource = summary.Logs;

                // Opsional: atur header
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

        private void gRPCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialogMenu = new DialogDebugMenuGrpc();

            dialogMenu.Show();
        }

        private void pLCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new DialogDebugMenuPlc();
            dialog.Show();
        }

        private async void panel8_Click(object sender, EventArgs e)
        {

            // dummy image 1×1 pixel
            //if (!_cam.Queue.TryDequeue(out byte[] jpegBytes))
            //{
            //    MessageBox.Show("Frame not found");
            //    return;
            //}
            var tcs = new TaskCompletionSource<byte[]>();

            _cam.FrameReadyForGrpc += bmp =>
            {
                if (bmp != null && bmp.Length > 0)
                    tcs.TrySetResult(bmp);
            };

            // Tunggu sampai event menghasilkan frame
            byte[] frameBytes = await tcs.Task;
            string imageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            string fileNameFormat = $"frame_{DateTime.Now:yyyyMMdd_HHmmss}_{imageId}.bmp";

            await fileUtils.SaveFrameToDiskAsync(frameBytes, fileNameFormat, imageId);
            _imageDbOperation.InsertImage(fileNameFormat, imageId);

            // Kirim lewat gRPC
            ImageResponse resp = await _grpc.ProcessImageAsync(frameBytes, default, imageId);
            //var resultJson = JsonConvert.DeserializeObject<Root>(resp.Result);
            RenderComponentsUI(GrpcResponseStore.LastResponse.Result, flowLayoutPanel1);

            var test = @"C:\\Users\\rizki.adha\\Desktop\\CapturedFrames\\frame_20251017_160417_566.bmp";
            var fileImage = fileUtils.FindById(imageId);

            if (string.IsNullOrEmpty(fileImage) || !File.Exists(fileImage))
            {
                Console.WriteLine($"[LoadImage] File belum ada: {imageId}");
                return;
            }
            var imgBox = flowLayoutPanel1.Controls.Find($"imgBox_fiducial", true).FirstOrDefault() as PictureBox;

            // salin ke memory dulu → tidak lock file
            byte[] bytes = File.ReadAllBytes(fileImage);
            using (var ms = new MemoryStream(bytes))
                imgBox.Image = System.Drawing.Image.FromStream(ms);


            // Panggil method UNARY
        }

        private void databaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var databaseDialog = new DatabaseDialog();

            databaseDialog.Show();
        }

        private void pLCToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var plcDialog = new PlcDialog();

            plcDialog.Show();
        }
    }
}
