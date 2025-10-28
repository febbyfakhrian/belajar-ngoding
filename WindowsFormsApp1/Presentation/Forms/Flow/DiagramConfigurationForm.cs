using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GraphSharp.Controls;        // GraphLayout
using QuickGraph;                 // BidirectionalGraph
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
                if (flow == null || flow.Nodes == null || flow.Connections == null)
                {
                    MessageBox.Show("Invalid JSON or empty graph.");
                    return;
                }

                // 1.  buat graph, isi vertex & edge
                var graph = BuildGraph(flow);

                // 2.  baru set algoritma
                _layout.OverlapRemovalAlgorithmType = "FSA";
                _layout.LayoutAlgorithmType = "FR";
                _layout.HighlightAlgorithmType = "Simple";

                // 3.  assign graph -> layout & overlap-removal dijalankan
                _layout.Graph = graph;

                // 4.  tampilkan
                _host.Child = _layout;
                Controls.Add(_host);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /* ---------- bangun graph, return instance YANG SUDAH TERISI ---------- */
        private BidirectionalGraph<object, IEdge<object>> BuildGraph(Flow flow)
        {
            var graph = new BidirectionalGraph<object, IEdge<object>>();
            var vMap = flow.Nodes.ToDictionary(n => n.Name,
                                                n => new Vertex(n.Name, n.Type) as object);

            // vertex
            foreach (var v in vMap.Values) graph.AddVertex(v);

            // edge
            foreach (var kv in flow.Connections)
            {
                AddEdges(graph, vMap, kv.Key, kv.Value.Main);
                AddEdges(graph, vMap, kv.Key, kv.Value.Done);
            }

            return graph;   // pastikan tidak ada return di atas baris ini
        }

        private static void AddEdges(BidirectionalGraph<object, IEdge<object>> g,
                                   Dictionary<string, object> vMap,
                                   string from,
                                   List<List<EdgeDef>> lists)
        {
            if (lists == null) return;
            if (!vMap.ContainsKey(from)) return;

            foreach (var grp in lists)
                foreach (var e in grp)
                    if (vMap.ContainsKey(e.Node))
                        g.AddEdge(new Edge<object>(vMap[from], vMap[e.Node]));
        }

        /* ---------------- JSON model ---------------- */
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