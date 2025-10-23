namespace WindowsFormsApp1
{
    partial class DialogDebugMenuPlc
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
            this.testPlcReadBtn = new ReaLTaiizor.Controls.Button();
            this.testPlcWriteBtn = new ReaLTaiizor.Controls.Button();
            this.testPlcWriteReadBtn = new ReaLTaiizor.Controls.Button();
            this.button1 = new ReaLTaiizor.Controls.Button();
            this.comboBoxDevices = new System.Windows.Forms.ComboBox();
            this.connectBtn = new ReaLTaiizor.Controls.Button();
            this.flowCam0 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.flowCam1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.flowMain = new System.Windows.Forms.FlowLayoutPanel();
            this.flowResponse = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.inputCommandPlc = new ReaLTaiizor.Controls.BigTextBox();
            this.plcLogBox = new System.Windows.Forms.RichTextBox();
            this.sendCommandBtn = new ReaLTaiizor.Controls.Button();
            this.baudRateTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // testPlcReadBtn
            // 
            this.testPlcReadBtn.BackColor = System.Drawing.Color.Transparent;
            this.testPlcReadBtn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.testPlcReadBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.testPlcReadBtn.EnteredBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.testPlcReadBtn.EnteredColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.testPlcReadBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.testPlcReadBtn.Image = null;
            this.testPlcReadBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.testPlcReadBtn.InactiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.testPlcReadBtn.Location = new System.Drawing.Point(12, 55);
            this.testPlcReadBtn.Name = "testPlcReadBtn";
            this.testPlcReadBtn.PressedBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.testPlcReadBtn.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.testPlcReadBtn.Size = new System.Drawing.Size(120, 40);
            this.testPlcReadBtn.TabIndex = 7;
            this.testPlcReadBtn.TabStop = false;
            this.testPlcReadBtn.Text = "Test PLC Read ";
            this.testPlcReadBtn.TextAlignment = System.Drawing.StringAlignment.Center;
            this.testPlcReadBtn.Click += new System.EventHandler(this.testPlcReadBtn_Click);
            // 
            // testPlcWriteBtn
            // 
            this.testPlcWriteBtn.BackColor = System.Drawing.Color.Transparent;
            this.testPlcWriteBtn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.testPlcWriteBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.testPlcWriteBtn.EnteredBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.testPlcWriteBtn.EnteredColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.testPlcWriteBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.testPlcWriteBtn.Image = null;
            this.testPlcWriteBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.testPlcWriteBtn.InactiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.testPlcWriteBtn.Location = new System.Drawing.Point(138, 55);
            this.testPlcWriteBtn.Name = "testPlcWriteBtn";
            this.testPlcWriteBtn.PressedBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.testPlcWriteBtn.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.testPlcWriteBtn.Size = new System.Drawing.Size(120, 40);
            this.testPlcWriteBtn.TabIndex = 8;
            this.testPlcWriteBtn.TabStop = false;
            this.testPlcWriteBtn.Text = "Test PLC Write";
            this.testPlcWriteBtn.TextAlignment = System.Drawing.StringAlignment.Center;
            this.testPlcWriteBtn.Click += new System.EventHandler(this.testPlcWriteBtn_Click);
            // 
            // testPlcWriteReadBtn
            // 
            this.testPlcWriteReadBtn.BackColor = System.Drawing.Color.Transparent;
            this.testPlcWriteReadBtn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.testPlcWriteReadBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.testPlcWriteReadBtn.EnteredBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.testPlcWriteReadBtn.EnteredColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.testPlcWriteReadBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.testPlcWriteReadBtn.Image = null;
            this.testPlcWriteReadBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.testPlcWriteReadBtn.InactiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(37)))));
            this.testPlcWriteReadBtn.Location = new System.Drawing.Point(12, 101);
            this.testPlcWriteReadBtn.Name = "testPlcWriteReadBtn";
            this.testPlcWriteReadBtn.PressedBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.testPlcWriteReadBtn.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.testPlcWriteReadBtn.Size = new System.Drawing.Size(183, 40);
            this.testPlcWriteReadBtn.TabIndex = 9;
            this.testPlcWriteReadBtn.TabStop = false;
            this.testPlcWriteReadBtn.Text = "Test PLC Write & Read";
            this.testPlcWriteReadBtn.TextAlignment = System.Drawing.StringAlignment.Center;
            this.testPlcWriteReadBtn.Click += new System.EventHandler(this.testPlcWriteReadBtn_Click);
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
            this.button1.Location = new System.Drawing.Point(277, 49);
            this.button1.Name = "button1";
            this.button1.PressedBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.button1.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.button1.Size = new System.Drawing.Size(120, 40);
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
            this.connectBtn.Location = new System.Drawing.Point(262, 25);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.PressedBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.connectBtn.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.connectBtn.Size = new System.Drawing.Size(93, 21);
            this.connectBtn.TabIndex = 12;
            this.connectBtn.Text = "Connect";
            this.connectBtn.TextAlignment = System.Drawing.StringAlignment.Center;
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            // 
            // flowCam0
            // 
            this.flowCam0.AutoScroll = true;
            this.flowCam0.Location = new System.Drawing.Point(12, 415);
            this.flowCam0.Name = "flowCam0";
            this.flowCam0.Size = new System.Drawing.Size(491, 199);
            this.flowCam0.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 399);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Camera 0";
            // 
            // flowCam1
            // 
            this.flowCam1.AutoScroll = true;
            this.flowCam1.Location = new System.Drawing.Point(529, 415);
            this.flowCam1.Name = "flowCam1";
            this.flowCam1.Size = new System.Drawing.Size(479, 199);
            this.flowCam1.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(532, 398);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Camera 1";
            // 
            // flowMain
            // 
            this.flowMain.AutoScroll = true;
            this.flowMain.Location = new System.Drawing.Point(529, 191);
            this.flowMain.Name = "flowMain";
            this.flowMain.Size = new System.Drawing.Size(479, 182);
            this.flowMain.TabIndex = 16;
            // 
            // flowResponse
            // 
            this.flowResponse.AutoScroll = true;
            this.flowResponse.Location = new System.Drawing.Point(469, 25);
            this.flowResponse.Name = "flowResponse";
            this.flowResponse.Size = new System.Drawing.Size(230, 120);
            this.flowResponse.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(526, 174);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "PLC V1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(466, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Response";
            // 
            // inputCommandPlc
            // 
            this.inputCommandPlc.BackColor = System.Drawing.Color.Transparent;
            this.inputCommandPlc.Font = new System.Drawing.Font("Tahoma", 11F);
            this.inputCommandPlc.ForeColor = System.Drawing.Color.DimGray;
            this.inputCommandPlc.Image = null;
            this.inputCommandPlc.Location = new System.Drawing.Point(12, 174);
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
            this.plcLogBox.Location = new System.Drawing.Point(12, 221);
            this.plcLogBox.Name = "plcLogBox";
            this.plcLogBox.Size = new System.Drawing.Size(489, 175);
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
            this.sendCommandBtn.Location = new System.Drawing.Point(238, 175);
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
            // DialogDebugMenuPlc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1022, 626);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.baudRateTextBox);
            this.Controls.Add(this.sendCommandBtn);
            this.Controls.Add(this.plcLogBox);
            this.Controls.Add(this.inputCommandPlc);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.flowResponse);
            this.Controls.Add(this.flowMain);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.flowCam1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.flowCam0);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.comboBoxDevices);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.testPlcWriteReadBtn);
            this.Controls.Add(this.testPlcWriteBtn);
            this.Controls.Add(this.testPlcReadBtn);
            this.Name = "DialogDebugMenuPlc";
            this.Text = "Form3";
            this.Load += new System.EventHandler(this.DialogDebugMenuPlc_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ReaLTaiizor.Controls.Button testPlcReadBtn;
        private ReaLTaiizor.Controls.Button testPlcWriteBtn;
        private ReaLTaiizor.Controls.Button testPlcWriteReadBtn;
        private ReaLTaiizor.Controls.Button button1;
        private System.Windows.Forms.ComboBox comboBoxDevices;
        private ReaLTaiizor.Controls.Button connectBtn;
        private System.Windows.Forms.FlowLayoutPanel flowCam0;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowCam1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flowMain;
        private System.Windows.Forms.FlowLayoutPanel flowResponse;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private ReaLTaiizor.Controls.BigTextBox inputCommandPlc;
        private System.Windows.Forms.RichTextBox plcLogBox;
        private ReaLTaiizor.Controls.Button sendCommandBtn;
        private System.Windows.Forms.TextBox baudRateTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}