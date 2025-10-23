using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Domain.Flow.Engine
{
    public interface ITriggerBus
    {
        Task FireAsync(string trigger);
    }

    // Simple adapter around StatelessHost
    public sealed class HostTriggerBus : ITriggerBus
    {
        private readonly StatelessHost _host;
        public HostTriggerBus(StatelessHost host) { _host = host; }
        public Task FireAsync(string trigger) { return _host.FireAsync(trigger); }
    }
}
