using Api;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Helpers;
using static WindowsFormsApp1.VideoGrabber;

namespace WindowsFormsApp1
{
    public partial class DialogDebugMenuGrpc : Form
    {
        // Re-use the same service instance while the dialog is open.
        private readonly GrpcService _grpc = new GrpcService();   // or inject host/port if required
        private readonly CameraManager _cam = new CameraManager(new ConcurrentQueue<byte[]>());
        private static readonly Random _rnd = new Random();
        public ImageResponse apiResponse = new ImageResponse();
        public VideoGrabber grabber = VideoGrabberService.Instance;
        private static string videoPath;
        private static string photoPath;
        private readonly ImageGrabber _imageGrabber = new ImageGrabber();

        public DialogDebugMenuGrpc()
        {
            InitializeComponent();
            sendImageDummyBtn.Enabled = false;
            testUpdateConfigBtn.Enabled = false;
            sendImageDummyStreamBtn.Enabled = false;
            sendVideoFrameBtn.Enabled = false;
        }

        private async void TestConnectionBtn_Click(object sender, EventArgs e)
        {
            // Disable button to prevent double-clicks
            testConnectionBtn.Enabled = false;

            try
            {
                bool ok = await _grpc.StartAsync();
                sendImageDummyBtn.Enabled = true;
                testUpdateConfigBtn.Enabled = true;
                sendImageDummyStreamBtn.Enabled = true;

                MessageBox.Show(this,
                    ok ? "gRPC connection succeeded." : "gRPC connection failed.",
                    "Test Connection",
                    MessageBoxButtons.OK,
                    ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Unexpected error while testing connection:\r\n{ex.Message}",
                    "Test Connection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                testConnectionBtn.Enabled = true;
            }
        }

        private byte[] CreateDummyImage(int width = 1, int height = 1)
        {
            using (var bmp = new Bitmap(width, height))
            {
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        bmp.SetPixel(x, y, Color.FromArgb(
                            _rnd.Next(256),
                            _rnd.Next(256),
                            _rnd.Next(256)
                        ));
                    }

                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        private async void sendImageDummyBtn_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Generate 5 dummy images
                //var dummyImages = Enumerable.Range(0, 5)
                //                            .Select(_ => CreateDummyImage(1, 1)) // 1x1 pixel
                //                            .ToList();

                // Ambil gambar dari resource
                Bitmap noImage = Properties.Resources.NoImage;

                // Konversi Bitmap ke byte[]
                byte[] imageBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    noImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    imageBytes = ms.ToArray();
                }

                // Kirim ke gRPC (anggap ProcessImageStreamAsync menerima IEnumerable<byte[]>)
                IEnumerable<byte[]> imageChunks = SplitBytes(imageBytes);

                ImageResponse resp = await _grpc.ProcessImageStreamAsync(imageChunks);

                // ---------------------------------------------
                // Fungsi lokal untuk memecah byte[] menjadi chunk
                IEnumerable<byte[]> SplitBytes(byte[] data, int chunkSize = 64 * 1024)
                {
                    for (int i = 0; i < data.Length; i += chunkSize)
                    {
                        int size = Math.Min(chunkSize, data.Length - i);
                        byte[] chunk = new byte[size];
                        Array.Copy(data, i, chunk, 0, size);
                        yield return chunk;
                    }
                }


                //Console.WriteLine($"Server responded: {resp}");

                Console.WriteLine("=== DEBUG ===");
                //Console.WriteLine("resp.Result:");
                //Console.WriteLine(resp.Result);
                this.apiResponse = resp;

                //ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(resp.Result);
                //Console.WriteLine("apiResponse.Result:");
                //Console.WriteLine(apiResponse.Result);
                //foreach (var item in apiResponse.Result.F)
                //{
                //    Console.WriteLine("Fiducial boxes: " + string.Join(", ", item.Boxes));
                //    Console.WriteLine("Score: " + item.Score);
                //    Console.WriteLine("Label: " + item.Label);
                //}

                ////Root data = JsonConvert.DeserializeObject<Root>(apiResponse.Result);
                ////Console.WriteLine("Deserialized data:");
                ////Console.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));

                // Show success/failure info
                MessageBox.Show(this,
                    $"Dummy images sent successfully.\r\nServer Response:\r\n{resp}",
                    "Send Dummy Image",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Show error dialog if something went wrong
                MessageBox.Show(this,
                    $"Failed to send dummy images:\r\n{ex.Message}",
                    "Send Dummy Image",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Pilih File Konfigurasi";
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                        return; // User batal pilih file

                    string path = openFileDialog.FileName;

                    if (!File.Exists(path))
                    {
                        MessageBox.Show(this,
                            $"File tidak ditemukan: {path}",
                            "Update Config",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    // Kirim file ke gRPC
                    var resp = await _grpc.UpdateConfigAsync(path);

                    Console.WriteLine($"Status : {resp.Status}");
                    Console.WriteLine($"Message: {resp.Message}");

                    // Tampilkan hasil
                    MessageBox.Show(this,
                        $"Config update finished.\r\nStatus : {resp.Status}\r\nMessage: {resp.Message}",
                        "Update Config",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Gagal update config:\r\n{ex.Message}",
                    "Update Config",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (var call = _grpc.CreateImageStream())
                {
                    // Task untuk baca response dari server
                    var readTask = Task.Run(async () =>
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            var response = call.ResponseStream.Current;
                            //Console.WriteLine($"[Server] {response.Status} - {response.Message} - {response.Result}");
                        }
                    });

                    // Kirim 5 dummy image
                    for (int i = 0; i < 5; i++)
                    {
                        var imgBytes = CreateDummyImage(1, 1);
                        await call.RequestStream.WriteAsync(new ImageRequest
                        {
                            ImageId = $"img-{i + 1}",
                            ImageData = Google.Protobuf.ByteString.CopyFrom(imgBytes)
                        });
                        Console.WriteLine($"[Client] Sent img-{i + 1}");
                    }

                    // Selesai kirim
                    await call.RequestStream.CompleteAsync();

                    // Tunggu sampai baca response selesai
                    await readTask;
                }

                MessageBox.Show(this,
                    "Streaming selesai.\r\nCek console untuk response server.",
                    "Stream Dummy Images",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Streaming error:\r\n" + ex.Message,
                    "Stream Dummy Images",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void DialogDebugMenu_Load(object sender, EventArgs e)
        {
        }

        private async void sendVideoFrameOnce_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(videoPath))
            {
                MessageBox.Show($"File tidak ditemukan: {videoPath}");
                return;
            }

            // dummy image 1×1 pixel
            if (!grabber._queue.TryDequeue(out byte[] jpegBytes))
            {
                MessageBox.Show("Frame not found");
                return;
            }

            // Simpan frame ke folder lokal
            string savePath = await SaveFrameToDiskAsync(jpegBytes);
            //MessageBox.Show($"Frame disimpan di: {savePath}");

            // Panggil method UNARY
            ImageResponse resp = await _grpc.ProcessImageAsync(jpegBytes);

            // Stream
            //IEnumerable<byte[]> imageChunks = SplitBytes(jpegBytes);

            //ImageResponse resp = await _grpc.ProcessImageStreamAsync(imageChunks);

            //// ---------------------------------------------
            //// Fungsi lokal untuk memecah byte[] menjadi chunk
            //IEnumerable<byte[]> SplitBytes(byte[] data, int chunkSize = 64 * 1024)
            //{
            //    for (int i = 0; i < data.Length; i += chunkSize)
            //    {
            //        int size = Math.Min(chunkSize, data.Length - i);
            //        byte[] chunk = new byte[size];
            //        Array.Copy(data, i, chunk, 0, size);
            //        yield return chunk;
            //    }
            //}


            ////Console.WriteLine($"Server responded: {resp}");

            //Console.WriteLine("=== DEBUG ===");
            ////Console.WriteLine("resp.Result:");
            ////Console.WriteLine(resp.Result);
        }

        private Task<string> SaveFrameToDiskAsync(byte[] jpegBytes)
        {
            return Task.Run(() =>
            {
                try
                {
                    // Tentukan folder tujuan (misal: ./CapturedFrames)
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string folderPath = Path.Combine(desktopPath, "CapturedFrames");

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    // Buat nama file unik berdasarkan timestamp
                    string fileName = $"frame_{DateTime.Now:yyyyMMdd_HHmmss_fff}.jpg";
                    string fullPath = Path.Combine(folderPath, fileName);

                    // Simpan file
                    File.WriteAllBytes(fullPath, jpegBytes);
                    return fullPath;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SaveFrameToDiskAsync] Error: {ex.Message}");
                    return null;
                }
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Pilih File Video";
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                        return; // User batal pilih file

                    videoPath = openFileDialog.FileName;

                    if (!File.Exists(videoPath))
                    {
                        MessageBox.Show($"File tidak ditemukan: {videoPath}");
                        return;
                    }
                    sendVideoFrameBtn.Enabled = true;
                    grabber.SetPath(videoPath);

                    grabber.Start();
                    //grabber.FramePreview += bmp => pictureBox1.Image = bmp;
                    //grabber.FrameEncoded += bytes => Console.WriteLine($"Encoded frame: {bytes.Length} bytes");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal update videoPath:\r\n{ex.Message}");
            }
        }

        private async void sendVideoFrameStreamBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (var call = _grpc.CreateImageStream())
                {
                    // Task untuk baca response dari server
                    var readTask = Task.Run(async () =>
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            var response = call.ResponseStream.Current;
                            //Console.WriteLine($"[Server] {response.Status} - {response.Message} - {response.Result}");
                        }
                    });

                    // Kirim 5 dummy image
                    for (int i = 0; i < 10000; i++)
                    {
                        var imgBytes = CreateDummyImage(1, 1);
                        _ = _grpc.ProcessImageAsync(imgBytes);
                        //await call.RequestStream.WriteAsync(new ImageRequest
                        //{
                        //    ImageId = $"img-{i + 1}",
                        //    ImageData = Google.Protobuf.ByteString.CopyFrom(imgBytes)
                        //});
                        //Console.WriteLine($"[Client] Sent img-{i + 1}");
                    }
                    // Selesai kirim
                    await call.RequestStream.CompleteAsync();

                    // Tunggu sampai baca response selesai
                    await readTask;
                }

                MessageBox.Show(this,
                    "Streaming selesai.\r\nCek console untuk response server.",
                    "Stream Dummy Images",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Streaming error:\r\n" + ex.Message,
                    "Stream Dummy Images",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void setPhotoBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Pilih File Photo";
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                        return; // User batal pilih file

                    photoPath = openFileDialog.FileName;

                    if (!File.Exists(photoPath))
                    {
                        MessageBox.Show($"File tidak ditemukan: {photoPath}");
                        return;
                    }
                    Console.WriteLine(photoPath);

                    _imageGrabber.SetImagePath(photoPath);
                    _imageGrabber.Start();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal update videoPath:\r\n{ex.Message}");
            }
        }
    }
}