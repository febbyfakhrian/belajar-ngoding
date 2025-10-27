using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GraphSharp.Controls;                // GraphLayout
using QuickGraph;                         // BidirectionalGraph
using Newtonsoft.Json;
using System.Windows.Forms.Integration;

namespace WindowsFormsApp1.Presentation.Flow
{
    public partial class DiagramConfigurationForm : Form
    {
        private readonly ElementHost _host = new ElementHost { Dock = DockStyle.Fill };
        private readonly GraphLayout _layout = new GraphLayout();

        public DiagramConfigurationForm()
        {
            InitializeComponent();
            Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inspectionflow.json");
            if (!File.Exists(jsonPath))
            {
                MessageBox.Show($"File {jsonPath} not found.");
                return;
            }

            try
            {
                var json = File.ReadAllText(jsonPath);
                var flow = JsonConvert.DeserializeObject<Flow>(json);
                if (flow == null)
                {
                    MessageBox.Show("Invalid JSON format");
                    return;
                }

                // Validate flow structure
                if (flow.Nodes == null || flow.Connections == null)
                {
                    MessageBox.Show("Invalid flow structure: nodes or connections are missing");
                    return;
                }

                BuildGraph(flow);

                _layout.LayoutAlgorithmType = "Sugiyama";          // Fruchterman-Reingold (DAG friendly)
                _layout.OverlapRemovalAlgorithmType = "FSA";
                _layout.HighlightAlgorithmType = "Simple";

                _host.Child = _layout;
                Controls.Add(_host);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void BuildGraph(Flow flow)
        {
            var graph = new BidirectionalGraph<object, IEdge<object>>();

            /* ---------- 1.  create vertices ---------- */
            // Check for null or empty nodes
            if (flow.Nodes == null)
            {
                MessageBox.Show("Flow nodes are null");
                return;
            }
            
            var vMap = flow.Nodes.ToDictionary(
                n => n.Name,
                n => new Vertex(n.Name, n.Type) as object);

            foreach (var v in vMap.Values) graph.AddVertex(v);

            /* ---------- 2.  create edges ---------- */
            // Check for null connections
            if (flow.Connections == null)
            {
                MessageBox.Show("Flow connections are null");
                _layout.Graph = graph;
                return;
            }
            
            foreach (var kv in flow.Connections)
            {
                AddEdge(graph, vMap, kv.Key, kv.Value.Main);
                AddEdge(graph, vMap, kv.Key, kv.Value.Done);
            }

            _layout.Graph = graph;
        }

        private static void AddEdge(
            BidirectionalGraph<object, IEdge<object>> g,   // ← bukan IBidirectionalGraph
            Dictionary<string, object> vMap,
            string from,
            List<List<EdgeDef>> lists)
        {
            if (lists == null) return;
            
            // Check if 'from' node exists in vMap
            if (!vMap.ContainsKey(from))
            {
                // Log or handle missing 'from' node
                return;
            }
            
            foreach (var grp in lists)
                foreach (var eDef in grp)
                {
                    // Check if target node exists in vMap
                    if (vMap.ContainsKey(eDef.Node))
                    {
                        g.AddEdge(new Edge<object>(vMap[from], vMap[eDef.Node]));
                    }
                    // Optionally log missing target nodes
                }
        }


        /* --------------------------------------------------
         *  JSON models (unchanged)
         * -------------------------------------------------- */
        public class Flow
        {
            public string Name { get; set; }
            public List<NodeDef> Nodes { get; set; }
            public Dictionary<string, PortDef> Connections { get; set; }
        }

        public class NodeDef
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public Dictionary<string, object> Parameters { get; set; }
        }

        public class PortDef
        {
            public List<List<EdgeDef>> Main { get; set; }
            public List<List<EdgeDef>> Done { get; set; }
        }

        public class EdgeDef
        {
            public string Node { get; set; }
            public string Key { get; set; }
            public int Index { get; set; }
        }

        /* --------------------------------------------------
         *  Vertex wrapper so we can attach colour/shape
         * -------------------------------------------------- */
        private sealed class Vertex
        {
            public string Name { get; }
            public string Type { get; }

            public Vertex(string name, string type)
            {
                Name = name;
                Type = type;
            }

            public override string ToString() => $"{Name}\n({Type})";
        }
    }
}