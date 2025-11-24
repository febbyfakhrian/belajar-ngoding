namespace WindowsFormsApp1.Presentation.Forms
{
    partial class CameraDebugForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.bigLabel1 = new ReaLTaiizor.Controls.BigLabel();
            this.materialDivider1 = new ReaLTaiizor.Controls.MaterialDivider();
            this.okButton = new ReaLTaiizor.Controls.ForeverButton();
            this.moonLabel1 = new ReaLTaiizor.Controls.MoonLabel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.enableRadioButton = new System.Windows.Forms.RadioButton();
            this.disableRadioButton = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.dungeonLabel1 = new ReaLTaiizor.Controls.DungeonLabel();
            this.chooseFileBtn = new ReaLTaiizor.Controls.CrownButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.materialDivider1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.okButton, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.moonLabel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 83.01887F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.98113F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(786, 169);
            this.tableLayoutPanel1.TabIndex = 2;
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
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(780, 26);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // bigLabel1
            // 
            this.bigLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bigLabel1.AutoSize = true;
            this.bigLabel1.BackColor = System.Drawing.Color.Transparent;
            this.bigLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bigLabel1.ForeColor = System.Drawing.Color.White;
            this.bigLabel1.Location = new System.Drawing.Point(3, 1);
            this.bigLabel1.Name = "bigLabel1";
            this.bigLabel1.Size = new System.Drawing.Size(163, 24);
            this.bigLabel1.TabIndex = 0;
            this.bigLabel1.Text = "Setting Photo Path";
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.White;
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialDivider1.Location = new System.Drawing.Point(3, 35);
            this.materialDivider1.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(780, 1);
            this.materialDivider1.TabIndex = 1;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.okButton.BackColor = System.Drawing.Color.Transparent;
            this.okButton.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.okButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.okButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.okButton.Location = new System.Drawing.Point(324, 136);
            this.okButton.Name = "okButton";
            this.okButton.Rounded = true;
            this.okButton.Size = new System.Drawing.Size(137, 29);
            this.okButton.TabIndex = 9;
            this.okButton.Text = "OK";
            this.okButton.TextColor = System.Drawing.Color.White;
            // 
            // moonLabel1
            // 
            this.moonLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.moonLabel1.AutoSize = true;
            this.moonLabel1.BackColor = System.Drawing.Color.Transparent;
            this.moonLabel1.ForeColor = System.Drawing.Color.White;
            this.moonLabel1.Location = new System.Drawing.Point(3, 76);
            this.moonLabel1.Name = "moonLabel1";
            this.moonLabel1.Size = new System.Drawing.Size(60, 13);
            this.moonLabel1.TabIndex = 2;
            this.moonLabel1.Text = "Upload File";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.enableRadioButton);
            this.flowLayoutPanel2.Controls.Add(this.disableRadioButton);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 41);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(780, 23);
            this.flowLayoutPanel2.TabIndex = 10;
            // 
            // enableRadioButton
            // 
            this.enableRadioButton.AutoSize = true;
            this.enableRadioButton.ForeColor = System.Drawing.Color.White;
            this.enableRadioButton.Location = new System.Drawing.Point(3, 3);
            this.enableRadioButton.Name = "enableRadioButton";
            this.enableRadioButton.Size = new System.Drawing.Size(58, 17);
            this.enableRadioButton.TabIndex = 0;
            this.enableRadioButton.TabStop = true;
            this.enableRadioButton.Text = "Enable";
            this.enableRadioButton.UseVisualStyleBackColor = true;
            // 
            // disableRadioButton
            // 
            this.disableRadioButton.AutoSize = true;
            this.disableRadioButton.ForeColor = System.Drawing.SystemColors.Control;
            this.disableRadioButton.Location = new System.Drawing.Point(67, 3);
            this.disableRadioButton.Name = "disableRadioButton";
            this.disableRadioButton.Size = new System.Drawing.Size(60, 17);
            this.disableRadioButton.TabIndex = 1;
            this.disableRadioButton.TabStop = true;
            this.disableRadioButton.Text = "Disable";
            this.disableRadioButton.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 690F));
            this.tableLayoutPanel3.Controls.Add(this.dungeonLabel1, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.chooseFileBtn, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 101);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(780, 28);
            this.tableLayoutPanel3.TabIndex = 11;
            // 
            // dungeonLabel1
            // 
            this.dungeonLabel1.AutoEllipsis = true;
            this.dungeonLabel1.AutoSize = true;
            this.dungeonLabel1.BackColor = System.Drawing.Color.Transparent;
            this.dungeonLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dungeonLabel1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.dungeonLabel1.ForeColor = System.Drawing.Color.White;
            this.dungeonLabel1.Location = new System.Drawing.Point(93, 0);
            this.dungeonLabel1.Name = "dungeonLabel1";
            this.dungeonLabel1.Size = new System.Drawing.Size(684, 28);
            this.dungeonLabel1.TabIndex = 2;
            this.dungeonLabel1.Text = "File has been not selected";
            this.dungeonLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chooseFileBtn
            // 
            this.chooseFileBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(82)))), ((int)(((byte)(82)))), ((int)(((byte)(80)))));
            this.chooseFileBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chooseFileBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chooseFileBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chooseFileBtn.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.chooseFileBtn.Location = new System.Drawing.Point(0, 0);
            this.chooseFileBtn.Margin = new System.Windows.Forms.Padding(0);
            this.chooseFileBtn.Name = "chooseFileBtn";
            this.chooseFileBtn.Padding = new System.Windows.Forms.Padding(5);
            this.chooseFileBtn.Size = new System.Drawing.Size(90, 28);
            this.chooseFileBtn.TabIndex = 1;
            this.chooseFileBtn.Text = "Choose File";
            // 
            // CameraDebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(786, 169);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CameraDebugForm";
            this.Text = "CameraDebugForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private ReaLTaiizor.Controls.ForeverButton okButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private ReaLTaiizor.Controls.BigLabel bigLabel1;
        private ReaLTaiizor.Controls.MaterialDivider materialDivider1;
        private ReaLTaiizor.Controls.MoonLabel moonLabel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.RadioButton enableRadioButton;
        private System.Windows.Forms.RadioButton disableRadioButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private ReaLTaiizor.Controls.CrownButton chooseFileBtn;
        private ReaLTaiizor.Controls.DungeonLabel dungeonLabel1;
    }
}