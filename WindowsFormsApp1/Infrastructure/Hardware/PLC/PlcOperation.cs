using PLCCommunication;
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;
using WindowsFormsApp1.Core.Entities.Models;
using WindowsFormsApp1.Core.Interfaces;
using WindowsFormsApp1.Infrastructure.Services;

namespace WindowsFormsApp1.Infrastructure.Hardware.PLC
{
    public class PlcOperation : IPlcService
    {
        private SerialPortStream _serial;
        private string _configuredPortName; // <— simpan nama port di sini
        private readonly StringBuilder _rxBuffer = new StringBuilder();
        private int _lineStart;
        private readonly Stopwatch _cycleStopwatch = new Stopwatch();
        private bool _waitingResponse;
        private readonly InspectionLogger _logger = new InspectionLogger();

        public event Action<string> LineReceived;
        public event Action<double> CycleMeasured;

        // Parameterless constructor for DI
        public PlcOperation() : this("COM1", 9600)
        {
        }

        public PlcOperation(string portName, int baudRate = 9600,
                          int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            InitializeSerial(portName, baudRate, dataBits, parity, stopBits);
        }

        private void InitializeSerial(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            _configuredPortName = portName; // <— simpan
            _serial = new SerialPortStream(portName, baudRate, dataBits, parity, stopBits)
            {
                NewLine = "\r",
                Encoding = Encoding.ASCII,
                ReadBufferSize = 2 * 1024 * 1024 // 2 MB
            };
        }

