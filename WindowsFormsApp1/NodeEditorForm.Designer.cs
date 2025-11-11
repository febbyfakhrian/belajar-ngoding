namespace WindowsFormsApp1
{
    partial class NodeEditorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.stNodeEditorPannel1 = new ST.Library.UI.NodeEditor.STNodeEditorPannel();
            this.saveNodeBtn = new ReaLTaiizor.Controls.CrownButton();
            this.loadConfigBtn = new ReaLTaiizor.Controls.CrownButton();
            this.SuspendLayout();
            // 
            // stNodeEditorPannel1
            // 
            this.stNodeEditorPannel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
            this.stNodeEditorPannel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stNodeEditorPannel1.Location = new System.Drawing.Point(0, 0);
            this.stNodeEditorPannel1.MinimumSize = new System.Drawing.Size(250, 250);
            this.stNodeEditorPannel1.Name = "stNodeEditorPannel1";
            this.stNodeEditorPannel1.Size = new System.Drawing.Size(1566, 867);
            this.stNodeEditorPannel1.TabIndex = 3;
            this.stNodeEditorPannel1.Text = "stNodeEditorPannel1";
            this.stNodeEditorPannel1.Y = 250;
            // 
            // saveNodeBtn
            // 
            this.saveNodeBtn.Location = new System.Drawing.Point(219, 12);
            this.saveNodeBtn.Name = "saveNodeBtn";
            this.saveNodeBtn.Padding = new System.Windows.Forms.Padding(5);
            this.saveNodeBtn.Size = new System.Drawing.Size(115, 31);
            this.saveNodeBtn.TabIndex = 5;
            this.saveNodeBtn.Text = "Save Node To Json";
            this.saveNodeBtn.Click += new System.EventHandler(this.saveNodeBtn_Click);
            // 
            // loadConfigBtn
            // 
            this.loadConfigBtn.Location = new System.Drawing.Point(340, 12);
            this.loadConfigBtn.Name = "loadConfigBtn";
            this.loadConfigBtn.Padding = new System.Windows.Forms.Padding(5);
            this.loadConfigBtn.Size = new System.Drawing.Size(86, 31);
            this.loadConfigBtn.TabIndex = 6;
            this.loadConfigBtn.Text = "Load Config";
            this.loadConfigBtn.Click += new System.EventHandler(this.loadConfigBtn_Click);
            // 
            // NodeEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1566, 867);
            this.Controls.Add(this.loadConfigBtn);
            this.Controls.Add(this.saveNodeBtn);
            this.Controls.Add(this.stNodeEditorPannel1);
            this.Name = "NodeEditorForm";
            this.Text = "NodeEditorForm";
            this.Load += new System.EventHandler(this.NodeEditorForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ST.Library.UI.NodeEditor.STNodeEditorPannel stNodeEditorPannel1;
        private ReaLTaiizor.Controls.CrownButton saveNodeBtn;
        private ReaLTaiizor.Controls.CrownButton loadConfigBtn;
    }
}