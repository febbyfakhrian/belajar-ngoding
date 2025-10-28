namespace WindowsFormsApp1
{
    partial class CameraDialog
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
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.setParameterButton = new ReaLTaiizor.Controls.ForeverButton();
            this.getParameterButton = new ReaLTaiizor.Controls.ForeverButton();
            this.materialDivider1 = new ReaLTaiizor.Controls.MaterialDivider();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.CloseButton = new System.Windows.Forms.PictureBox();
            this.bigLabel1 = new ReaLTaiizor.Controls.BigLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.frameRateTextBox = new ReaLTaiizor.Controls.MetroTextBox();
            this.frameRateLabel = new ReaLTaiizor.Controls.MoonLabel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.gainTextBox = new ReaLTaiizor.Controls.MetroTextBox();
            this.moonLabel1 = new ReaLTaiizor.Controls.MoonLabel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.exposureTimeLabel = new ReaLTaiizor.Controls.MoonLabel();
            this.exposureTimeTextBox = new ReaLTaiizor.Controls.MetroTextBox();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CloseButton)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.setParameterButton, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.getParameterButton, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 192);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(313, 37);
            this.tableLayoutPanel4.TabIndex = 8;
            // 
            // setParameterButton
            // 
            this.setParameterButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.setParameterButton.BackColor = System.Drawing.Color.Transparent;
            this.setParameterButton.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.setParameterButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.setParameterButton.Font = new System.Drawing.Font("Arial Rounded MT Bold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.setParameterButton.Location = new System.Drawing.Point(174, 3);
            this.setParameterButton.Name = "setParameterButton";
            this.setParameterButton.Rounded = true;
            this.setParameterButton.Size = new System.Drawing.Size(121, 31);
            this.setParameterButton.TabIndex = 8;
            this.setParameterButton.Text = "Set Parameter";
            this.setParameterButton.TextColor = System.Drawing.Color.White;
            this.setParameterButton.Click += new System.EventHandler(this.SetParameterButton_Click);
            // 
            // getParameterButton
            // 
            this.getParameterButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.getParameterButton.BackColor = System.Drawing.Color.Transparent;
            this.getParameterButton.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.getParameterButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.getParameterButton.Font = new System.Drawing.Font("Arial Rounded MT Bold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.getParameterButton.Location = new System.Drawing.Point(18, 3);
            this.getParameterButton.Name = "getParameterButton";
            this.getParameterButton.Rounded = true;
            this.getParameterButton.Size = new System.Drawing.Size(120, 31);
            this.getParameterButton.TabIndex = 7;
            this.getParameterButton.Text = "Get Parameter";
            this.getParameterButton.TextColor = System.Drawing.Color.White;
            this.getParameterButton.Click += new System.EventHandler(this.GetParameterButton_Click);
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.White;
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialDivider1.Location = new System.Drawing.Point(3, 49);
            this.materialDivider1.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(313, 2);
            this.materialDivider1.TabIndex = 1;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 93.13869F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.861314F));
            this.tableLayoutPanel2.Controls.Add(this.CloseButton, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.bigLabel1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(313, 40);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CloseButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CloseButton.Image = global::WindowsFormsApp1.Properties.Resources.x;
            this.CloseButton.Location = new System.Drawing.Point(296, 12);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(12, 15);
            this.CloseButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.CloseButton.TabIndex = 2;
            this.CloseButton.TabStop = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // bigLabel1
            // 
            this.bigLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bigLabel1.AutoSize = true;
            this.bigLabel1.BackColor = System.Drawing.Color.Transparent;
            this.bigLabel1.Font = new System.Drawing.Font("Arial Rounded MT Bold", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bigLabel1.ForeColor = System.Drawing.Color.White;
            this.bigLabel1.Location = new System.Drawing.Point(3, 9);
            this.bigLabel1.Name = "bigLabel1";
            this.bigLabel1.Size = new System.Drawing.Size(153, 22);
            this.bigLabel1.TabIndex = 0;
            this.bigLabel1.Text = "Setting Camera";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.materialDivider1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 84.21053F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.78947F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(319, 232);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Controls.Add(this.frameRateTextBox, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.frameRateLabel, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 147);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(313, 39);
            this.tableLayoutPanel6.TabIndex = 10;
            // 
            // frameRateTextBox
            // 
            this.frameRateTextBox.AutoCompleteCustomSource = null;
            this.frameRateTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.frameRateTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.frameRateTextBox.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.frameRateTextBox.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.frameRateTextBox.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.frameRateTextBox.DisabledForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.frameRateTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.frameRateTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.frameRateTextBox.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.frameRateTextBox.Image = null;
            this.frameRateTextBox.IsDerivedStyle = true;
            this.frameRateTextBox.Lines = null;
            this.frameRateTextBox.Location = new System.Drawing.Point(159, 3);
            this.frameRateTextBox.MaxLength = 32767;
            this.frameRateTextBox.Multiline = false;
            this.frameRateTextBox.Name = "frameRateTextBox";
            this.frameRateTextBox.ReadOnly = false;
            this.frameRateTextBox.Size = new System.Drawing.Size(151, 33);
            this.frameRateTextBox.Style = ReaLTaiizor.Enum.Metro.Style.Light;
            this.frameRateTextBox.StyleManager = null;
            this.frameRateTextBox.TabIndex = 6;
            this.frameRateTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.frameRateTextBox.ThemeAuthor = "Taiizor";
            this.frameRateTextBox.ThemeName = "MetroLight";
            this.frameRateTextBox.UseSystemPasswordChar = false;
            this.frameRateTextBox.WatermarkText = "";
            // 
            // frameRateLabel
            // 
            this.frameRateLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.frameRateLabel.AutoSize = true;
            this.frameRateLabel.BackColor = System.Drawing.Color.Transparent;
            this.frameRateLabel.ForeColor = System.Drawing.Color.White;
            this.frameRateLabel.Location = new System.Drawing.Point(3, 13);
            this.frameRateLabel.Name = "frameRateLabel";
            this.frameRateLabel.Size = new System.Drawing.Size(62, 13);
            this.frameRateLabel.TabIndex = 3;
            this.frameRateLabel.Text = "Frame Rate";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.gainTextBox, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.moonLabel1, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 102);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(313, 39);
            this.tableLayoutPanel5.TabIndex = 9;
            // 
            // gainTextBox
            // 
            this.gainTextBox.AutoCompleteCustomSource = null;
            this.gainTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.gainTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.gainTextBox.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.gainTextBox.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.gainTextBox.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.gainTextBox.DisabledForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.gainTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gainTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.gainTextBox.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.gainTextBox.Image = null;
            this.gainTextBox.IsDerivedStyle = true;
            this.gainTextBox.Lines = null;
            this.gainTextBox.Location = new System.Drawing.Point(159, 3);
            this.gainTextBox.MaxLength = 32767;
            this.gainTextBox.Multiline = false;
            this.gainTextBox.Name = "gainTextBox";
            this.gainTextBox.ReadOnly = false;
            this.gainTextBox.Size = new System.Drawing.Size(151, 33);
            this.gainTextBox.Style = ReaLTaiizor.Enum.Metro.Style.Light;
            this.gainTextBox.StyleManager = null;
            this.gainTextBox.TabIndex = 5;
            this.gainTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.gainTextBox.ThemeAuthor = "Taiizor";
            this.gainTextBox.ThemeName = "MetroLight";
            this.gainTextBox.UseSystemPasswordChar = false;
            this.gainTextBox.WatermarkText = "";
            // 
            // moonLabel1
            // 
            this.moonLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.moonLabel1.AutoSize = true;
            this.moonLabel1.BackColor = System.Drawing.Color.Transparent;
            this.moonLabel1.ForeColor = System.Drawing.Color.White;
            this.moonLabel1.Location = new System.Drawing.Point(3, 13);
            this.moonLabel1.Name = "moonLabel1";
            this.moonLabel1.Size = new System.Drawing.Size(29, 13);
            this.moonLabel1.TabIndex = 3;
            this.moonLabel1.Text = "Gain";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.exposureTimeLabel, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.exposureTimeTextBox, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 57);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(313, 39);
            this.tableLayoutPanel3.TabIndex = 7;
            // 
            // exposureTimeLabel
            // 
            this.exposureTimeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.exposureTimeLabel.AutoSize = true;
            this.exposureTimeLabel.BackColor = System.Drawing.Color.Transparent;
            this.exposureTimeLabel.ForeColor = System.Drawing.Color.White;
            this.exposureTimeLabel.Location = new System.Drawing.Point(3, 13);
            this.exposureTimeLabel.Name = "exposureTimeLabel";
            this.exposureTimeLabel.Size = new System.Drawing.Size(77, 13);
            this.exposureTimeLabel.TabIndex = 3;
            this.exposureTimeLabel.Text = "Exposure Time";
            // 
            // exposureTimeTextBox
            // 
            this.exposureTimeTextBox.AutoCompleteCustomSource = null;
            this.exposureTimeTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.exposureTimeTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.exposureTimeTextBox.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.exposureTimeTextBox.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.exposureTimeTextBox.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.exposureTimeTextBox.DisabledForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.exposureTimeTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exposureTimeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.exposureTimeTextBox.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.exposureTimeTextBox.Image = null;
            this.exposureTimeTextBox.IsDerivedStyle = true;
            this.exposureTimeTextBox.Lines = null;
            this.exposureTimeTextBox.Location = new System.Drawing.Point(159, 3);
            this.exposureTimeTextBox.MaxLength = 32767;
            this.exposureTimeTextBox.Multiline = false;
            this.exposureTimeTextBox.Name = "exposureTimeTextBox";
            this.exposureTimeTextBox.ReadOnly = false;
            this.exposureTimeTextBox.Size = new System.Drawing.Size(151, 33);
            this.exposureTimeTextBox.Style = ReaLTaiizor.Enum.Metro.Style.Light;
            this.exposureTimeTextBox.StyleManager = null;
            this.exposureTimeTextBox.TabIndex = 4;
            this.exposureTimeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.exposureTimeTextBox.ThemeAuthor = "Taiizor";
            this.exposureTimeTextBox.ThemeName = "MetroLight";
            this.exposureTimeTextBox.UseSystemPasswordChar = false;
            this.exposureTimeTextBox.WatermarkText = "";
            // 
            // CameraDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(319, 232);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CameraDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CloseButton)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private ReaLTaiizor.Controls.ForeverButton setParameterButton;
        private ReaLTaiizor.Controls.ForeverButton getParameterButton;
        private ReaLTaiizor.Controls.MaterialDivider materialDivider1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.PictureBox CloseButton;
        private ReaLTaiizor.Controls.BigLabel bigLabel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private ReaLTaiizor.Controls.MoonLabel frameRateLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private ReaLTaiizor.Controls.MoonLabel moonLabel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private ReaLTaiizor.Controls.MoonLabel exposureTimeLabel;
        private ReaLTaiizor.Controls.MetroTextBox exposureTimeTextBox;
        private ReaLTaiizor.Controls.MetroTextBox frameRateTextBox;
        private ReaLTaiizor.Controls.MetroTextBox gainTextBox;
    }
}