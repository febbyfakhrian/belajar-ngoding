using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Helpers
{
    internal class ImageManipulationUtils
    {
        private static readonly Random _rng = new Random();
        public Bitmap DrawBoundingBox(
            Bitmap src,
            float x1,
            float y1,
            float x2,
            float y2,
            Scalar? color = null,
            int strokeWidth = 3)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            // Default warna = LimeGreen
            Scalar drawColor = color ?? new Scalar(50, 205, 50); // BGR (LimeGreen)

            // Convert dari Bitmap -> Mat
            using (Mat mat = BitmapConverter.ToMat(src))
            {
                // Pastikan koordinat valid
                int x = (int)Math.Round(x1);
                int y = (int)Math.Round(y1);
                int width = (int)Math.Round(x2 - x1);
                int height = (int)Math.Round(y2 - y1);

                if (width <= 0 || height <= 0)
                {
                    Console.WriteLine("[Warning] Invalid bounding box size, skipped drawing.");
                    return (Bitmap)src.Clone();
                }

                // Gambar rectangle
                Cv2.Rectangle(
                    mat,
                    new OpenCvSharp.Point(x, y),
                    new OpenCvSharp.Point(x + width, y + height),
                    drawColor,
                    strokeWidth,
                    LineTypes.AntiAlias
                );

                // Convert kembali ke Bitmap
                return BitmapConverter.ToBitmap(mat);
            }
        }

        /* ---------- GENERATE BITMAP ACAK ---------- */
        public Bitmap GenerateRandomBitmap(int w, int h)
        {
            var bmp = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    bmp.SetPixel(x, y, Color.FromArgb(_rng.Next(256), _rng.Next(256), _rng.Next(256)));
            return bmp;
        }

        /* ---------- GENERATE RANDOM TEXT ---------- */
        public string GenerateRandomText(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] buffer = new char[length];
            for (int i = 0; i < length; i++)
                buffer[i] = chars[_rng.Next(chars.Length)];
            return new string(buffer);
        }

        public Bitmap DrawBoundingBoxMultiple(Bitmap src, List<int> boxes, Scalar? color = null, int strokeWidth = 5)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (boxes == null || boxes.Count % 4 != 0) return src;   // skip kalau tidak valid

            Scalar drawColor = color ?? new Scalar(50, 205, 50);

            using (Mat mat = BitmapConverter.ToMat(src))
            {
                int srcW = mat.Width;
                int srcH = mat.Height;

                for (int i = 0; i < boxes.Count; i += 4)
                {
                    int x1 = Math.Max(0, boxes[i]);
                    int y1 = Math.Max(0, boxes[i + 1]);
                    int x2 = Math.Min(srcW, boxes[i + 2]);
                    int y2 = Math.Min(srcH, boxes[i + 3]);

                    int w = x2 - x1;
                    int h = y2 - y1;

                    if (w <= 0 || h <= 0) continue;   // skip box invalid

                    Cv2.Rectangle(mat,
                                  new OpenCvSharp.Point(x1, y1),
                                  new OpenCvSharp.Point(x2, y2),
                                  drawColor,
                                  strokeWidth,
                                  LineTypes.AntiAlias);
                }

                return BitmapConverter.ToBitmap(mat);
            }
        }

        public Bitmap CropAndDrawBoundingBoxes(Bitmap src, List<int> boxes, Scalar? color = null, int strokeWidth = 5)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (boxes == null || boxes.Count % 4 != 0) throw new ArgumentException("List boxes harus kelipatan 4 (x1, y1, x2, y2)");

            Scalar drawColor = color ?? new Scalar(0, 255, 0); // Default hijau

            using (Mat mat = BitmapConverter.ToMat(src))
            {
                int srcW = mat.Width;
                int srcH = mat.Height;

                int x1 = Math.Max(0, boxes[0]);
                int y1 = Math.Max(0, boxes[1]);
                int x2 = Math.Min(srcW, boxes[2]);
                int y2 = Math.Min(srcH, boxes[3]);

                int w = x2 - x1;
                int h = y2 - y1;

                // --- Gambar bounding box ---
                Cv2.Rectangle(mat,
                              new OpenCvSharp.Point(x1, y1),
                              new OpenCvSharp.Point(x2, y2),
                              drawColor,
                              strokeWidth,
                              LineTypes.AntiAlias);

                // --- Crop bagian gambar ---
                Rect roi = new Rect(x1, y1, w, h);
                using (Mat croppedMat = new Mat(mat, roi))
                {
                    Bitmap croppedBmp = BitmapConverter.ToBitmap(croppedMat);

                    return croppedBmp;
                }
            }
        }
    }
}
