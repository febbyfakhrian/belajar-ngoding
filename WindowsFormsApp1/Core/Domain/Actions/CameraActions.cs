using MvCamCtrl.NET;
using System;
using System.Diagnostics;
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
        private readonly string _saveRoot;

        public CameraCaptureFrameAction(ICameraService cam, ImageDbOperation db)
        {
            _cam = cam;
            _db = db;

            _saveRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "CapturedFrames");

            Directory.CreateDirectory(_saveRoot);
        }

        public override async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            try
            {
                LogInfo("Capturing frame");
                
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

                    ctx.LastFrame = frame;
                    ctx.LastImageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

                    var fileName = $"frame_{DateTime.Now:yyyyMMdd_HHmmss}_{ctx.LastImageId}.bmp";
                    var fullPath = Path.Combine(_saveRoot, fileName);

                    File.WriteAllBytes(fullPath, frame);
                    _db.InsertImage(fileName, ctx.LastImageId);

                    LogInfo($"Frame saved -> {fullPath}");
                }
                finally
                {
                    _cam.FrameReadyForGrpc -= Handler;
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to capture frame: {ex.Message}");
                throw;
            }
        }
    }
    #endregion
}