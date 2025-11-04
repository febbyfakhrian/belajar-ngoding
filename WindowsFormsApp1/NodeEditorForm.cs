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

                var nodeMap = new Dictionary<string, DynamicNode>(StringComparer.OrdinalIgnoreCase);

                foreach (var node in workflow.Nodes)
                {
                    var dynamicNode = new DynamicNode(node.Name, node.Type, node.Parameters)
                    {
                        Location = new Point(xPos + (currentCol * colWidth), yPos + (currentRow * rowHeight))
                    };

                    currentCol++;
                    if (currentCol > 3) { currentCol = 0; currentRow++; }

                    stNodeEditor1.Nodes.Add(dynamicNode);
                    nodeMap[node.Name] = dynamicNode; // simpan untuk koneksi
                }


                // Pastikan semua node sudah punya Owner dan slot utama siap
                if (nodeMap.Values.All(n => n.Owner != null && n.InMain != null && n.OutMain != null))
                {

                    // Helper untuk ambil slot output berdasarkan key
                    STNodeOption GetOut(DynamicNode n, string key)
                    {
                        if (string.Equals(key, "main", StringComparison.OrdinalIgnoreCase)) return n.OutMain;
                        if (string.Equals(key, "done", StringComparison.OrdinalIgnoreCase)) return n.OutDone;
                        return null;
                    }

                    // Helper untuk ambil slot input berdasarkan key (saat ini hanya "main" yg ada)
                    STNodeOption GetIn(DynamicNode n, string key)
                    {
                        if (string.Equals(key, "main", StringComparison.OrdinalIgnoreCase)) return n.InMain;
                        // Tambah mapping lain jika nanti kamu punya input berbeda
                        return null;
                    }

                    try
                    {
                        // Iterasi seluruh koneksi dari JSON
                        foreach (var fromNodeEntry in workflow.Connections) // from-name
                        {
                            var fromName = fromNodeEntry.Key;
                            if (!nodeMap.TryGetValue(fromName, out var fromNode)) continue;

                            var dictByKey = fromNodeEntry.Value; // e.g. "main", "done"
                            foreach (var keyEntry in dictByKey)
                            {
                                var fromKey = keyEntry.Key;                // "main" / "done"
                                var listOfLists = keyEntry.Value;          // List<List<Connection>>
                                var outSlot = GetOut(fromNode, fromKey);
                                if (outSlot == null) continue;

                                foreach (var group in listOfLists)
                                {
                                    if (group == null) continue;
                                    foreach (var conn in group)
                                    {
                                        // conn.node: target name, conn.key: target slot (biasanya "main")
                                        if (!nodeMap.TryGetValue(conn.Node, out var toNode)) continue;
                                        var inSlot = GetIn(toNode, conn.Key);
                                        if (inSlot == null) continue;

                                        // KONEKSI INTI: pakai ConnectOption di level slot
                                        outSlot.ConnectOption(inSlot);
                                    }
                                }
                            }
                        }

                        stNodeEditor1.Refresh();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal membuat koneksi: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading workflow: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }

    public class NodeA : STNode
    {
        private STNodeOption _out;
        protected override void OnCreate()
        {
            base.OnCreate();
            Title = "NodeA";
            _out = this.OutputOptions.Add("OutData", typeof(int), false);
        }
        public STNodeOption OutDataSlot => _out;
    }

    public class NodeB : STNode
    {
        private STNodeOption _in;
        protected override void OnCreate()
        {
            base.OnCreate();
            Title = "NodeB";
            _in = this.InputOptions.Add("InData", typeof(int), true); // izinkan koneksi, selaras NumberAddNode
        }
        public STNodeOption InDataSlot => _in;
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

        private STNodeOption _inMain;
        private STNodeOption _outMain;
        private STNodeOption _outDone;

        public STNodeOption InMain => _inMain;
        public STNodeOption OutMain => _outMain;
        public STNodeOption OutDone => _outDone;

        public DynamicNode(string title, string nodeType, Dictionary<string, object> parameters)
        {
            this.Title = title;
            this.NodeType = nodeType; 
            this.Parameters = parameters;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            // Buat ukuran & slot dasar (tidak bergantung NodeType)
            this.Size = new Size(200, 100);
            _inMain = this.InputOptions.Add("main", typeof(object), true);
            _outMain = this.OutputOptions.Add("main", typeof(object), false);

            // JANGAN cek NodeType di sini (belum pasti terisi)
            // Console.WriteLine($"OnCreate NodeType? {this.NodeType}"); // bisa null
        }

        protected override void OnOwnerChanged()
        {
            base.OnOwnerChanged();

            if (this.Owner == null) return;

            // Pada tahap ini NodeType sudah terisi -> aman
            // Tambah "done" khusus split-batches DI SINI
            if (this.NodeType?.ToLower() == "split-batches" && _outDone == null)
            {
                _outDone = this.OutputOptions.Add("done", typeof(object), false);
            }

            // Pewarnaan judul berdasarkan tipe (kode kamu sudah begini)
            switch (this.NodeType?.ToLower())
            {
                case "action": this.TitleColor = Color.FromArgb(200, Color.Green); break;
                case "trigger": this.TitleColor = Color.FromArgb(200, Color.Blue); break;
                case "condition": this.TitleColor = Color.FromArgb(200, Color.Orange); break;
                case "split-batches": this.TitleColor = Color.FromArgb(200, Color.Purple); break;
                default: this.TitleColor = Color.FromArgb(200, Color.Gray); break;
            }
        }
    }
}