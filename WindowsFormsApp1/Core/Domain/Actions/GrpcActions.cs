using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using Newtonsoft.Json;
using WindowsFormsApp1.Core.Entities.Models;
using WindowsFormsApp1.Core.Interfaces;
using WindowsFormsApp1.Core.Domain.Flow.Engine;

namespace WindowsFormsApp1.Core.Domain.Actions
{
    public sealed class GrpcProcessImageAction : BaseAction
    {
        public override string Key => "Grpc.ProcessImage";
        private readonly IGrpcService _grpc;
        public GrpcProcessImageAction(IGrpcService grpc) => _grpc = grpc;

        public override async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            try
            {
                LogInfo("Processing image via gRPC");

                var resp = await _grpc.ProcessImageAsync(ctx.LastFrame, ct, ctx.LastImageId);
                ctx.LastGrpcJson = resp.Result;

                var root = JsonConvert.DeserializeObject<Root>(ctx.LastGrpcJson);
                // Ensure FinalLabel is never null by providing a default value
                ctx.FinalLabel = root?.FinalLabel ?? false;  // Default to false if root is null
                LogInfo($"FinalLabel = {ctx.FinalLabel}");

                // ➜ panggil render (non-blocking)
                _ = Task.Run(() =>
                {
                    var mainForm = Application.OpenForms.OfType<MainDashboard>().FirstOrDefault();
                    mainForm?.BeginInvoke(new Action(() => mainForm?.RenderComponentsUI(ctx.LastGrpcJson, mainForm.flowLayoutPanel1)));
                });
            }
            catch (Exception ex)
            {
                LogError($"Failed to process image via gRPC: {ex.Message}");
                throw;                  // biar tetap muncul di Output / log
            }
        }
    }
}