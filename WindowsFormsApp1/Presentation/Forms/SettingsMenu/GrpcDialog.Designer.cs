namespace WindowsFormsApp1
{
    partial class GRPCDialog
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
            this.moonLabel1 = new ReaLTaiizor.Controls.MoonLabel();
            this.materialDivider1 = new ReaLTaiizor.Controls.MaterialDivider();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.bigLabel1 = new ReaLTaiizor.Controls.BigLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.testConnectionBtn = new ReaLTaiizor.Controls.ForeverButton();
            this.saveBtn = new ReaLTaiizor.Controls.ForeverButton();
            this.updateConfigBtn = new ReaLTaiizor.Controls.ForeverButton();
            this.urlHostGrpcTextBox = new ReaLTaiizor.Controls.BigTextBox();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // moonLabel1
            // 
            this.moonLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.moonLabel1.AutoSize = true;
            this.moonLabel1.BackColor = System.Drawing.Color.Transparent;
            this.moonLabel1.ForeColor = System.Drawing.Color.White;
            this.moonLabel1.Location = new System.Drawing.Point(3, 50);
            this.moonLabel1.Name = "moonLabel1";
            this.moonLabel1.Size = new System.Drawing.Size(56, 13);
            this.moonLabel1.TabIndex = 2;
            this.moonLabel1.Text = "Input URL";
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.White;
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialDivider1.Location = new System.Drawing.Point(3, 37);
            this.materialDivider1.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(652, 2);
            this.materialDivider1.TabIndex = 1;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 93.13869F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.861314F));
            this.tableLayoutPanel2.Controls.Add(this.bigLabel1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(652, 28);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // bigLabel1
            // 
            this.bigLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bigLabel1.AutoSize = true;
            this.bigLabel1.BackColor = System.Drawing.Color.Transparent;
            this.bigLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bigLabel1.ForeColor = System.Drawing.Color.White;
            this.bigLabel1.Location = new System.Drawing.Point(3, 2);
            this.bigLabel1.Name = "bigLabel1";
            this.bigLabel1.Size = new System.Drawing.Size(62, 24);
            this.bigLabel1.TabIndex = 0;
            this.bigLabel1.Text = "GRPC";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.materialDivider1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.moonLabel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.urlHostGrpcTextBox, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.85107F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.14894F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(658, 171);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.testConnectionBtn);
            this.flowLayoutPanel1.Controls.Add(this.saveBtn);
            this.flowLayoutPanel1.Controls.Add(this.updateConfigBtn);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 123);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(652, 45);
            this.flowLayoutPanel1.TabIndex = 8;
            // 
            // testConnectionBtn
            // 
            this.testConnectionBtn.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.testConnectionBtn.BackColor = System.Drawing.Color.Transparent;
            this.testConnectionBtn.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.testConnectionBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.testConnectionBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.testConnectionBtn.Location = new System.Drawing.Point(518, 3);
            this.testConnectionBtn.Name = "testConnectionBtn";
            this.testConnectionBtn.Rounded = true;
            this.testConnectionBtn.Size = new System.Drawing.Size(131, 38);
            this.testConnectionBtn.TabIndex = 6;
            this.testConnectionBtn.Text = "Test Connection";
            this.testConnectionBtn.TextColor = System.Drawing.Color.White;
            this.testConnectionBtn.Click += new System.EventHandler(this.testConnectionBtn_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.saveBtn.BackColor = System.Drawing.Color.Transparent;
            this.saveBtn.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.saveBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.saveBtn.Enabled = false;
            this.saveBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveBtn.Location = new System.Drawing.Point(433, 3);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Rounded = true;
            this.saveBtn.Size = new System.Drawing.Size(79, 38);
            this.saveBtn.TabIndex = 7;
            this.saveBtn.Text = "Save";
            this.saveBtn.TextColor = System.Drawing.Color.White;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // updateConfigBtn
            // 
            this.updateConfigBtn.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.updateConfigBtn.BackColor = System.Drawing.Color.Transparent;
            this.updateConfigBtn.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.updateConfigBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.updateConfigBtn.Enabled = false;
            this.updateConfigBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updateConfigBtn.Location = new System.Drawing.Point(304, 3);
            this.updateConfigBtn.Name = "updateConfigBtn";
            this.updateConfigBtn.Rounded = true;
            this.updateConfigBtn.Size = new System.Drawing.Size(123, 38);
            this.updateConfigBtn.TabIndex = 8;
            this.updateConfigBtn.Text = "Update Config";
            this.updateConfigBtn.TextColor = System.Drawing.Color.White;
            this.updateConfigBtn.Click += new System.EventHandler(this.updateConfigBtn_Click);
            // 
            // urlHostGrpcTextBox
            // 
            this.urlHostGrpcTextBox.BackColor = System.Drawing.Color.Transparent;
            this.urlHostGrpcTextBox.Font = new System.Drawing.Font("Tahoma", 11F);
            this.urlHostGrpcTextBox.ForeColor = System.Drawing.Color.Black;
            this.urlHostGrpcTextBox.Image = null;
            this.urlHostGrpcTextBox.Location = new System.Drawing.Point(3, 75);
            this.urlHostGrpcTextBox.MaxLength = 32767;
            this.urlHostGrpcTextBox.Multiline = false;
            this.urlHostGrpcTextBox.Name = "urlHostGrpcTextBox";
            this.urlHostGrpcTextBox.ReadOnly = false;
            this.urlHostGrpcTextBox.Size = new System.Drawing.Size(652, 41);
            this.urlHostGrpcTextBox.TabIndex = 9;
            this.urlHostGrpcTextBox.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.urlHostGrpcTextBox.UseSystemPasswordChar = false;
            // 
            // GRPCDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(658, 171);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GRPCDialog";
            this.RightToLeftLayout = true;
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private ReaLTaiizor.Controls.MoonLabel moonLabel1;
        private ReaLTaiizor.Controls.MaterialDivider materialDivider1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private ReaLTaiizor.Controls.BigLabel bigLabel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private ReaLTaiizor.Controls.ForeverButton testConnectionBtn;
        private ReaLTaiizor.Controls.ForeverButton saveBtn;
        private ReaLTaiizor.Controls.BigTextBox urlHostGrpcTextBox;
        private ReaLTaiizor.Controls.ForeverButton updateConfigBtn;
    }
}