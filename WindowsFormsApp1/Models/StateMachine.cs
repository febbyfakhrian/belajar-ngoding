using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Models
{
    public class FlowConfig
    {
        public List<string> States { get; set; }
        public List<Transition> Transitions { get; set; }
        public Dictionary<string, List<string>> OnEntry { get; set; }
    }

    public class Transition
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Trigger { get; set; } = string.Empty;
        public string Guard { get; set; } = string.Empty;
    }
    public class InternalTrg
    {
        public string Trigger { get; set; }
        public string Method { get; set; }
    }
}
