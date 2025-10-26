using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Core.Domain.Flow.Engine
{
    public interface IFlowAction
    {
        string Key { get; } // e.g. "Plc.LampOn"
        Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default);
    }
}
