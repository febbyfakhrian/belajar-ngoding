using OpenCvSharp;
using OpenCvSharp.Extensions;
using PhotoSauce.MagicScaler;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1.Core.Common.Helpers
{
    public class VideoGrabber
    {
        private string _videoPath;
        public readonly ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();
        private Thread _worker;
        private bool _running;
        private int _maxQueue = 10;

        public event Action<Bitmap> FramePreview;
        public event Action<byte[]> FrameEncoded;
        public event Action<byte[]> FrameReadyForGrpc;

        public static class VideoGrabberService
        {
            public static readonly VideoGrabber Instance = new VideoGrabber();
        }

        public VideoGrabber()
        {
            // konstruktor tanpa path (opsional)
        }

        public void SetPath(string videoPath)
        {
            if (string.IsNullOrWhiteSpace(videoPath))
                throw new ArgumentException("Video path cannot be empty.");

            if (!System.IO.File.Exists(videoPath))
                throw new System.IO.FileNotFoundException("Video file not found.", videoPath);

            _videoPath = videoPath;
        }

        public void Start()
        {
            if (_running)
                return;

            if (string.IsNullOrEmpty(_videoPath))
            {
                MessageBox.Show("Video path has not been set. Use SetPath() before Start().");
                return;
            }

            _running = true;
            _worker = new Thread(GrabLoop);
            _worker.IsBackground = true;
            _worker.Start();
        }

        public void Stop()
        {
            _running = false;
            _worker?.Join();
        }

        private void GrabLoop()
        {
            while (_running)                     // keep spinning while the grabber is on
            {
                using (var capture = new VideoCapture(_videoPath))
                {
                    if (!capture.IsOpened())
                    {
                        Console.WriteLine("[VideoGrabber] Gagal membuka video: " + _videoPath);
                        return;                  // fatal – file missing / codec problem
                    }

                    var frame = new Mat();
                    while (_running)
                    {
                        if (!capture.Read(frame) || frame.Empty())
                        {
                            // reached EOF -> rewind instantly, no gap
                            capture.Set(VideoCaptureProperties.PosFrames, 0);
                            continue;
                        }

                        try
                        {
                            // 1. Mat → BMP bytes
                            byte[] bmpBuf;
                            Cv2.ImEncode(".bmp", frame, out bmpBuf);

                            // 2. BMP stream for MagicScaler
                            using (MemoryStream bmpStream = new MemoryStream(bmpBuf))
                            using (MemoryStream jpegStream = new MemoryStream())
                            {
                                MagicImageProcessor.ProcessImage(
                                    bmpStream,
                                    jpegStream,
                                    new ProcessImageSettings
                                    {
                                        ResizeMode = CropScaleMode.Stretch,
                                        Width = frame.Width,
                                        Height = frame.Height
                                    });

                                byte[] jpegBytes = jpegStream.ToArray();

                                // 3. queue
                                if (_queue.Count >= _maxQueue) _queue.TryDequeue(out _);
                                _queue.Enqueue(jpegBytes);

                                // 4. preview
                                using (Bitmap previewBmp = BitmapConverter.ToBitmap(frame))
                                {
                                    FramePreview?.Invoke((Bitmap)previewBmp.Clone());
                                }

                                FrameEncoded?.Invoke(jpegBytes);
                                FrameReadyForGrpc?.Invoke(jpegBytes);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[VideoGrabber] Error: " + ex.Message);
                        }

                        Thread.Sleep(33); // ~30 FPS
                    }
                }
            }
        }
    }
}
