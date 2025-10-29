using ST.Library.UI.NodeEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class NodeEditorForm : Form
    {
        public NodeEditorForm()
        {
            InitializeComponent();
        }

        private void NodeEditorForm_Load(object sender, EventArgs e)
        {
            LoadWorkflowFromFile();
        }

        private void LoadWorkflowFromFile()
        {
            try
            {
                // Read the JSON file
                string jsonPath = Path.Combine(Application.StartupPath, "inspectionflow.json");
                if (!File.Exists(jsonPath))
                {
                    // Try alternative path
                    jsonPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "inspectionflow.json");
                }
                
                if (!File.Exists(jsonPath))
                {
                    MessageBox.Show("Workflow file not found: " + jsonPath);
                    return;
                }

                string jsonContent = File.ReadAllText(jsonPath);
                var workflow = JsonConvert.DeserializeObject<Workflow>(jsonContent);

                // Clear existing nodes
                stNodeEditor1.Nodes.Clear();

                // Create nodes
                int xPos = 50;
                int yPos = 50;
                int rowHeight = 150;
                int colWidth = 250;
                int currentRow = 0;
                int currentCol = 0;

                foreach (var node in workflow.Nodes)
                {
                    // Create a dynamic node class at runtime
                    DynamicNode dynamicNode = new DynamicNode(node.Name, node.Type, node.Parameters)
                    {
                        // Position nodes in a grid layout
                        Location = new Point(xPos + (currentCol * colWidth), yPos + (currentRow * rowHeight))
                    };

                    currentCol++;
                    if (currentCol > 3)
                    {
                        currentCol = 0;
                        currentRow++;
                    }

                    stNodeEditor1.Nodes.Add(dynamicNode);
                }
                
                // Auto arrange nodes for better visualization
                //stNodeEditor1.AutoArrangement();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading workflow: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }

    // Classes to represent the JSON structure
    public class Workflow
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("nodes")]
        public List<Node> Nodes { get; set; }

        [JsonProperty("connections")]
        public Dictionary<string, Dictionary<string, List<List<Connection>>>> Connections { get; set; }
    }

    public class Node
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters { get; set; }
    }

    public class Connection
    {
        [JsonProperty("node")]
        public string Node { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }
    }

    // Dynamic node class that can be configured at runtime
    public class DynamicNode : STNode
    {
        public string NodeType { get; set; }
        public string NodeId { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public DynamicNode(string title, string nodeType, Dictionary<string, object> parameters)
        {
            this.Title = title;
            this.NodeType = nodeType;
            this.Parameters = parameters;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            
            // Set node size
            this.Size = new Size(200, 100);
            
            // Add default input and output
            this.InputOptions.Add("main", typeof(object), true);
            this.OutputOptions.Add("main", typeof(object), false);
            
            // For split-batches nodes, add a "done" output
            if (this.NodeType?.ToLower() == "split-batches")
            {
                this.OutputOptions.Add("done", typeof(object), false);
            }
        }
        
        protected override void OnOwnerChanged()
        {
            base.OnOwnerChanged();
            
            // Set type colors when the node is added to the editor
            if (this.Owner != null)
            {
                // Set node color based on type
                switch (this.NodeType?.ToLower())
                {
                    case "action":
                        this.TitleColor = Color.FromArgb(200, Color.Green);
                        break;
                    case "trigger":
                        this.TitleColor = Color.FromArgb(200, Color.Blue);
                        break;
                    case "condition":
                        this.TitleColor = Color.FromArgb(200, Color.Orange);
                        break;
                    case "split-batches":
                        this.TitleColor = Color.FromArgb(200, Color.Purple);
                        break;
                    default:
                        this.TitleColor = Color.FromArgb(200, Color.Gray);
                        break;
                }
            }
        }
    }
}