        /// <summary>
        /// Cek apakah device/port yang dikonfigurasi tersedia di sistem.
        /// </summary>
        public bool DeviceExists()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_configuredPortName)) return false;

                string[] ports = System.IO.Ports.SerialPort.GetPortNames();
                return ports.Any(p => string.Equals(p, _configuredPortName, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        public void SetConfig(string portName = null, int? baudRate = null)
        {
            bool wasOpen = _serial?.IsOpen == true;
            if (wasOpen)
            {
                try { _serial.Close(); } catch { /* ignore */ }
            }

            if (!string.IsNullOrEmpty(portName))
            {
                _configuredPortName = portName;  // <— sinkronkan field
                _serial.PortName = portName;
            }
            if (baudRate.HasValue) _serial.BaudRate = baudRate.Value;
        }

        /* ---------------  open / close  --------------- */
        /// <summary>
        /// Buka hanya jika device ada. Kalau tidak ada, aplikasi tetap jalan tanpa exception.
        /// </summary>
        public void Open()
        {
            // Skip jika device/port tidak terdeteksi
            if (!DeviceExists())
            {
                Debug.WriteLine($"[PLC] Port '{_configuredPortName}' tidak ditemukan. Skip open().");
                return;
            }

            if (_serial == null) throw new InvalidOperationException("Serial belum diinisialisasi.");

            if (!_serial.IsOpen)
            {
                _serial.Open();
                _ = Task.Run(ReadPumpAsync); // polling tanpa event
            }
        }

        public void Close()
        {
            try
            {
                if (_serial?.IsOpen == true) _serial.Close();
            }
            catch { /* ignore */ }
        }

        public bool IsOpen => _serial?.IsOpen == true;

        public void SendCommand(byte[] command)
        {
            if (!IsOpen) throw new InvalidOperationException("Port not open");
            _serial.Write(command, 0, command.Length);
        }

        /* ---------------  benchmark cycle-time  --------------- */
        public async Task<double> ReadCycleTimeAsync(int totalLoops = 20, int timeoutMs = 2000)
        {
            if (!IsOpen) throw new InvalidOperationException("Port not open");

            var times = new List<double>(totalLoops);

            for (int i = 0; i < totalLoops; i++)
            {
                _waitingResponse = true;
                _cycleStopwatch.Restart();

                SendCommand(WritePLCAddress.READ);

                using (var cts = new CancellationTokenSource(timeoutMs))
                {
                    try
                    {
                        while (_waitingResponse && !cts.IsCancellationRequested)
                            await Task.Delay(10, cts.Token);
                    }
                    catch (OperationCanceledException) { /* timeout */ }
                }

                _cycleStopwatch.Stop();

                times.Add(_waitingResponse
                    ? timeoutMs // timeout
                    : _cycleStopwatch.Elapsed.TotalMilliseconds);

                _waitingResponse = false;
                await Task.Delay(50); // jeda antar loop
            }

            double avg = times.Count == 0 ? 0 : times.Average();
            CycleMeasured?.Invoke(avg);
            return avg;
        }

        public void SaveCycleResult(double avgMs, string response)
        {
            var result = new InspectionResult
            {
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                CycleTimeMs = avgMs,
                RawResponse = response ?? "",
                Pass = response?.Contains("PASS") == true
            };
            //_logger.AddLog(result);
            _logger.SaveToJson();
        }

        /* ---------------  read-pump tanpa event  --------------- */
        private async Task ReadPumpAsync()
        {
            var buf = new byte[4096];
            try
            {
                while (IsOpen)
                {
                    int read = await _serial.ReadAsync(buf, 0, buf.Length);
                    if (read == 0) break;

                    string chunk = Encoding.ASCII.GetString(buf, 0, read);
                    ParseChunk(chunk);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PLC] ReadPumpAsync error: {ex.Message}");
            }
        }

        /* ---------------  parser line-oriented  --------------- */
        private void ParseChunk(string chunk)
        {
            if (string.IsNullOrEmpty(chunk)) return;
            Debug.WriteLine($"[PLC] Received chunk: {chunk}");

            lock (_rxBuffer) _rxBuffer.Append(chunk);

            string buffered;
            lock (_rxBuffer) buffered = _rxBuffer.ToString();
            Debug.WriteLine($"[PLC] Buffer content: {buffered}");

            int lineEnd;
            
            // Process complete lines (terminated with \r, \n, or \r\n)
            while ((lineEnd = buffered.IndexOfAny(new[] { '\r', '\n' }, _lineStart)) >= 0)
            {
                int len = lineEnd - _lineStart;

                // Skip \r\n as a unit, otherwise skip 1 character
                int skip = 1;
                if (lineEnd + 1 < buffered.Length &&
                    buffered[lineEnd] == '\r' && buffered[lineEnd + 1] == '\n')
                {
                    skip = 2;
                }

                string line = buffered.Substring(_lineStart, len).Trim();
                _lineStart = lineEnd + skip;
                
                Debug.WriteLine($"[PLC] Processing complete line: {line}");

                if (!string.IsNullOrWhiteSpace(line))
                {
                    LineReceived?.Invoke(line);
                    _waitingResponse = false;
                }
            }

            // ---------- 2.  compact buffer ----------
            // Remove processed lines from buffer to prevent reprocessing
            if (_lineStart > 0)
            {
                // Check if there's any remaining unprocessed data
                lock (_rxBuffer)
                {
                    if (_lineStart < _rxBuffer.Length)
                    {
                        string remaining = _rxBuffer.ToString(_lineStart, _rxBuffer.Length - _lineStart);
                        if (!string.IsNullOrWhiteSpace(remaining))
                        {
                            Debug.WriteLine($"[PLC] Remaining unprocessed data: {remaining}");
                        }
                    }
                }
                
                Debug.WriteLine($"[PLC] Compacting buffer, removing {_lineStart} characters");
                lock (_rxBuffer)
                {
                    // Compact the buffer to remove processed data
                    if (_lineStart < _rxBuffer.Length)
                    {
                        _rxBuffer.Remove(0, _lineStart);
                    }
                    else
                    {
                        _rxBuffer.Clear(); // All data has been processed
                    }
                    _lineStart = 0;
                }
            }

            // ---------- 3.  buffer overflow protection ----------
            lock (_rxBuffer)
            {
                if (_rxBuffer.Length > 8192)
                {
                    Debug.WriteLine($"[PLC] Warning: RX buffer size is {_rxBuffer.Length} characters, clearing...");
                    _rxBuffer.Clear();
                    _lineStart = 0;
                }
            }
        }

        public Task<bool> OpenAsync()
        {
            try
            {
                Open();
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task CloseAsync()
        {
            Close();
            return Task.CompletedTask;
        }

        public Task SendCommandAsync(byte[] command)
        {
            SendCommand(command);
            return Task.CompletedTask;
        }

        public Task<string> ReadDataAsync()
        {
            // This is a simplified implementation
            // In a real scenario, you would need to implement proper async reading
            return Task.FromResult<string>(null);
        }

        public void Dispose()
        {
            try
            {
                Close();
                _serial?.Dispose();
            }
            catch { /* ignore */ }
        }
    }
}