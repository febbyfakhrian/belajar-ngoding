using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Core.Entities.Models { 
    public static class WorkflowModels
    {
        public class WorkflowDef
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<NodeDef> Nodes { get; set; }

            public WorkflowDef()
            {
                Id = string.Empty;
                Name = string.Empty;
                Nodes = new List<NodeDef>();
            }
        }

        public class NodeDef
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public List<string> DependsOn { get; set; }
            public Dictionary<string, object> Params { get; set; }
            public double? X { get; set; }
            public double? Y { get; set; }

            public NodeDef()
            {
                Id = string.Empty;
                Type = string.Empty;
                DependsOn = new List<string>();
                Params = new Dictionary<string, object>();
            }
        }
    }
}
