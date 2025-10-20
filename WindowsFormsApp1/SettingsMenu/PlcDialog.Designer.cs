namespace WindowsFormsApp1
{
    partial class PlcDialog
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
            this.button1 = new ReaLTaiizor.Controls.Button();
            this.comboBoxDevices = new System.Windows.Forms.ComboBox();
            this.connectBtn = new ReaLTaiizor.Controls.Button();
            this.flowMain = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.inputCommandPlc = new ReaLTaiizor.Controls.BigTextBox();
            this.plcLogBox = new System.Windows.Forms.RichTextBox();
            this.sendCommandBtn = new ReaLTaiizor.Controls.Button();
            this.baudRateTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Transparent;
            this.button1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.EnteredBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.button1.EnteredColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.button1.Image = null;
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.InactiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.button1.Location = new System.Drawing.Point(344, 18);
            this.button1.Name = "button1";
            this.button1.PressedBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.button1.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.button1.Size = new System.Drawing.Size(75, 28);
            this.button1.TabIndex = 10;
            this.button1.TabStop = false;
            this.button1.Text = "Stop";
            this.button1.TextAlignment = System.Drawing.StringAlignment.Center;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBoxDevices
            // 
            this.comboBoxDevices.FormattingEnabled = true;
            this.comboBoxDevices.Location = new System.Drawing.Point(11, 24);
            this.comboBoxDevices.Name = "comboBoxDevices";
            this.comboBoxDevices.Size = new System.Drawing.Size(121, 21);
            this.comboBoxDevices.TabIndex = 11;
            this.comboBoxDevices.SelectedIndexChanged += new System.EventHandler(this.comboBoxDevices_SelectedIndexChanged);
            // 
            // connectBtn
            // 
            this.connectBtn.BackColor = System.Drawing.Color.Transparent;
            this.connectBtn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.connectBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.connectBtn.EnteredBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.connectBtn.EnteredColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.connectBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.connectBtn.Image = null;
            this.connectBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.connectBtn.InactiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.connectBtn.Location = new System.Drawing.Point(262, 18);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.PressedBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.connectBtn.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.connectBtn.Size = new System.Drawing.Size(76, 28);
            this.connectBtn.TabIndex = 12;
            this.connectBtn.Text = "Connect";
            this.connectBtn.TextAlignment = System.Drawing.StringAlignment.Center;
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            // 
            // flowMain
            // 
            this.flowMain.AutoScroll = true;
            this.flowMain.Location = new System.Drawing.Point(438, 22);
            this.flowMain.Name = "flowMain";
            this.flowMain.Size = new System.Drawing.Size(324, 103);
            this.flowMain.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(435, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "PLC V1";
            // 
            // inputCommandPlc
            // 
            this.inputCommandPlc.BackColor = System.Drawing.Color.Transparent;
            this.inputCommandPlc.Font = new System.Drawing.Font("Tahoma", 11F);
            this.inputCommandPlc.ForeColor = System.Drawing.Color.DimGray;
            this.inputCommandPlc.Image = null;
            this.inputCommandPlc.Location = new System.Drawing.Point(12, 84);
            this.inputCommandPlc.MaxLength = 32767;
            this.inputCommandPlc.Multiline = false;
            this.inputCommandPlc.Name = "inputCommandPlc";
            this.inputCommandPlc.ReadOnly = false;
            this.inputCommandPlc.Size = new System.Drawing.Size(218, 41);
            this.inputCommandPlc.TabIndex = 20;
            this.inputCommandPlc.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.inputCommandPlc.UseSystemPasswordChar = false;
            // 
            // plcLogBox
            // 
            this.plcLogBox.Location = new System.Drawing.Point(11, 144);
            this.plcLogBox.Name = "plcLogBox";
            this.plcLogBox.Size = new System.Drawing.Size(751, 175);
            this.plcLogBox.TabIndex = 22;
            this.plcLogBox.Text = "";
            // 
            // sendCommandBtn
            // 
            this.sendCommandBtn.BackColor = System.Drawing.Color.Transparent;
            this.sendCommandBtn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.sendCommandBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.sendCommandBtn.EnteredBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.sendCommandBtn.EnteredColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.sendCommandBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.sendCommandBtn.Image = null;
            this.sendCommandBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.sendCommandBtn.InactiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.sendCommandBtn.Location = new System.Drawing.Point(238, 85);
            this.sendCommandBtn.Name = "sendCommandBtn";
            this.sendCommandBtn.PressedBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.sendCommandBtn.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.sendCommandBtn.Size = new System.Drawing.Size(79, 40);
            this.sendCommandBtn.TabIndex = 23;
            this.sendCommandBtn.TabStop = false;
            this.sendCommandBtn.Text = "Send";
            this.sendCommandBtn.TextAlignment = System.Drawing.StringAlignment.Center;
            this.sendCommandBtn.Click += new System.EventHandler(this.sendCommandBtn_Click);
            // 
            // baudRateTextBox
            // 
            this.baudRateTextBox.Location = new System.Drawing.Point(139, 25);
            this.baudRateTextBox.Name = "baudRateTextBox";
            this.baudRateTextBox.Size = new System.Drawing.Size(117, 20);
            this.baudRateTextBox.TabIndex = 24;
            this.baudRateTextBox.Text = "9600";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 13);
            this.label5.TabIndex = 25;
            this.label5.Text = "Com";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(135, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "Baud Rate";
            // 
            // PlcDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 330);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.baudRateTextBox);
            this.Controls.Add(this.sendCommandBtn);
            this.Controls.Add(this.plcLogBox);
            this.Controls.Add(this.inputCommandPlc);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.flowMain);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.comboBoxDevices);
            this.Name = "PlcDialog";
            this.Text = "Form3";
            this.Load += new System.EventHandler(this.DialogDebugMenuPlc_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ReaLTaiizor.Controls.Button button1;
        private System.Windows.Forms.ComboBox comboBoxDevices;
        private ReaLTaiizor.Controls.Button connectBtn;
        private System.Windows.Forms.FlowLayoutPanel flowMain;
        private System.Windows.Forms.Label label3;
        private ReaLTaiizor.Controls.BigTextBox inputCommandPlc;
        private System.Windows.Forms.RichTextBox plcLogBox;
        private ReaLTaiizor.Controls.Button sendCommandBtn;
        private System.Windows.Forms.TextBox baudRateTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}