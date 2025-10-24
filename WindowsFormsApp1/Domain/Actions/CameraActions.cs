using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Domain.Flow.Engine;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1.Domain.Actions
{
    #region ---- Camera : Prepare -------------------------------------------------
    public sealed class CameraPrepareAction : IFlowAction
    {
        public string Key => "Camera.Prepare";
        private readonly CameraManager _cam;
        private readonly IFlowContext _ctx;

        public CameraPrepareAction(CameraManager cam, IFlowContext ctx) {
            _cam = cam; 
            _ctx = ctx;
                }

        public Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            int nRet = _cam.Start(); // internal guard sudah ada
            return Task.CompletedTask;
        }
    }
    #endregion

    #region ---- Camera : Capture -------------------------------------------------
    public sealed class CameraCaptureFrameAction : IFlowAction
    {
        public string Key => "Camera.CaptureFrame";

        private readonly CameraManager _cam;
        private readonly ImageDbOperation _db;
        private readonly string _saveRoot;

        public CameraCaptureFrameAction(CameraManager cam, ImageDbOperation db)
        {
            _cam = cam;
            _db = db;

            _saveRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "CapturedFrames");

            Directory.CreateDirectory(_saveRoot);
        }

        public async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            Debug.WriteLine("[CAM] CaptureFrame - waiting frame...");

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

                Debug.WriteLine($"[CAM] Frame saved -> {fullPath}");
            }
            finally
            {
                _cam.FrameReadyForGrpc -= Handler;
            }
        }
    }
    #endregion
}