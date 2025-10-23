using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Domain.Flow.Engine;
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
            var resp = await _grpc.ProcessImageAsync(ctx.LastFrame, ct, ctx.LastImageId);
            ctx.LastGrpcJson = resp.Result;
            var root = JsonConvert.DeserializeObject<Root>(ctx.LastGrpcJson);
            ctx.FinalLabel = root?.FinalLabel; // drive AutoTriggers
        }
    }
}
