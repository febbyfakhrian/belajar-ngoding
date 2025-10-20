using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1.Helpers
{
    class FileUtils
    {
        private static readonly SemaphoreSlim _throttle =
            new SemaphoreSlim(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

        private static readonly BlockingCollection<FilePacket> _writeQueue = new BlockingCollection<FilePacket>(256);

        private ImageDbOperation _imageDbOperation => AutoInspectionPlatform.Program.DbHelper;

        private sealed class FilePacket
        {
            public byte[] Data;
            public string FileName;
        }

        public Task<string> SaveFrameToDiskAsync(byte[] jpegBytes, string fileNameFormat = "", string imageId = "")
        {
            if (jpegBytes == null || jpegBytes.Length == 0)
                return Task.FromResult<string>(null);

            var packet = new FilePacket
            {
                Data = jpegBytes,
                FileName = fileNameFormat   // nama unik dari luar
            };
            bool posted = _writeQueue.TryAdd(packet);

            if (!posted) return Task.FromResult<string>(null);
            imageId = imageId ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            fileNameFormat = fileNameFormat ?? $"frame_{DateTime.Now:yyyyMMdd_HHmmss}_{imageId}.bmp";

            EnsureWriterRunning(fileNameFormat, imageId);
            return Task.FromResult<string>(null); // fire-and-forget
        }

        private static Task _writerTask;
        private static void EnsureWriterRunning(string fileNameFormat, string imageId)
        {
            if (_writerTask != null) return;

            _writerTask = Task.Run(() =>
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "CapturedFrames");

                Directory.CreateDirectory(dir);

                foreach (var pkt in _writeQueue.GetConsumingEnumerable())
                {
                    _throttle.Wait();
                    Task.Run(async () =>
                    {
                        try
                        {
                            string path = Path.Combine(dir, pkt.FileName);  // ➜ nama unik!
                            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write,
                                                           FileShare.None, 4096, useAsync: true))
                            {
                                await fs.WriteAsync(pkt.Data, 0, pkt.Data.Length).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SaveFrameToDiskAsync] {ex.Message}");
                        }
                        finally
                        {
                            _throttle.Release();
                        }
                    });
                }

                return Task.CompletedTask;
            });
        }

        public string FindById(string imageId)
        {
            var dir = Path.Combine(
                   Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                   "CapturedFrames");
            var fn = _imageDbOperation.FindById(imageId);

            return fn == null ? null : Path.Combine(dir, fn);
        }
    }
}
