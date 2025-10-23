using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Domain.Flow.Engine;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1.Domain.Actions
{
    public sealed class CameraPrepareAction : IFlowAction
    {
        public string Key => "Camera.Prepare";
        private readonly CameraManager _cam;
        public CameraPrepareAction(CameraManager cam) => _cam = cam;
        public Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            // No-throw start: your CameraManager already guards SDK presence
            _cam.Start();
            return Task.CompletedTask;
        }
    }

    public sealed class CameraCaptureFrameAction : IFlowAction
    {
        public string Key => "Camera.CaptureFrame";
        private readonly CameraManager _cam;
        private readonly ImageDbOperation _db;
        private readonly string _saveRoot;

        public CameraCaptureFrameAction(CameraManager cam, ImageDbOperation db)
        {
            _cam = cam; _db = db;
            _saveRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CapturedFrames");
            Directory.CreateDirectory(_saveRoot);
        }

        public async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            var tcs = new TaskCompletionSource<byte[]>();
            void Handler(byte[] bytes) { if (bytes?.Length > 0) tcs.TrySetResult(bytes); }
            _cam.FrameReadyForGrpc += Handler;
            try
            {
                ctx.LastFrame = await tcs.Task;
            }
            finally
            {
                _cam.FrameReadyForGrpc -= Handler;
            }

            ctx.LastImageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var fileName = $"frame_{DateTime.Now:yyyyMMdd_HHmmss}_{ctx.LastImageId}.bmp";
            var full = Path.Combine(_saveRoot, fileName);
            await Task.Run(() => File.WriteAllBytes(full, ctx.LastFrame));
            _db.InsertImage(fileName, ctx.LastImageId);
        }
    }
}
