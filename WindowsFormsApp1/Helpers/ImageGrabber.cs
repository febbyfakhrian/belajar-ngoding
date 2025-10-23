using OpenCvSharp;
using OpenCvSharp.Extensions;
using PhotoSauce.MagicScaler;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Threading;

namespace WindowsFormsApp1.Helpers
{
    public class ImageGrabber : IDisposable
    {
        private string _imagePath;
        private Thread _worker;
        private volatile bool _running;

        // konfigurasi
        private int _refreshIntervalMs = 1000; // periksa gambar tiap 1 detik

        public readonly ConcurrentQueue<byte[]> Queue = new ConcurrentQueue<byte[]>();

        // event
        public event Action<Bitmap> FramePreview;
        public event Action<byte[]> FrameEncoded;
        public event Action<byte[]> FrameReadyForGrpc;

        public static class ImageGrabberService
        {
            public static readonly ImageGrabber Instance = new ImageGrabber();
        }

        public void SetImagePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path gambar tidak boleh kosong.", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("File gambar tidak ditemukan.", path);

            _imagePath = path;
        }

        public bool IsRunning => _running;

        public void Start()
        {
            if (_running) return;

            if (string.IsNullOrEmpty(_imagePath))
            {
                System.Diagnostics.Debug.WriteLine("[ImageGrabber] Path gambar belum diset.");
                return;
            }

            _running = true;
            _worker = new Thread(GrabLoop) { IsBackground = true, Name = "ImageGrabberThread" };
            _worker.Start();
        }

        public void Stop()
        {
            _running = false;
            try { _worker?.Join(); } catch { /* ignore */ }
            _worker = null;
        }

        private void GrabLoop()
        {
            while (_running)
            {
                try
                {
                    if (!File.Exists(_imagePath))
                    {
                        Console.WriteLine("[ImageGrabber] Gambar tidak ditemukan, retry...");
                        SleepSafe(_refreshIntervalMs);
                        continue;
                    }

                    ProcessImageFile(_imagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ImageGrabber] Error: " + ex.Message);
                }

                SleepSafe(_refreshIntervalMs);
            }
        }

        private void ProcessImageFile(string path)
        {
            try
            {
                // 1) Baca gambar pakai OpenCV
                using (var mat = Cv2.ImRead(path, ImreadModes.Color))
                {
                    if (mat.Empty())
                    {
                        Console.WriteLine("[ImageGrabber] Tidak bisa membaca gambar.");
                        return;
                    }

                    // 2) Encode langsung ke BMP
                    if (!Cv2.ImEncode(".bmp", mat, out byte[] bmpBuf))
                        throw new Exception("ImEncode BMP gagal.");

                    // Masukkan ke antrian (max 10 item)
                    while (Queue.Count >= 10 && Queue.TryDequeue(out _)) { /* drop lama */ }
                    Queue.Enqueue(bmpBuf);

                    // 3) Tampilkan preview ke UI
                    using (Bitmap bmp = BitmapConverter.ToBitmap(mat))
                    {
                        FramePreview?.Invoke((Bitmap)bmp.Clone());
                    }

                    // 4) Kirim event ke subscriber (data BMP)
                    FrameEncoded?.Invoke(bmpBuf);
                    FrameReadyForGrpc?.Invoke(bmpBuf);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ImageGrabber] ProcessImageFile error: " + ex.Message);
            }
        }


        private static void SleepSafe(int ms)
        {
            try { Thread.Sleep(ms); } catch { /* ignore */ }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
