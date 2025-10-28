namespace WindowsFormsApp1
{
    partial class MESDialog
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.bigLabel1 = new ReaLTaiizor.Controls.BigLabel();
            this.materialDivider1 = new ReaLTaiizor.Controls.MaterialDivider();
            this.moonLabel1 = new ReaLTaiizor.Controls.MoonLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.onlineRadioButton = new ReaLTaiizor.Controls.HopeRadioButton();
            this.offlineRadioButton = new ReaLTaiizor.Controls.HopeRadioButton();
            this.cyberRichTextBox1 = new ReaLTaiizor.Controls.CyberRichTextBox();
            this.foreverButton1 = new ReaLTaiizor.Controls.ForeverButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.materialDivider1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.moonLabel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.cyberRichTextBox1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.foreverButton1, 0, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 86.48649F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.51351F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(665, 184);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 93.13869F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.861314F));
            this.tableLayoutPanel2.Controls.Add(this.pictureBox1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.bigLabel1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(659, 21);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::WindowsFormsApp1.Properties.Resources.x;
            this.pictureBox1.Location = new System.Drawing.Point(624, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(24, 15);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // bigLabel1
            // 
            this.bigLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bigLabel1.AutoSize = true;
            this.bigLabel1.BackColor = System.Drawing.Color.Transparent;
            this.bigLabel1.Font = new System.Drawing.Font("Arial Rounded MT Bold", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bigLabel1.ForeColor = System.Drawing.Color.White;
            this.bigLabel1.Location = new System.Drawing.Point(3, 0);
            this.bigLabel1.Name = "bigLabel1";
            this.bigLabel1.Size = new System.Drawing.Size(52, 21);
            this.bigLabel1.TabIndex = 0;
            this.bigLabel1.Text = "MES";
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.White;
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialDivider1.Location = new System.Drawing.Point(3, 30);
            this.materialDivider1.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(659, 1);
            this.materialDivider1.TabIndex = 1;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // moonLabel1
            // 
            this.moonLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.moonLabel1.AutoSize = true;
            this.moonLabel1.BackColor = System.Drawing.Color.Transparent;
            this.moonLabel1.ForeColor = System.Drawing.Color.White;
            this.moonLabel1.Location = new System.Drawing.Point(3, 35);
            this.moonLabel1.Name = "moonLabel1";
            this.moonLabel1.Size = new System.Drawing.Size(56, 13);
            this.moonLabel1.TabIndex = 2;
            this.moonLabel1.Text = "Input URL";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.onlineRadioButton);
            this.flowLayoutPanel1.Controls.Add(this.offlineRadioButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 107);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(659, 29);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // onlineRadioButton
            // 
            this.onlineRadioButton.AutoSize = true;
            this.onlineRadioButton.CheckedColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.onlineRadioButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.onlineRadioButton.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(198)))), ((int)(((byte)(202)))));
            this.onlineRadioButton.DisabledStringColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(187)))), ((int)(((byte)(189)))));
            this.onlineRadioButton.Enable = true;
            this.onlineRadioButton.EnabledCheckedColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.onlineRadioButton.EnabledStringColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(146)))), ((int)(((byte)(146)))));
            this.onlineRadioButton.EnabledUncheckedColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(158)))), ((int)(((byte)(161)))));
            this.onlineRadioButton.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.onlineRadioButton.ForeColor = System.Drawing.Color.White;
            this.onlineRadioButton.Location = new System.Drawing.Point(3, 3);
            this.onlineRadioButton.Name = "onlineRadioButton";
            this.onlineRadioButton.Size = new System.Drawing.Size(81, 20);
            this.onlineRadioButton.TabIndex = 1;
            this.onlineRadioButton.Text = "Online";
            this.onlineRadioButton.UseVisualStyleBackColor = true;
            this.onlineRadioButton.CheckedChanged += new System.EventHandler(this.onlineRadioButton_CheckedChanged);
            // 
            // offlineRadioButton
            // 
            this.offlineRadioButton.AutoSize = true;
            this.offlineRadioButton.Checked = true;
            this.offlineRadioButton.CheckedColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.offlineRadioButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.offlineRadioButton.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(198)))), ((int)(((byte)(202)))));
            this.offlineRadioButton.DisabledStringColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(187)))), ((int)(((byte)(189)))));
            this.offlineRadioButton.Enable = true;
            this.offlineRadioButton.EnabledCheckedColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.offlineRadioButton.EnabledStringColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(146)))), ((int)(((byte)(146)))));
            this.offlineRadioButton.EnabledUncheckedColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(158)))), ((int)(((byte)(161)))));
            this.offlineRadioButton.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.offlineRadioButton.ForeColor = System.Drawing.Color.White;
            this.offlineRadioButton.Location = new System.Drawing.Point(83, 3);
            this.offlineRadioButton.Name = "offlineRadioButton";
            this.offlineRadioButton.Size = new System.Drawing.Size(82, 20);
            this.offlineRadioButton.TabIndex = 2;
            this.offlineRadioButton.TabStop = true;
            this.offlineRadioButton.Text = "Offline";
            this.offlineRadioButton.UseVisualStyleBackColor = true;
            // 
            // cyberRichTextBox1
            // 
            this.cyberRichTextBox1.Alpha = 20;
            this.cyberRichTextBox1.BackColor = System.Drawing.Color.Transparent;
            this.cyberRichTextBox1.Background_WidthPen = 3F;
            this.cyberRichTextBox1.BackgroundPen = true;
            this.cyberRichTextBox1.ColorBackground = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.cyberRichTextBox1.ColorBackground_Pen = System.Drawing.Color.White;
            this.cyberRichTextBox1.ColorLighting = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(200)))), ((int)(((byte)(238)))));
            this.cyberRichTextBox1.ColorPen_1 = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(200)))), ((int)(((byte)(238)))));
            this.cyberRichTextBox1.ColorPen_2 = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(52)))), ((int)(((byte)(68)))));
            this.cyberRichTextBox1.CyberRichTextBoxStyle = ReaLTaiizor.Enum.Cyber.StateStyle.Custom;
            this.cyberRichTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cyberRichTextBox1.Font = new System.Drawing.Font("Arial", 11F);
            this.cyberRichTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.cyberRichTextBox1.Lighting = false;
            this.cyberRichTextBox1.LinearGradientPen = false;
            this.cyberRichTextBox1.Location = new System.Drawing.Point(3, 55);
            this.cyberRichTextBox1.Name = "cyberRichTextBox1";
            this.cyberRichTextBox1.PenWidth = 15;
            this.cyberRichTextBox1.RGB = false;
            this.cyberRichTextBox1.Rounding = true;
            this.cyberRichTextBox1.RoundingInt = 30;
            this.cyberRichTextBox1.Size = new System.Drawing.Size(659, 46);
            this.cyberRichTextBox1.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            this.cyberRichTextBox1.TabIndex = 5;
            this.cyberRichTextBox1.Tag = "Cyber";
            this.cyberRichTextBox1.TextButton = "";
            this.cyberRichTextBox1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            this.cyberRichTextBox1.Timer_RGB = 300;
            // 
            // foreverButton1
            // 
            this.foreverButton1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.foreverButton1.BackColor = System.Drawing.Color.Transparent;
            this.foreverButton1.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.foreverButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.foreverButton1.Font = new System.Drawing.Font("Arial Rounded MT Bold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.foreverButton1.Location = new System.Drawing.Point(264, 142);
            this.foreverButton1.Name = "foreverButton1";
            this.foreverButton1.Rounded = true;
            this.foreverButton1.Size = new System.Drawing.Size(137, 39);
            this.foreverButton1.TabIndex = 6;
            this.foreverButton1.Text = "Test Connection";
            this.foreverButton1.TextColor = System.Drawing.Color.White;
            // 
            // MESDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(665, 184);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "MESDialog";
            this.RightToLeftLayout = true;
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private ReaLTaiizor.Controls.BigLabel bigLabel1;
        private ReaLTaiizor.Controls.MaterialDivider materialDivider1;
        private ReaLTaiizor.Controls.MoonLabel moonLabel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private ReaLTaiizor.Controls.HopeRadioButton onlineRadioButton;
        private ReaLTaiizor.Controls.CyberRichTextBox cyberRichTextBox1;
        private ReaLTaiizor.Controls.HopeRadioButton offlineRadioButton;
        private System.Windows.Forms.PictureBox pictureBox1;
        private ReaLTaiizor.Controls.ForeverButton foreverButton1;
    }
}