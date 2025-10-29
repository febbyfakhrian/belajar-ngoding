using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApp1.Core.Entities.Models
{
    public class Workflow
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public List<Node> Nodes { get; set; }
        public Dictionary<string, NodeConnections> Connections { get; set; }
    }

    public class Node
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    public class NodeConnections
    {
        public List<List<Connection>> Main { get; set; }
        public List<List<Connection>> Done { get; set; }
    }

    public class Connection
    {
        public string Node { get; set; }
        public string Key { get; set; }
        public int Index { get; set; }
    }
}