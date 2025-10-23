using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Domain.Flow.Engine
{
    public interface IActionRegistry
    {
        void Register(IFlowAction action);
        IFlowAction Get(string key);
        bool TryGet(string key, out IFlowAction action);
    }
}
