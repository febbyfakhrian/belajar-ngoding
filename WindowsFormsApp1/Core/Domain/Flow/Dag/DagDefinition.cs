using System.Collections.Generic;

namespace WindowsFormsApp1.Core.Domain.Flow.Dag
{
    public sealed class DagDefinition
    {
        public string Name { get; set; } = string.Empty;
        public List<DagNode> Nodes { get; set; }
        public List<DagEdge> Connections { get; set; }
    }

    public sealed class DagNode
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = "action"; // action | condition | trigger | delay | loop-controller
        public Dictionary<string, object> Parameters { get; set; }
    }

    public sealed class DagEdge
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string When { get; set; } // optional bool expression
    }
}