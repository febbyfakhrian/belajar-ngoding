namespace WindowsFormsApp1
{
    partial class LoginForm
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
            this.foreverButton1 = new ReaLTaiizor.Controls.ForeverButton();
            this.moonLabel1 = new ReaLTaiizor.Controls.MoonLabel();
            this.materialDivider1 = new ReaLTaiizor.Controls.MaterialDivider();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.bigLabel1 = new ReaLTaiizor.Controls.BigLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.metroTextBox2 = new ReaLTaiizor.Controls.MetroTextBox();
            this.moonLabel2 = new ReaLTaiizor.Controls.MoonLabel();
            this.metroTextBox1 = new ReaLTaiizor.Controls.MetroTextBox();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // foreverButton1
            // 
            this.foreverButton1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.foreverButton1.BackColor = System.Drawing.Color.Transparent;
            this.foreverButton1.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(32)))), ((int)(((byte)(46)))));
            this.foreverButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.foreverButton1.Font = new System.Drawing.Font("Arial Rounded MT Bold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.foreverButton1.Location = new System.Drawing.Point(150, 166);
            this.foreverButton1.Name = "foreverButton1";
            this.foreverButton1.Rounded = true;
            this.foreverButton1.Size = new System.Drawing.Size(137, 36);
            this.foreverButton1.TabIndex = 6;
            this.foreverButton1.Text = "LOGIN";
            this.foreverButton1.TextColor = System.Drawing.Color.White;
            this.foreverButton1.Click += new System.EventHandler(this.foreverButton1_Click);
            // 
            // moonLabel1
            // 
            this.moonLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.moonLabel1.AutoSize = true;
            this.moonLabel1.BackColor = System.Drawing.Color.Transparent;
            this.moonLabel1.ForeColor = System.Drawing.Color.White;
            this.moonLabel1.Location = new System.Drawing.Point(3, 48);
            this.moonLabel1.Name = "moonLabel1";
            this.moonLabel1.Size = new System.Drawing.Size(58, 13);
            this.moonLabel1.TabIndex = 2;
            this.moonLabel1.Text = "Badge No.";
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.White;
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialDivider1.Location = new System.Drawing.Point(3, 41);
            this.materialDivider1.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(431, 2);
            this.materialDivider1.TabIndex = 1;
            this.materialDivider1.Text = "materialDivider1";
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(431, 32);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::WindowsFormsApp1.Properties.Resources.x;
            this.pictureBox1.Location = new System.Drawing.Point(404, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(24, 14);
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
            this.bigLabel1.Location = new System.Drawing.Point(3, 5);
            this.bigLabel1.Name = "bigLabel1";
            this.bigLabel1.Size = new System.Drawing.Size(60, 22);
            this.bigLabel1.TabIndex = 0;
            this.bigLabel1.Text = "Login";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.metroTextBox2, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.moonLabel2, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.materialDivider1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.moonLabel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.foreverButton1, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.metroTextBox1, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.MaximumSize = new System.Drawing.Size(437, 205);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(437, 205);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 82.6087F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 17.3913F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(437, 205);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // metroTextBox2
            // 
            this.metroTextBox2.AutoCompleteCustomSource = null;
            this.metroTextBox2.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.metroTextBox2.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.metroTextBox2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.metroTextBox2.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.metroTextBox2.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.metroTextBox2.DisabledForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.metroTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTextBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.metroTextBox2.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.metroTextBox2.Image = null;
            this.metroTextBox2.IsDerivedStyle = true;
            this.metroTextBox2.Lines = null;
            this.metroTextBox2.Location = new System.Drawing.Point(3, 124);
            this.metroTextBox2.MaxLength = 100;
            this.metroTextBox2.Multiline = false;
            this.metroTextBox2.Name = "metroTextBox2";
            this.metroTextBox2.ReadOnly = false;
            this.metroTextBox2.Size = new System.Drawing.Size(431, 36);
            this.metroTextBox2.Style = ReaLTaiizor.Enum.Metro.Style.Light;
            this.metroTextBox2.StyleManager = null;
            this.metroTextBox2.TabIndex = 11;
            this.metroTextBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.metroTextBox2.ThemeAuthor = "Taiizor";
            this.metroTextBox2.ThemeName = "MetroLight";
            this.metroTextBox2.UseSystemPasswordChar = true;
            this.metroTextBox2.WatermarkText = "";
            this.metroTextBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.passwordTextBox_KeyDown);
            // 
            // moonLabel2
            // 
            this.moonLabel2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.moonLabel2.AutoSize = true;
            this.moonLabel2.BackColor = System.Drawing.Color.Transparent;
            this.moonLabel2.ForeColor = System.Drawing.Color.White;
            this.moonLabel2.Location = new System.Drawing.Point(3, 105);
            this.moonLabel2.Name = "moonLabel2";
            this.moonLabel2.Size = new System.Drawing.Size(53, 13);
            this.moonLabel2.TabIndex = 7;
            this.moonLabel2.Text = "Password";
            // 
            // metroTextBox1
            // 
            this.metroTextBox1.AutoCompleteCustomSource = null;
            this.metroTextBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.metroTextBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.metroTextBox1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.metroTextBox1.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.metroTextBox1.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.metroTextBox1.DisabledForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.metroTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.metroTextBox1.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.metroTextBox1.Image = null;
            this.metroTextBox1.IsDerivedStyle = true;
            this.metroTextBox1.Lines = null;
            this.metroTextBox1.Location = new System.Drawing.Point(3, 67);
            this.metroTextBox1.MaxLength = 100;
            this.metroTextBox1.Multiline = false;
            this.metroTextBox1.Name = "metroTextBox1";
            this.metroTextBox1.ReadOnly = false;
            this.metroTextBox1.Size = new System.Drawing.Size(431, 32);
            this.metroTextBox1.Style = ReaLTaiizor.Enum.Metro.Style.Light;
            this.metroTextBox1.StyleManager = null;
            this.metroTextBox1.TabIndex = 10;
            this.metroTextBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.metroTextBox1.ThemeAuthor = "Taiizor";
            this.metroTextBox1.ThemeName = "MetroLight";
            this.metroTextBox1.UseSystemPasswordChar = false;
            this.metroTextBox1.WatermarkText = "";
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(437, 205);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ReaLTaiizor.Controls.ForeverButton foreverButton1;
        private ReaLTaiizor.Controls.MoonLabel moonLabel1;
        private ReaLTaiizor.Controls.MaterialDivider materialDivider1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private ReaLTaiizor.Controls.BigLabel bigLabel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private ReaLTaiizor.Controls.MoonLabel moonLabel2;
        private ReaLTaiizor.Controls.MetroTextBox metroTextBox1;
        private ReaLTaiizor.Controls.MetroTextBox metroTextBox2;
    }
}