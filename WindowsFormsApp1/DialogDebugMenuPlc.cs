using Newtonsoft.Json;
using PLCCommunication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1
{
    public partial class DialogDebugMenuPlc : Form
    {
        private SerialPort serial;
        private Stopwatch cycleStopwatch = new Stopwatch();
        private bool waitingResponse = false;
        private bool testRunning = false;
        private int loopCount = 0;
        private int totalLoops = 20; // jumlah percobaan
        private List<double> cycleTimes = new List<double>();
        private InspectionLogger logger = new InspectionLogger(); // bisa diinisialisasi global/form
        private StringBuilder rxBuffer = new StringBuilder();
        private int _lineStart = 0;   // posisi awal baris saat ini

        public DialogDebugMenuPlc()
        {
            InitializeComponent();
            // Setup Serial Port
        }

        private void DialogDebugMenuPlc_Load(object sender, EventArgs e)
        {
            LoadComPorts();

            // === Setup ComboBox COM Port ===
            comboBoxDevices.Items.AddRange(SerialPort.GetPortNames());
            if (comboBoxDevices.Items.Count > 0)
                comboBoxDevices.SelectedIndex = 0;

            // === Generate Buttons Dynamically ===
            GenerateButtonsFromClass(flowMain, typeof(WritePLCAddress)); // Umum
            GenerateCameraButtons(flowCam0, "0");
            GenerateCameraButtons(flowCam1, "1");
            GenerateButtonsFromClass(flowResponse, typeof(PLCResponseMessage)); // Response PLC
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

        private async void testPlcReadBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serial.IsOpen)
                    serial.Open();

                if (testRunning)
                {
                    MessageBox.Show("Test sudah berjalan!");
                    return;
                }

                testRunning = true;
                cycleTimes.Clear();
                loopCount = 0;

                Console.WriteLine($"Mulai pengukuran cycle time ({totalLoops} kali loop)...");

                for (int i = 0; i < totalLoops; i++)
                {
                    loopCount = i + 1;

                    waitingResponse = true;
                    cycleStopwatch.Restart();

                    byte[] command = WritePLCAddress.READ;
                    serial.Write(command, 0, command.Length);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Kirim READ #{loopCount}");

                    // Tunggu sampai respons diterima atau timeout 2 detik
                    int timeoutMs = 2000;
                    int waited = 0;
                    while (waitingResponse && waited < timeoutMs)
                    {
                        await Task.Delay(10);
                        waited += 10;
                    }

                    if (waitingResponse)
                    {
                        Console.WriteLine($"Timeout! PLC tidak merespons pada percobaan #{loopCount}");
                        waitingResponse = false;
                        cycleTimes.Add(timeoutMs);
                    }

                    await Task.Delay(200); // jeda antar pengiriman agar PLC tidak overload
                }

                testRunning = false;
                ShowCycleTimeSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error membuka serial: " + ex.Message);
                testRunning = false;
            }
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // 1. Baca semua data yang datang
                string chunk = serial.ReadExisting();
                Console.WriteLine(chunk);
                if (string.IsNullOrEmpty(chunk)) return;

                lock (rxBuffer) rxBuffer.Append(chunk);

                // 2. Cek apakah sudah ada baris utuh (CR/LF)
                string buffered = rxBuffer.ToString();
                int current = 0;
                int len = buffered.Length;

                while (current < len)
                {
                    char ch = buffered[current];
                    if (ch == '\r' || ch == '\n')
                    {
                        int lineLen = current - _lineStart;
                        if (lineLen > 0)
                        {
                            string line = buffered.Substring(_lineStart, lineLen).Trim();
                            ProcessCompleteLine(line);
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
                if (finalLen > 0) ProcessCompleteLine(buffered.Substring(_lineStart, finalLen).Trim());

                // 3. Sisa yang belum selesai
                lock (rxBuffer)
                {
                    rxBuffer.Clear();
                    if (_lineStart < len)
                        rxBuffer.Append(buffered, _lineStart, len - _lineStart);
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

        private void ShowCycleTimeSummary()
        {
            if (cycleTimes.Count == 0)
            {
                Console.WriteLine("Tidak ada data cycle time yang valid.");
                return;
            }

            double avg = 0;
            double min = double.MaxValue;
            double max = 0;

            foreach (var t in cycleTimes)
            {
                avg += t;
                if (t < min) min = t;
                if (t > max) max = t;
            }

            avg /= cycleTimes.Count;

            Console.WriteLine("\n=== HASIL PENGUKURAN CYCLE TIME ===");
            Console.WriteLine($"Total percobaan: {cycleTimes.Count}");
            Console.WriteLine($"Rata-rata: {avg:F2} ms");
            Console.WriteLine($"Minimum  : {min:F2} ms");
            Console.WriteLine($"Maksimum : {max:F2} ms");
            Console.WriteLine("===================================");

            // Buat struktur data untuk disimpan
            var summary = new
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                totalTests = cycleTimes.Count,
                averageMs = Math.Round(avg, 2),
                minMs = Math.Round(min, 2),
                maxMs = Math.Round(max, 2),
                logs = new List<object>()
            };

            // Isi array logs
            for (int i = 0; i < cycleTimes.Count; i++)
            {
                summary.logs.Add(new
                {
                    index = i + 1,
                    cycleTimeMs = Math.Round(cycleTimes[i], 2)
                });
            }

            // Simpan ke file JSON
            string json = JsonConvert.SerializeObject(summary, Formatting.Indented);

            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string filePath = Path.Combine(folder, $"CycleTime_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            File.WriteAllText(filePath, json);

            Console.WriteLine($"📁 File hasil disimpan ke: {filePath}");
        }


        private async void testPlcWriteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serial.IsOpen)
                    serial.Open();

                Console.WriteLine("=== MULAI TEST WRITE PLC (NEXT/PASS/FAIL) ===");

                // Kirim perintah NEXT ke PLC
                await SendWriteCommand(WritePLCAddress.NEXT, "NEXT");

                // Tunggu sebentar (simulate post toggle)
                await Task.Delay(200);

                // Kirim POST_NEXT untuk reset signal
                await SendWriteCommand(WritePLCAddress.POST_NEXT, "POST_NEXT");

                Console.WriteLine("=== TEST WRITE SELESAI ===");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Gagal mengirim data ke PLC: {ex.Message}");
                Console.WriteLine($"❌ Gagal mengirim data ke PLC: {ex.Message}");
            }
        }

        private async Task SendWriteCommand(byte[] command, string label)
        {
            try
            {
                cycleStopwatch.Restart();
                waitingResponse = true;

                serial.Write(command, 0, command.Length);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Kirim command {label}");

                int timeoutMs = 2000;
                int waited = 0;
                string plcResponse = "";

                while (waitingResponse && waited < timeoutMs)
                {
                    await Task.Delay(10);
                    waited += 10;
                }

                cycleStopwatch.Stop();

                double elapsed = cycleStopwatch.Elapsed.TotalMilliseconds;

                if (waitingResponse)
                {
                    Console.WriteLine($"⚠️ Timeout ({label}) setelah {timeoutMs} ms");
                    waitingResponse = false;
                }
                else
                {
                    Console.WriteLine($"✅ Respons {label} diterima dalam {elapsed:F2} ms: {plcResponse}");
                }

                // Simpan log ke JSON
                var logs = new List<CycleLog>
                {
                    new CycleLog
                    {
                        TransactionId = 1,
                        Start = DateTime.Now.AddMilliseconds(-elapsed),
                        End = DateTime.Now,
                        CycleTimeMs = elapsed,
                        Response = plcResponse
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saat mengirim command {label}: {ex.Message}");
            }
        }

        private async void testPlcWriteReadBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serial.IsOpen)
                    serial.Open();

                Console.WriteLine("=== MULAI TEST WRITE+READ PLC ===");

                // Step 1: Kirim perintah WRITE (NEXT)
                cycleStopwatch.Restart();
                waitingResponse = true;

                serial.Write(WritePLCAddress.NEXT, 0, WritePLCAddress.NEXT.Length);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Kirim command NEXT");

                int timeoutMs = 2000;
                int waited = 0;
                string writeResponse = "";

                while (waitingResponse && waited < timeoutMs)
                {
                    await Task.Delay(10);
                    waited += 10;
                }

                if (waitingResponse)
                {
                    Console.WriteLine("⚠️ Timeout saat menunggu respon WRITE");
                    waitingResponse = false;
                }
                else
                {
                    Console.WriteLine("✅ Respons WRITE diterima");
                }

                cycleStopwatch.Stop();
                double writeCycle = cycleStopwatch.Elapsed.TotalMilliseconds;

                await Task.Delay(300); // beri jeda sebelum READ

                // Step 2: Kirim perintah READ
                cycleStopwatch.Restart();
                waitingResponse = true;

                serial.Write(WritePLCAddress.READ, 0, WritePLCAddress.READ.Length);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Kirim command READ");

                waited = 0;
                string readResponse = "";

                while (waitingResponse && waited < timeoutMs)
                {
                    await Task.Delay(10);
                    waited += 10;
                }

                cycleStopwatch.Stop();
                double readCycle = cycleStopwatch.Elapsed.TotalMilliseconds;

                if (waitingResponse)
                {
                    Console.WriteLine("⚠️ Timeout saat menunggu respon READ");
                    waitingResponse = false;
                }
                else
                {
                    Console.WriteLine("✅ Respons READ diterima");
                }

                double totalCycle = writeCycle + readCycle;

                Console.WriteLine($"=== HASIL ===");
                Console.WriteLine($"Write Cycle : {writeCycle:F2} ms");
                Console.WriteLine($"Read Cycle  : {readCycle:F2} ms");
                Console.WriteLine($"Total Cycle : {totalCycle:F2} ms");
                Console.WriteLine("===================================");

                // Step 3: Simpan hasil ke JSON
                var logs = new List<CycleLog>
                {
                    new CycleLog
                    {
                        TransactionId = 1,
                        Start = DateTime.Now.AddMilliseconds(-totalCycle),
                        End = DateTime.Now,
                        CycleTimeMs = totalCycle,
                        Response = $"WRITE: {writeResponse} | READ: {readResponse}"
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Gagal menjalankan test Write+Read: {ex.Message}");
                Console.WriteLine($"❌ Error: {ex}");
            }
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
        // GENERATE BUTTONS PER CAMERA GROUP
        // ==========================================================
        private void GenerateCameraButtons(FlowLayoutPanel flow, string cameraId)
        {
            if (flow == null) return;

            var fields = typeof(WritePLCAddressV2)
                .GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                if (field.Name.Contains(cameraId))
                {
                    Button btn = new Button
                    {
                        Text = field.Name,
                        Tag = field.Name,
                        Width = 120,
                        Height = 35,
                        Margin = new Padding(6)
                    };
                    btn.Click += PlcCommandButton_Click;
                    flow.Controls.Add(btn);
                }
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
                    string commandToSend = cmd + "\r"; // ← kirim CR
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

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }

}
