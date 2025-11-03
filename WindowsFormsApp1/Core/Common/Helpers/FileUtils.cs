using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Infrastructure.Data;
using WindowsFormsApp1.Core.Interfaces; // Added for ISettingsService

namespace WindowsFormsApp1.Core.Common.Helpers
{
    class FileUtils
    {
        private static readonly SemaphoreSlim _throttle =
            new SemaphoreSlim(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

        private static readonly BlockingCollection<FilePacket> _writeQueue = new BlockingCollection<FilePacket>(256);
        private readonly ImageDbOperation _dbHelper;
        private static ISettingsService _settingsService; // Static field for settings service

        // Constructor with dependency injection including settings service
        public FileUtils(ImageDbOperation dbHelper, ISettingsService settingsService = null)
        {
            _dbHelper = dbHelper;
            _settingsService = settingsService; // Store in static field
        }

        // Parameterless constructor for backward compatibility
        public FileUtils()
        {
            // Constructor with no parameters for static usage
        }

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
                // Use path from settings service with fallback to default
                var dir = GetCapturedFramesDirectory();
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
            // Use path from settings service with fallback to default
            var dir = GetCapturedFramesDirectory();
            
            // Use the injected dbHelper if available, otherwise fall back to static
            var fn = _dbHelper?.FindById(imageId) ?? DatabaseDialog.DbHelper?.FindById(imageId);

            return fn == null ? null : Path.Combine(dir, fn);
        }

        public string GetLatestImage()
        {
            // Use path from settings service with fallback to default
            var dir = GetCapturedFramesDirectory();
            
            // Use the injected dbHelper if available, otherwise fall back to static
            var fn = _dbHelper?.GetLatestImageById() ?? DatabaseDialog.DbHelper?.GetLatestImageById();

            return fn == null ? null : Path.Combine(dir, fn);
        }

        /// <summary>
        /// Gets the captured frames directory path from settings service or falls back to default
        /// </summary>
        /// <returns>The directory path for captured frames</returns>
        private static string GetCapturedFramesDirectory()
        {
            string savedPath = null;
            
            try
            {
                // Try to get the saved folder path from settings service
                savedPath = _settingsService?.GetSetting<string>("captured_frames", "folder_path");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get saved folder path from settings: {ex.Message}");
            }

            // Use the saved path if available and valid, otherwise fallback to default
            if (!string.IsNullOrWhiteSpace(savedPath) && Directory.Exists(savedPath))
            {
                return savedPath;
            }
            else
            {
                // Fallback to default path on desktop
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "CapturedFrames");
            }
        }
    }
}