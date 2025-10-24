using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace WindowsFormsApp1.Domain.Flow.Engine
{
    public interface IFlowContext
    {
        bool? FinalLabel { get; set; }
        string LastImageId { get; set; }
        byte[] LastFrame { get; set; }
        string LastGrpcJson { get; set; }

        string Trigger { get; set; }

        ConcurrentDictionary<string, bool> Conditions { get; } // <-- baru
    }

    public sealed class FlowContext : IFlowContext
    {
        public bool? FinalLabel { get; set; }
        public string LastImageId { get; set; }
        public byte[] LastFrame { get; set; }
        public string LastGrpcJson { get; set; }

        public string Trigger { get; set; }

        public ConcurrentDictionary<string, bool> Conditions { get; } =
       new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
    }
}
