using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Domain.Flow.Engine
{
    public sealed class ActionRegistry : IActionRegistry
    {
        private readonly Dictionary<string, IFlowAction> _map = new Dictionary<string, IFlowAction>(StringComparer.OrdinalIgnoreCase);
        public void Register(IFlowAction action) => _map[action.Key] = action;
        public IFlowAction Get(string key) => _map.TryGetValue(key, out var a) ? a : throw new KeyNotFoundException(key);
        public bool TryGet(string key, out IFlowAction action) => _map.TryGetValue(key, out action);
    }
}
