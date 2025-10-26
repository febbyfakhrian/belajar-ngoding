using AutoInspectionPlatform;
using PLCCommunication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using System.Text;
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
        private SerialPort serial;
        private Stopwatch cycleStopwatch = new Stopwatch();
        private bool waitingResponse = false;
        private bool testRunning = false;
        private List<double> cycleTimes = new List<double>();
        private InspectionLogger logger = new InspectionLogger(); // bisa diinisialisasi global/form
        private StringBuilder rxBuffer = new StringBuilder();
        private int _lineStart = 0;   // posisi awal baris saat ini
        private readonly IServiceProvider _serviceProvider;
        private IPlcService _plcService;

        // Constructor with dependency injection
        public PlcDialog(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
            // Setup Serial Port
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
            comboBoxDevices.Items.Clear();
            comboBoxDevices.Items.AddRange(SerialPort.GetPortNames());
            if (comboBoxDevices.Items.Count > 0)
                comboBoxDevices.SelectedIndex = 0;
            else
                comboBoxDevices.Items.Add("No COM ports found");
        }

        private void comboBoxDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TIDAK buka port di sini – cukup simpan nama
            Console.WriteLine($"[INFO] Port dipilih: {comboBoxDevices.SelectedItem}");
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

            // Simpan ke list benchmark
            if (testRunning)
                cycleTimes.Add(cycleMs);
        }

        private void button1_Click(object sender, EventArgs e)
        {
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
            try
            {
                var button = sender as Button;
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

                // Get the PLC service from DI
                var plcService = GetPlcService();
                if (plcService == null || !plcService.IsOpen)
                {
                    MessageBox.Show("Serial port belum dibuka!");
                    return;
                }

                plcService.SendCommandAsync(command).Wait(); // Blocking call for simplicity
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
                if (string.IsNullOrWhiteSpace(baudRateTextBox.Text))
                {
                    MessageBox.Show("Masukkan baud rate!");
                    return;
                }

                string portName = comboBoxDevices.SelectedItem?.ToString();
                if (string.IsNullOrWhiteSpace(portName))
                {
                    MessageBox.Show("Pilih port terlebih dahulu!");
                    return;
                }

                // Get the PLC service from DI
                var plcService = GetPlcService();
                if (plcService == null)
                {
                    MessageBox.Show("PLC service tidak tersedia!");
                    return;
                }

                // If it's a PlcOperation instance, we can configure it
                if (plcService is PlcOperation plcOperation)
                {
                    plcOperation.SetConfig(portName, Int32.Parse(baudRateTextBox.Text));

                    if (plcOperation.IsOpen)
                    {
                        plcOperation.Close();
                    }

                    // Buat instance baru (atau reuse) dan buka
                    plcOperation.Open();

                    connectBtn.Text = "Disconnect";
                    comboBoxDevices.Enabled = false;
                    MessageBox.Show($"Terkoneksi ke {portName}");
                }
                else
                {
                    MessageBox.Show("PLC service tidak dapat dikonfigurasi!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void sendCommandBtn_Click(object sender, EventArgs e)
        {
            // Get the PLC service from DI
            var plcService = GetPlcService();
            if (plcService == null || !plcService.IsOpen)
            {
                MessageBox.Show("Port belum dibuka!");
                return;
            }

            string cmd = inputCommandPlc.Text.Trim();
            if (string.IsNullOrEmpty(cmd)) return;

            // Tambah terminator sesuai PLC
            byte[] data = Encoding.ASCII.GetBytes(cmd + "\r");
            plcService.SendCommandAsync(data).Wait(); // Blocking call for simplicity
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
    }
}