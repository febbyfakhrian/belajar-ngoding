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
            stNodeEditor1.Nodes.Add(new MyNode());
        }
    }

    public class MyNode : STNode
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            this.Title = "MyNode";
            this.TitleColor = Color.FromArgb(200, Color.Goldenrod);
            this.AutoSize = false;
            this.Size = new Size(100, 100);

            var ctrl = new STNodeControl();
            ctrl.Text = "Button";
            ctrl.Location = new Point(10, 10);
            this.Controls.Add(ctrl);
            ctrl.MouseClick += new MouseEventHandler(ctrl_MouseClick);
        }

        void ctrl_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("MouseClick");
        }
    }

}
