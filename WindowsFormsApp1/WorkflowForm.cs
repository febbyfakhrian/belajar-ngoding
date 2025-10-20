using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApp1.Models.WorkflowModels;
using WindowsFormsApp1.Activities;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;

namespace WindowsFormsApp1
{
    public partial class WorkflowForm: Form
    {
        private readonly GViewer _viewer = new GViewer();
        private Graph _graph = new Graph();

        public WorkflowForm()
        {
            InitializeComponent();
        }

        private async void WorkflowForm_LoadAsync(object sender, EventArgs e)
        {
            var wf = new WorkflowDef
                {
                    Id = "wf1",
                    Name = "Demo Workflow",
                    Nodes = new List<NodeDef>
                {
                    new NodeDef { Id = "start", Type = "Start" },
                    new NodeDef { Id = "fetch", Type = "HttpGet", DependsOn = new List<string>{"start"},
                        Params = new Dictionary<string, object>{{ "url", "https://jsonplaceholder.typicode.com/todos/1" }}},
                    new NodeDef { Id = "transform", Type = "Script", DependsOn = new List<string>{"fetch"},
                        Params = new Dictionary<string, object>{{ "source", "fetch.body" }}}
                }
            };

            // 🔹 TAMPILKAN GRAF DULU
            RenderWorkflowGraph(wf);

            var activities = new Dictionary<string, IActivity>
            {
                ["Start"] = new StartActivity(),
                ["HttpGet"] = new HttpGetActivity(),
                ["Script"] = new ScriptActivity()
            };

            var executor = new WorkflowExecutor(activities);
            await executor.RunAsync(wf);
        }

         private void RenderWorkflowGraph(WorkflowDef wf)
            {
                var g = new Graph();

                // Tambah node (kotak dengan label Type + Id)
                foreach (var n in wf.Nodes)
                {
                    var node = new Microsoft.Msagl.Drawing.Node(n.Id)
                    {
                        LabelText = $"{n.Type}\n({n.Id})"
                    };
                    node.Attr.Shape = Shape.Box;
                    g.AddNode(node);
                }

                // Tambah edge dari DependsOn -> Node
                foreach (var n in wf.Nodes)
                {
                    foreach (var dep in n.DependsOn)
                    {
                        var e = g.AddEdge(dep, n.Id);
                        e.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
                    }
                }

                _graph = g;
                _viewer.Graph = _graph;   // <- WAJIB: assign supaya tampil
                _viewer.Invalidate();
            }
        }
    }
