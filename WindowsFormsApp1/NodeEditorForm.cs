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
using static WindowsFormsApp1.NodeEditorForm;

namespace WindowsFormsApp1
{
    public partial class NodeEditorForm : Form
    {
        public NodeEditorForm()
        {
            InitializeComponent();
            RegisterCameraActionsToTree();
            stNodeEditorPannel1.TreeView.AddNode(typeof(PlcReadReceivedTriggerNode));
            stNodeEditorPannel1.TreeView.AddNode(typeof(CameraStartedTriggerNode));
            stNodeEditorPannel1.Editor.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                {
                    var selected = stNodeEditorPannel1.Editor.GetSelectedNode().ToList();
                    if (selected.Count == 0) return;

                    // (optional) confirm deletion
                    if (MessageBox.Show($"Delete {selected.Count} selected node(s)?",
                                        "Confirm",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        foreach (var node in selected)
                            stNodeEditorPannel1.Editor.Nodes.Remove(node);
                    }
                }
            };
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
                stNodeEditorPannel1.Editor.Nodes.Clear();

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

                    stNodeEditorPannel1.Editor.Nodes.Add(dynamicNode);
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

                        stNodeEditorPannel1.Editor.Refresh();
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


        private void RegisterCameraActionsToTree()
        {
            // 1) Assembly tempat aksi kamera didefinisikan
            var asm = typeof(WindowsFormsApp1.Core.Domain.Actions.CameraPrepareAction).Assembly;

            // 2) Filter: kelas konkrit di namespace kamera dan turunan BaseAction
            var baseActionType = typeof(WindowsFormsApp1.Core.Domain.Actions.BaseAction); // sesuaikan jika namespace BaseAction berbeda
            var camNs = "WindowsFormsApp1.Core.Domain.Actions";

            var actionTypes =
                asm.GetTypes()
                   .Where(t => t.IsClass && !t.IsAbstract)
                   .Where(t => t.Namespace == camNs)
                   .Where(t => baseActionType.IsAssignableFrom(t))
                   .ToList();

            // 3) Untuk tiap aksi, buat closed generic ActionNode<TAction> dan tambahkan ke tree
            foreach (var actType in actionTypes)
            {
                var nodeType = typeof(ActionNode<>).MakeGenericType(actType);
                stNodeEditorPannel1.TreeView.AddNode(nodeType); // kategori
            }

            // (Opsional) refresh editor/panel
            stNodeEditorPannel1.Editor.Refresh();
        }

        private void saveNodeBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // === Buka dialog untuk pilih lokasi & nama file ===
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Simpan Workflow Node";
                    saveDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                    saveDialog.DefaultExt = "json";
                    saveDialog.FileName = "inspectionflow.json";
                    saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    if (saveDialog.ShowDialog() != DialogResult.OK)
                        return;

                    string jsonPath = saveDialog.FileName;

                    // === Siapkan struktur workflow ===
                    var workflow = new Workflow
                    {
                        Name = Path.GetFileNameWithoutExtension(jsonPath),
                        Id = Guid.NewGuid().ToString(),
                        Nodes = new List<Node>(),
                        Connections = new Dictionary<string, Dictionary<string, List<List<Connection>>>>()
                    };

                    // === Simpan semua node ===
                    foreach (STNode node in stNodeEditorPannel1.Editor.Nodes)
                    {
                        if (node == null) continue;

                        var newNode = new Node
                        {
                            Id = node.Guid.ToString(),
                            Name = node.Title,
                            Type = node.GetType().Name,
                            Parameters = new Dictionary<string, object>
                            {
                                ["X"] = node.Location.X,
                                ["Y"] = node.Location.Y
                            }
                        };

                        workflow.Nodes.Add(newNode);
                    }

                    // === Simpan semua koneksi antar node ===
                    foreach (STNode fromNode in stNodeEditorPannel1.Editor.Nodes)
                    {
                        if (fromNode == null) continue;

                        var outputs = fromNode.GetOutputOptions();
                        if (outputs == null) continue;

                        var outDict = new Dictionary<string, List<List<Connection>>>(StringComparer.OrdinalIgnoreCase);

                        foreach (var output in outputs)
                        {
                            if (output == null) continue;

                            var connected = output.GetConnectedOption();
                            if (connected == null || connected.Count == 0) continue;

                            var connList = new List<Connection>();
                            foreach (var conn in connected)
                            {
                                var toNode = conn?.Owner;
                                if (toNode == null) continue;

                                connList.Add(new Connection
                                {
                                    Node = string.IsNullOrWhiteSpace(toNode.Title) ? toNode.GetType().Name : toNode.Title,
                                    Key = string.IsNullOrWhiteSpace(conn.Text) ? "main" : conn.Text,
                                    Index = 0
                                });
                            }

                            if (connList.Count > 0)
                            {
                                string key = string.IsNullOrWhiteSpace(output.Text) ? "main" : output.Text;
                                outDict[key] = new List<List<Connection>> { connList };
                            }
                        }

                        if (outDict.Count > 0)
                        {
                            string fromKey = !string.IsNullOrWhiteSpace(fromNode.Title)
                                ? fromNode.Title
                                : fromNode.GetType().Name;

                            workflow.Connections[fromKey] = outDict;
                        }
                    }

                    // === Serialize ke JSON & simpan ===
                    var json = JsonConvert.SerializeObject(workflow, Formatting.Indented);
                    File.WriteAllText(jsonPath, json);

                    MessageBox.Show(
                        $"Workflow berhasil disimpan!\n\nLokasi:\n{jsonPath}",
                        "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan workflow:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                this.LetGetOptions = true;

                // Buat ukuran & slot dasar (tidak bergantung NodeType)
                this.Size = new Size(200, 100);
                _inMain = this.InputOptions.Add("main", typeof(object), true);
                _outMain = this.OutputOptions.Add("main", typeof(object), false);
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

        [STNode("Actions")]
        public class ActionNode<TAction> : STNode
        {
            private STNodeOption _inMain;
            private STNodeOption _outMain;

            protected override void OnCreate()
            {
                base.OnCreate();

                this.LetGetOptions = true;

                // Judul node = nama tipe aksi
                var actionType = typeof(TAction);
                this.Title = actionType.Name;                // contoh: CameraPrepareAction
                this.Size = new Size(200, 90);

                // Port standar untuk alur
                _inMain = this.InputOptions.Add("in", typeof(object), true);   // bisa menerima 1+ koneksi
                _outMain = this.OutputOptions.Add("out", typeof(object), false); // 1 koneksi keluar

                // Opsional: warna/ikon khusus untuk grup "Camera"
                this.TitleColor = Color.FromArgb(200, Color.SteelBlue);
            }

            // Properti expose jika perlu
            public STNodeOption InMain => _inMain;
            public STNodeOption OutMain => _outMain;
        }


        [STNode("Triggers")]
        public class PlcReadReceivedTriggerNode : STNode
        {
            private STNodeOption _inMain;
            private STNodeOption _outMain;

            private string _type;

            // Property 2: String
            [STNodeProperty("Type", "This is a string property.")]
            public string Type
            {
                get { return _type; }
                private set { _type = value;  }
            }

            protected override void OnCreate()
            {
                base.OnCreate();

                this.LetGetOptions = true;
                _type = "test";

                // Judul node = nama tipe aksi
                this.Title = "PLC_READ_RECEIVED";                
                this.Size = new Size(200, 90);

                // Port standar untuk alur
                _inMain = this.InputOptions.Add("in", typeof(object), true);   // bisa menerima 1+ koneksi
                _outMain = this.OutputOptions.Add("out", typeof(object), false); // 1 koneksi keluar

                // Opsional: warna/ikon khusus untuk grup "Camera"
                this.TitleColor = Color.FromArgb(200, Color.SteelBlue);
            }

            // Properti expose jika perlu
            public STNodeOption InMain => _inMain;
            public STNodeOption OutMain => _outMain;
        }

        [STNode("Triggers")]
        public class CameraStartedTriggerNode : STNode
        {
            private STNodeOption _inMain;
            private STNodeOption _outMain;

            protected override void OnCreate()
            {
                base.OnCreate();

                this.LetGetOptions = true;

                // Judul node = nama tipe aksi
                this.Title = "CAMERA_STARTED";                
                this.Size = new Size(200, 90);

                // Port standar untuk alur
                _inMain = this.InputOptions.Add("in", typeof(object), true);   // bisa menerima 1+ koneksi
                _outMain = this.OutputOptions.Add("out", typeof(object), false); // 1 koneksi keluar

                // Opsional: warna/ikon khusus untuk grup "Camera"
                this.TitleColor = Color.FromArgb(200, Color.SteelBlue);
            }

            // Properti expose jika perlu
            public STNodeOption InMain => _inMain;
            public STNodeOption OutMain => _outMain;
        }


        private void loadConfigBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Title = "Pilih File Workflow JSON";
                    openDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                    openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    if (openDialog.ShowDialog() != DialogResult.OK)
                        return; // batal

                    string jsonPath = openDialog.FileName;

                    if (!File.Exists(jsonPath))
                    {
                        MessageBox.Show("File tidak ditemukan!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Baca JSON dan deserialisasi
                    string jsonContent = File.ReadAllText(jsonPath);
                    var workflow = JsonConvert.DeserializeObject<Workflow>(jsonContent);

                    if (workflow == null)
                    {
                        MessageBox.Show("File JSON tidak valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Bersihkan node lama di editor
                    stNodeEditorPannel1.Editor.Nodes.Clear();

                    // Buat node baru dari data JSON
                    var nodeMap = new Dictionary<string, STNode>();

                    foreach (var nodeData in workflow.Nodes)
                    {
                        // Buat DynamicNode (atau node biasa) sesuai type
                        var dynamicNode = new DynamicNode(nodeData.Name, nodeData.Type, nodeData.Parameters ?? new Dictionary<string, object>())
                        {
                            Location = new Point(
                                nodeData.Parameters != null && nodeData.Parameters.ContainsKey("X") ? Convert.ToInt32(nodeData.Parameters["X"]) : 50,
                                nodeData.Parameters != null && nodeData.Parameters.ContainsKey("Y") ? Convert.ToInt32(nodeData.Parameters["Y"]) : 50
                            )
                        };

                        // Tambahkan ke editor
                        stNodeEditorPannel1.Editor.Nodes.Add(dynamicNode);
                        nodeMap[nodeData.Name] = dynamicNode;
                    }

                    // Buat koneksi antar node dari workflow.Connections
                    //    Gunakan Timer kecil agar node sudah attach ke editor sebelum koneksi dibuat
                    var t = new System.Windows.Forms.Timer { Interval = 100 };
                    t.Tick += (s2, e2) =>
                    {
                        if (nodeMap.Values.All(n => n.Owner != null))
                        {
                            t.Stop();

                            foreach (var fromNodeEntry in workflow.Connections)
                            {
                                string fromName = fromNodeEntry.Key;
                                if (!nodeMap.TryGetValue(fromName, out var fromNode)) continue;

                                var fromOutputs = fromNode.GetOutputOptions();
                                if (fromOutputs == null) continue;

                                foreach (var outputKeyPair in fromNodeEntry.Value)
                                {
                                    string fromSlotKey = outputKeyPair.Key;
                                    var connLists = outputKeyPair.Value;

                                    var fromOut = fromOutputs.FirstOrDefault(o => o.Text == fromSlotKey);
                                    if (fromOut == null) continue;

                                    foreach (var list in connLists)
                                    {
                                        foreach (var conn in list)
                                        {
                                            if (!nodeMap.TryGetValue(conn.Node, out var toNode)) continue;

                                            var toInputs = toNode.GetInputOptions();
                                            var toIn = toInputs?.FirstOrDefault(i => i.Text == conn.Key);
                                            if (toIn != null)
                                            {
                                                try
                                                {
                                                    fromOut.ConnectOption(toIn);
                                                }
                                                catch { /* Abaikan koneksi invalid */ }
                                            }
                                        }
                                    }
                                }
                            }

                            stNodeEditorPannel1.Editor.Refresh();
                        }
                    };
                    t.Start();

                    // Beri notifikasi
                    MessageBox.Show($"Workflow '{workflow.Name}' berhasil dimuat!\n\nLokasi: {jsonPath}",
                                    "Loaded",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat konfigurasi:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}