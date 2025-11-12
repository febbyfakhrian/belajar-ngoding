using AutoInspectionPlatform;
using PLCCommunication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Core.Entities.Models;
using WindowsFormsApp1.Infrastructure.Hardware.PLC;
using WindowsFormsApp1.Infrastructure.Services;
using WindowsFormsApp1.Infrastructure.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1
{
    public partial class PlcDialog : Form
    {
        // Add debounce fields
        private DateTime _lastPlcCommandTime = DateTime.MinValue;
        private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(200); // 200ms debounce
        
        private SerialPort serial;
        private bool waitingResponse = false;
        private readonly StringBuilder rxBuffer = new StringBuilder();
        private int _lineStart = 0;
        private readonly InspectionLogger logger = new InspectionLogger();
        private readonly Stopwatch cycleStopwatch = new Stopwatch();
        private bool testRunning = false;
        private readonly List<double> cycleTimes = new List<double>();
        private int loopCount = 0;
        private int totalLoops = 20;

        private readonly IServiceProvider _serviceProvider;
        private IPlcService _plcService;
        private readonly ISettingsService _settingsService; // Made readonly

        // Constructor with dependency injection
        public PlcDialog(IServiceProvider serviceProvider, ISettingsService settingsService = null)
        {
            _serviceProvider = serviceProvider;
            _settingsService = settingsService ?? GetSettingsServiceFromProvider();
            InitializeComponent();
            // Setup Serial Port
        }

        private ISettingsService GetSettingsServiceFromProvider()
        {
            // If we have a service provider, use it to get the service
            if (_serviceProvider != null)
            {
                try
                {
                    return _serviceProvider.GetRequiredService<ISettingsService>();
                }
                catch (Exception ex)
                {
                    // Log the error for debugging purposes
                    Debug.WriteLine($"Error getting settings service from provider: {ex.Message}");
                    Console.WriteLine($"Error getting settings service from provider: {ex.Message}");
                }
            }

            // Fallback to null for backward compatibility
            return null;
        }

        // Legacy constructor for backward compatibility
        public PlcDialog() : this(null)
        {
        }

        private void DialogDebugMenuPlc_Load(object sender, EventArgs e)
        {
            LoadComPorts();   // isi combobox

            GenerateButtonsFromClass(flowMain, typeof(WritePLCAddress));
        }
           
        private void LoadComPorts()
        {
            // Ambil semua COM port yang tersedia di sistem
            string[] ports = SerialPort.GetPortNames();

            // Bersihkan isi combo box dulu
            comboBoxDevices.Items.Clear();

            // Masukkan semua port ke combobox
            foreach (string port in ports)
            {
                comboBoxDevices.Items.Add(port);
            }

            // Pilih otomatis port pertama (kalau ada)
            if (comboBoxDevices.Items.Count > 0)
                comboBoxDevices.SelectedIndex = 0;
            else
                comboBoxDevices.Items.Add("No COM ports found");
        }

        private void comboBoxDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedPort = comboBoxDevices.SelectedItem.ToString();

            // Tutup port lama dulu kalau sedang terbuka
            if (serial != null && serial.IsOpen)
            {
                serial.Close();
            }

            // Buat ulang instance SerialPort sesuai pilihan user
            serial = new SerialPort(selectedPort, 9600, Parity.None, 8, StopBits.One);

            Console.WriteLine($"[INFO] Serial port diset ke: {selectedPort}");
        }

        private void ProcessCompleteLine(string line)
        {
            waitingResponse = false;
            cycleStopwatch.Stop();
            double cycleMs = cycleStopwatch.Elapsed.TotalMilliseconds;

            BeginInvoke(new Action(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string log = $"[{timestamp}] [RECV] {line}\r\n";
                if (testRunning)
                    log += $"[{timestamp}] [CYCLE] {cycleMs:F2} ms\r\n";

                plcLogBox.AppendText(log);
                plcLogBox.SelectionStart = plcLogBox.Text.Length;
                plcLogBox.ScrollToCaret();
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Debounce mechanism to prevent multiple rapid clicks
            var now = DateTime.UtcNow;
            if (now - _lastPlcCommandTime < _debounceInterval)
            {
                Console.WriteLine($"[PLC] Command ignored due to debounce at {now:HH:mm:ss.fff}");
                return;
            }
            _lastPlcCommandTime = now;

            try
            {
                // Mulai stopwatch
                CycleTimer.Start();
                DateTime startTime = DateTime.Now;

                // Kirim perintah PLC (misal)
                byte[] command = WritePLCAddress.READ;
                serial.Write(command, 0, command.Length);
                Console.WriteLine($"[{startTime:HH:mm:ss.fff}] Kirim READ ke PLC...");

                // Tunggu respons PLC (misal blocking read atau event)
                string response = serial.ReadLine(); // atau dapat dari Serial_DataReceived
                DateTime endTime = DateTime.Now;

                // Stop timer
                double cycleTimeMs = CycleTimer.Stop();
                Console.WriteLine($"[{endTime:HH:mm:ss.fff}] Respons PLC: {response}");
                Console.WriteLine($"Cycle time final: {cycleTimeMs:F2} ms");

                // Tambahkan log ke InspectionLogger
                var result = new InspectionResult
                {
                    TransactionId = Guid.NewGuid().ToString(), // atau bisa increment
                    Timestamp = startTime,
                    CycleTimeMs = cycleTimeMs,
                    RawResponse = response,
                    Pass = response.Contains("PASS") // contoh, bisa disesuaikan
                };

                logger.AddLog(result);

                logger.SaveToJson();

                Console.WriteLine("✅ Log inspection ditambahkan.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error saat klik button: " + ex.Message);
            }
        }

        // ==========================================================
        // GENERATE UNTUK CLASS TANPA CAMERA (WritePLCAddress, PLCResponseMessage)
        // ==========================================================
        private void GenerateButtonsFromClass(FlowLayoutPanel flow, Type plcClass)
        {
            if (flow == null) return;

            var fields = plcClass.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                Button btn = new Button
                {
                    Text = field.Name,
                    Tag = $"{plcClass.Name}.{field.Name}",
                    Width = 120,
                    Height = 35,
                    Margin = new Padding(6)
                };

                // Tambahkan warna untuk visual clarity
                if (field.Name.Contains("PASS")) btn.BackColor = System.Drawing.Color.LightGreen;
                else if (field.Name.Contains("FAIL")) btn.BackColor = System.Drawing.Color.LightCoral;
                else if (field.Name.Contains("READ")) btn.BackColor = System.Drawing.Color.LightBlue;
                else if (field.Name.Contains("NEXT")) btn.BackColor = System.Drawing.Color.Khaki;

                btn.Click += PlcCommandButton_Click;
                flow.Controls.Add(btn);
            }
        }

        // ==========================================================
        // BUTTON CLICK UNTUK KIRIM COMMAND PLC
        // ==========================================================
        private void PlcCommandButton_Click(object sender, EventArgs e)
        {
            // Debounce mechanism to prevent multiple rapid clicks
            var now = DateTime.UtcNow;
            if (now - _lastPlcCommandTime < _debounceInterval)
            {
                Console.WriteLine($"[PLC] Command ignored due to debounce at {now:HH:mm:ss.fff}");
                return;
            }
            _lastPlcCommandTime = now;

            try
            {
                Button button = sender as Button;
                if (button == null) return;

                string tag = button.Tag?.ToString();
                if (string.IsNullOrEmpty(tag)) return;

                string[] parts = tag.Split('.');
                if (parts.Length != 2) return;

                string className = parts[0];
                string fieldName = parts[1];

                Type targetClass = Type.GetType($"PLCCommunication.{className}");
                if (targetClass == null)
                {
                    MessageBox.Show($"Class {className} tidak ditemukan.");
                    return;
                }

                FieldInfo field = targetClass.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
                if (field == null)
                {
                    MessageBox.Show($"Field {fieldName} tidak ditemukan di {className}.");
                    return;
                }

                byte[] command = (byte[])field.GetValue(null);

                if (!serial.IsOpen)
                {
                    MessageBox.Show("Serial port belum dibuka!");
                    return;
                }

                serial.Write(command, 0, command.Length);
                Console.WriteLine($"[PLC] Sent {className}.{fieldName} → {Encoding.ASCII.GetString(command)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal kirim command PLC: {ex.Message}");
            }
        }

        // ==========================================================
        // BUTTON CONNECT / DISCONNECT
        // ==========================================================
        private void connectBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(baudRateTextBox.Text))
                {
                    MessageBox.Show("Please input baud rate");
                    return;
                }
                if (!serial.IsOpen)
                {
                    serial.PortName = comboBoxDevices.SelectedItem.ToString();
                    serial.BaudRate = Int32.Parse(baudRateTextBox.Text);
                    serial.Parity = Parity.None;
                    serial.DataBits = 8;
                    serial.StopBits = StopBits.One;
                    serial.ReadBufferSize = 1024;
                    serial.WriteBufferSize = 1024;
                    serial.NewLine = "\r";          // atau "\r\n" sesuai PLC
                    serial.Encoding = Encoding.ASCII;

                    serial.Open();

                    serial.DataReceived += Serial_DataReceived;

                    connectBtn.Text = "Disconnect";
                    comboBoxDevices.Enabled = false;
                    MessageBox.Show($"Connected to {serial.PortName}");
                }
                else
                {
                    serial.Close();
                    connectBtn.Text = "Connect";
                    comboBoxDevices.Enabled = true;
                    MessageBox.Show("Disconnected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Serial error: {ex.Message}");
            }
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // 1. Baca semua data yang datang
                string chunk = serial.ReadExisting();
                if (string.IsNullOrEmpty(chunk)) return;

                lock (rxBuffer) rxBuffer.Append(chunk);

                // 2. Cek apakah sudah ada baris utuh (CR/LF)
                string buffered = rxBuffer.ToString();
                int current = 0;
                int len = buffered.Length;
                
                // Track processed lines to prevent duplicate processing
                var processedLines = new List<string>();

                while (current < len)
                {
                    char ch = buffered[current];
                    if (ch == '\r' || ch == '\n')
                    {
                        int lineLen = current - _lineStart;
                        if (lineLen > 0)
                        {
                            string line = buffered.Substring(_lineStart, lineLen).Trim();
                            // Only process each line once
                            if (!processedLines.Contains(line))
                            {
                                processedLines.Add(line);
                                ProcessCompleteLine(line);
                            }
                        }
                        current++;               // skip '\r' atau '\n'
                        _lineStart = current;    // mark posisi baru
                    }
                    else
                    {
                        current++;
                    }
                }

                int finalLen = current - _lineStart;
                if (finalLen > 0) 
                {
                    string finalLine = buffered.Substring(_lineStart, finalLen).Trim();
                    // Only process each line once
                    if (!processedLines.Contains(finalLine))
                    {
                        processedLines.Add(finalLine);
                        ProcessCompleteLine(finalLine);
                    }
                }

                // 3. Sisa yang belum selesai
                lock (rxBuffer)
                {
                    // Only keep unprocessed data
                    if (_lineStart < len)
                    {
                        string remaining = buffered.Substring(_lineStart);
                        rxBuffer.Clear();
                        rxBuffer.Append(remaining);
                    }
                    else
                    {
                        rxBuffer.Clear();
                    }
                }

                _lineStart = 0;   // reset untuk next event
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() =>
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    plcLogBox.AppendText($"[{timestamp}] [ERROR] {ex.Message}\r\n");
                    plcLogBox.SelectionStart = plcLogBox.Text.Length;
                    plcLogBox.ScrollToCaret();
                }));
            }
        }

        private void sendCommandBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (serial.IsOpen)
                {
                    string cmd = inputCommandPlc.Text.Trim();

                    if (string.IsNullOrEmpty(cmd))
                    {
                        MessageBox.Show("Input command cannot be empty.");
                        return;
                    }

                    // Banyak PLC butuh terminator seperti CR atau LF.
                    // Sesuaikan dengan spesifikasi PLC kamu.
                    string commandToSend = cmd + "\r";
                    serial.Write(commandToSend);

                    waitingResponse = true;
                    Console.WriteLine($"[SEND] {cmd}");
                }
                else
                {
                    MessageBox.Show("Serial port not connected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Send error: {ex.Message}");
            }
        }
        
        // Helper method to get the PLC service from DI
        private IPlcService GetPlcService()
        {
            // If we have a service provider, use it to get the service
            if (_serviceProvider != null)
            {
                return _serviceProvider.GetRequiredService<IPlcService>();
            }
            
            // Fallback to static instance for backward compatibility
            return null;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            // Access the Checked property value of the disablePLCradioButton control
            // and implement conditional logic based on this value
            if (disablePlcRadioButton.Checked)
            {
                // PLC functionality should be disabled
                HandlePlcDisabledState();
            }
            else
            {
                // PLC functionality should be enabled
                HandlePlcEnabledState();
            }
        }

        /// <summary>
        /// Handles the logic when PLC functionality is disabled
        /// </summary>
        private void HandlePlcDisabledState()
        {
            try
            {
                // Show message to user
                MessageBox.Show("PLC functionality has been disabled.", "PLC Status", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Close any open connections
                if (serial != null && serial.IsOpen)
                {
                    serial.Close();
                }
                
                // Update UI to reflect disabled state
                connectBtn.Enabled = false;
                sendCommandBtn.Enabled = false;
                button1.Enabled = false;
                comboBoxDevices.Enabled = false;

                _settingsService.SetSetting("plc", "is_used", "False");

                // Log the state change
                Console.WriteLine("[PLC] PLC functionality disabled by user");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while disabling PLC: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"[PLC] Error disabling PLC: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the logic when PLC functionality is enabled
        /// </summary>
        private void HandlePlcEnabledState()
        {
            try
            {
                // Show message to user
                MessageBox.Show("PLC functionality has been enabled.", "PLC Status", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Update UI to reflect enabled state
                connectBtn.Enabled = true;
                sendCommandBtn.Enabled = true;
                button1.Enabled = true;
                comboBoxDevices.Enabled = true;

                _settingsService.SetSetting("plc", "is_used", "True");

                // Log the state change
                Console.WriteLine("[PLC] PLC functionality enabled by user");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while enabling PLC: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"[PLC] Error enabling PLC: {ex.Message}");
            }
        }
    }
}