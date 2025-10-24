using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Domain.Flow.Engine;
using System.Windows.Forms;
using System.Linq;
using Newtonsoft.Json;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Domain.Actions
{
    public sealed class GrpcProcessImageAction : IFlowAction
    {
        public string Key => "Grpc.ProcessImage";
        private readonly GrpcService _grpc;
        public GrpcProcessImageAction(GrpcService grpc) => _grpc = grpc;

        public async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            try
            {
                Debug.WriteLine(ctx.LastFrame.Length);
                Debug.WriteLine(ctx.LastImageId);
                var resp = await _grpc.ProcessImageAsync(ctx.LastFrame, ct, ctx.LastImageId);
                ctx.LastGrpcJson = resp.Result;

                var root = JsonConvert.DeserializeObject<Root>(ctx.LastGrpcJson);
                ctx.FinalLabel = root?.FinalLabel;
                Debug.WriteLine($"[AI] FinalLabel = {ctx.FinalLabel}");

                // ➜ panggil render (non-blocking)
                _ = Task.Run(() =>
                {
                    var mainForm = Application.OpenForms.OfType<MainDashboard>().FirstOrDefault();
                    mainForm?.BeginInvoke(new Action(() => mainForm?.RenderComponentsUI(ctx.LastGrpcJson, mainForm.flowLayoutPanel1)));
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI][ERR] {ex}");
                throw;                  // biar tetap muncul di Output / log
            }
        }
    }
}
