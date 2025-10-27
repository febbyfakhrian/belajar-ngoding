using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Color = Microsoft.Msagl.Drawing.Color;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Presentation.Flow
{
    public partial class DiagramConfigurationForm : Form
    {
        private readonly GViewer viewer = new GViewer();
        private readonly Graph graph = new Graph("DAG");

        public DiagramConfigurationForm()
        {
            InitializeComponent();
            Load += DiagramConfigurationForm_Load;
        }

        private void DiagramConfigurationForm_Load(object sender, EventArgs e)
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

                // CHANGED: Newtonsoft.Json
                var flow = JsonConvert.DeserializeObject<Flow>(json);
                if (flow == null)
                {
                    MessageBox.Show("Invalid JSON format");
                    return;
                }

                BuildGraph(flow);

                viewer.Graph = graph;
                viewer.Dock = DockStyle.Fill;
                viewer.ToolBarIsVisible = true;
                viewer.LayoutAlgorithmSettingsButtonVisible = true;
                Controls.Add(viewer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void BuildGraph(Flow flow)
        {
            var nodes = flow.Nodes.ToDictionary(n => n.Name, n => n);

            // Add nodes
            foreach (var node in flow.Nodes)
            {
                var gNode = graph.AddNode(node.Name);
                gNode.LabelText = $"{node.Name}\n({node.Type})";

                switch (node.Type)
                {
                    case "trigger":
                        gNode.Attr.FillColor = Color.LightSkyBlue;
                        gNode.Attr.Shape = Shape.DoubleCircle;
                        break;
                    case "action":
                        gNode.Attr.FillColor = Color.LightGreen;
                        gNode.Attr.Shape = Shape.Box;
                        break;
                    case "split-batches":
                        gNode.Attr.FillColor = Color.Orange;
                        gNode.Attr.Shape = Shape.Diamond;
                        break;
                    default:
                        gNode.Attr.FillColor = Color.LightGray;
                        gNode.Attr.Shape = Shape.Ellipse;
                        break;
                }

                gNode.Attr.LineWidth = 1.5;
            }

            // Add edges
            foreach (var kv in flow.Connections)
            {
                AddEdges(kv.Key, kv.Value.Main);
                AddEdges(kv.Key, kv.Value.Done);
            }
        }

        private void AddEdges(string from, List<List<EdgeDef>> lists)
        {
            if (lists == null) return;  // aman walau null
            foreach (var group in lists)
            {
                foreach (var edge in group)
                {
                    var e = graph.AddEdge(from, edge.Node);
                    e.LabelText = edge.Key;
                    e.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
                }
            }
        }
    }

    // JSON models
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
}
