
namespace WindowsFormsApp1.Core.Domain.Flow.Engine
{
    public interface IActionRegistry
    {
        void Register(IFlowAction action);
        IFlowAction Get(string key);
        bool TryGet(string key, out IFlowAction action);
    }
}
