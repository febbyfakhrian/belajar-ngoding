﻿using MvCamCtrl.NET;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Core.Domain.Flow.Engine;
using WindowsFormsApp1.Infrastructure.Data;
using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1.Core.Domain.Actions
{
    #region ---- Camera : Prepare -------------------------------------------------
    public sealed class CameraPrepareAction : BaseAction
    {
        public override string Key => "Camera.Prepare";
        private readonly ICameraService _cam;
        private readonly IFlowContext _ctx;

        public CameraPrepareAction(ICameraService cam, IFlowContext ctx) {
            _cam = cam; 
            _ctx = ctx;
        }

        public override async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct)
        {
            try
            {
                LogInfo("Preparing camera");
                
                // 2. Set handle display
                if (ctx.DisplayHandle != IntPtr.Zero)
                    _cam.DisplayHandle = ctx.DisplayHandle;

                // 3. Baru start
                int ret = await _cam.StartAsync();
                
                LogInfo($"Camera started with result: {ret}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to prepare camera: {ex.Message}");
                throw;
            }
        }
    }
    #endregion

    #region ---- Camera : Capture -------------------------------------------------
    public sealed class CameraCaptureFrameAction : BaseAction
    {
        public override string Key => "Camera.CaptureFrame";

        private readonly ICameraService _cam;
        private readonly ImageDbOperation _db;
        private readonly ISettingsService _settings;
        private readonly string _saveRoot;
        private static readonly object _fileLock = new object(); // Static lock for thread safety across instances

        public CameraCaptureFrameAction(ICameraService cam, ImageDbOperation db, ISettingsService settings = null)
        {
            _cam = cam;
            _db = db;
            _settings = settings;

            LogInfo($"CameraCaptureFrameAction initialized with settings service: {_settings != null}");

            // Try to get the saved folder path from settings, fallback to desktop if not available
            string savedPath = null;
            try
            {
                savedPath = _settings?.GetSetting<string>("captured_frames", "folder_path");
                LogInfo($"Saved folder path from settings: {savedPath}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to get saved folder path from settings: {ex.Message}");
            }

            // Use the saved path if available and valid, otherwise fallback to default
            if (!string.IsNullOrWhiteSpace(savedPath) && Directory.Exists(savedPath))
            {
                _saveRoot = savedPath;
                LogInfo($"Using saved folder path: {_saveRoot}");
            }
            else
            {
                _saveRoot = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "CapturedFrames");
                LogInfo($"Using default folder path: {_saveRoot}");
            }

            try
            {
                Directory.CreateDirectory(_saveRoot);
                LogInfo($"Save root directory ensured: {_saveRoot}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to create save root directory: {ex.Message}");
            }
        }

        public override async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            try
            {
                // Check if we're in debug mode
                bool isDebug = false;
                try
                {
                    // Try to get the debug flag from settings
                    var debugSetting = _settings?.GetSetting<string>("debug", "is_debug");
                    LogInfo($"Debug setting value: {debugSetting}");
                    isDebug = !string.IsNullOrEmpty(debugSetting) && debugSetting.ToLower() == "true";
                    LogInfo($"Is debug mode: {isDebug}");
                }
                catch (Exception ex)
                {
                    // If we can't get the setting, assume not in debug mode
                    LogError($"Failed to get debug setting: {ex.Message}");
                    isDebug = false;
                }

                if (isDebug)
                {
                    LogInfo("Executing in debug mode");
                    // In debug mode, use a static image
                    await ExecuteDebugAsync(ctx, ct);
                }
                else
                {
                    LogInfo("Executing in normal mode");
                    // Normal camera capture
                    await ExecuteNormalAsync(ctx, ct);
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to capture frame: {ex.Message}");
                throw;
            }
        }

        private async Task ExecuteNormalAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            // one-shot frame grab
            var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Handler(byte[] bytes)
            {
                if (bytes?.Length > 0) tcs.TrySetResult(bytes);
            }

            _cam.FrameReadyForGrpc += Handler;
            try
            {
                var frame = await tcs.Task; // akan selesai setelah 1 frame
                ct.ThrowIfCancellationRequested();

                // Check if we have a valid frame
                if (frame == null || frame.Length == 0)
                {
                    LogError("Failed to capture frame from camera");
                    throw new InvalidOperationException("Failed to capture frame from camera");
                }

                ctx.LastFrame = frame;
                ctx.LastImageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

                var fileName = $"frame_{DateTime.Now:yyyyMMdd_HHmmss}_{ctx.LastImageId}.bmp";
                var fullPath = Path.Combine(_saveRoot, fileName);

                // Use lock to ensure thread-safe file access
                lock (_fileLock)
                {
                    File.WriteAllBytes(fullPath, frame);
                    _db.InsertImage(fileName, ctx.LastImageId);
                }

                LogInfo($"Frame saved -> {fullPath}");
            }
            finally
            {
                _cam.FrameReadyForGrpc -= Handler;
            }
        }

        private async Task ExecuteDebugAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            // In debug mode, we'll use a placeholder image or a predefined image
            try
            {
                // Check if there's a debug image path in settings
                string debugImagePath = null;
                try
                {
                    debugImagePath = _settings?.GetSetting<string>("debug", "image_path");
                    LogInfo($"Debug image path from settings: {debugImagePath}");
                }
                catch (Exception ex)
                {
                    LogInfo($"Failed to get debug image path from settings: {ex.Message}");
                }

                byte[] frame;
                
                if (!string.IsNullOrEmpty(debugImagePath))
                {
                    LogInfo($"Debug image path is not null or empty: {debugImagePath}");
                    try
                    {
                        if (File.Exists(debugImagePath))
                        {
                            LogInfo($"Using debug image: {debugImagePath}");
                            // Use the specified debug image
                            frame = File.ReadAllBytes(debugImagePath);
                        }
                        else
                        {
                            LogInfo($"Debug image file does not exist, creating placeholder");
                            frame = CreatePlaceholderImage();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to load debug image, creating placeholder: {ex.Message}");
                        frame = CreatePlaceholderImage();
                    }
                }
                else
                {
                    LogInfo("No debug image path specified, creating placeholder debug image");
                    frame = CreatePlaceholderImage();
                }

                ctx.LastFrame = frame;
                ctx.LastImageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

                var fileName = $"frame_{DateTime.Now:yyyyMMdd_HHmmss}_{ctx.LastImageId}.bmp";
                var fullPath = Path.Combine(_saveRoot, fileName);

                // Use lock to ensure thread-safe file access
                lock (_fileLock)
                {
                    File.WriteAllBytes(fullPath, frame);
                    _db.InsertImage(fileName, ctx.LastImageId);
                }

                // Check if we have a valid frame
                if (frame == null || frame.Length == 0)
                {
                    LogError("Failed to create or load debug frame");
                    throw new InvalidOperationException("Failed to create or load debug frame");
                }

                ctx.LastFrame = frame;
                ctx.LastImageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

                // Use lock to ensure thread-safe file access
                lock (_fileLock)
                {
                    File.WriteAllBytes(fullPath, frame);
                    _db.InsertImage(fileName, ctx.LastImageId);
                }

                LogInfo($"Debug frame saved -> {fullPath}");
                
                // Add a small delay to simulate camera capture time
                await Task.Delay(100, ct);
            }
            catch (Exception ex)
            {
                LogError($"Failed to create debug frame: {ex.Message}");
                throw;
            }
        }

        private byte[] CreatePlaceholderImage()
        {
            try
            {
                // Create a simple placeholder bitmap
                using (var bitmap = new Bitmap(640, 480))
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.Gray);
                    g.DrawString("Debug Mode", new Font("Arial", 24), Brushes.White, new PointF(10, 10));
                    g.DrawString($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}", new Font("Arial", 16), Brushes.White, new PointF(10, 50));
                    
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Bmp);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to create placeholder image: {ex.Message}");
                // Return empty array if we can't create a placeholder
                return new byte[0];
            }
        }
    }
    #endregion
}