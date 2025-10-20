using PLCCommunication;
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    public class PlcOperation
    {
        private SerialPortStream _serial;
        private readonly StringBuilder _rxBuffer = new StringBuilder();
        private int _lineStart;
        private readonly Stopwatch _cycleStopwatch = new Stopwatch();
        private bool _waitingResponse;
        private readonly InspectionLogger _logger = new InspectionLogger();

        public event Action<string> LineReceived;
        public event Action<double> CycleMeasured;

        public PlcOperation(string portName, int baudRate = 9600,
                          int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            InitializeSerial(portName, baudRate, dataBits, parity, stopBits);
        }

        private void InitializeSerial(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            _serial = new SerialPortStream(portName, baudRate, dataBits, parity, stopBits)
            {
                NewLine = "\r",
                Encoding = Encoding.ASCII,
                ReadBufferSize = 2 * 1024 * 1024 // 2 MB
            };
        }

        public void SetConfig(
            string portName = null,
            int? baudRate = null
        )
        {
            bool wasOpen = _serial.IsOpen;
            if (wasOpen) _serial.Close();

            // Update konfigurasi serial
            if (!string.IsNullOrEmpty(portName)) _serial.PortName = portName;
            if (baudRate.HasValue) _serial.BaudRate = baudRate.Value;
        }

        /* ---------------  open / close  --------------- */
        public void Open()
        {
            if (!_serial.IsOpen)
            {
                _serial.Open();
                _ = Task.Run(ReadPumpAsync); // <— polling tanpa event
            }
        }

        public void Close() => _serial.Close();

        public bool IsOpen => _serial.IsOpen;

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

                using (var cts = new CancellationTokenSource(timeoutMs)){
                try
                {
                    while (_waitingResponse && !cts.IsCancellationRequested)
                        await Task.Delay(10, cts.Token);
                }
                    catch (OperationCanceledException) { /* timeout */ }
                };

                _cycleStopwatch.Stop();

                times.Add(_waitingResponse ? timeoutMs          // timeout
                                           : _cycleStopwatch.Elapsed.TotalMilliseconds);

                _waitingResponse = false;
                await Task.Delay(200); // antar loop
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
            _logger.AddLog(result);
            _logger.SaveToJson();
        }

        /* ---------------  read-pump tanpa event  --------------- */
        private async Task ReadPumpAsync()
        {
            var buf = new byte[4096];
            while (IsOpen)
            {
                int read = await _serial.ReadAsync(buf, 0, buf.Length);
                if (read == 0) break;

                string chunk = Encoding.ASCII.GetString(buf, 0, read);
                ParseChunk(chunk);
            }
        }

        /* ---------------  parser line-oriented  --------------- */
        private void ParseChunk(string chunk)
        {
            if (string.IsNullOrEmpty(chunk)) return;

            lock (_rxBuffer) _rxBuffer.Append(chunk);

            string buffered;
            lock (_rxBuffer) buffered = _rxBuffer.ToString();

            int cur = 0;
            while (cur < buffered.Length)
            {
                char ch = buffered[cur];
                if (ch == '\r' || ch == '\n')
                {
                    int len = cur - _lineStart;
                    if (len > 0)
                        LineReceived?.Invoke(buffered.Substring(_lineStart, len).Trim());

                    cur++;
                    _lineStart = cur;
                }
                else cur++;
            }

            // sisakan sisa karakter
            lock (_rxBuffer)
            {
                _rxBuffer.Clear();
                if (_lineStart < buffered.Length)
                    _rxBuffer.Append(buffered, _lineStart, buffered.Length - _lineStart);
            }
            _lineStart = 0;

            // flag selesai menunggu response
            if (_waitingResponse && buffered.Contains("PASS"))
                _waitingResponse = false;
        }

        /* ---------------  dispose  --------------- */
        public void Dispose()
        {
            Close();
            _serial?.Dispose();
        }
    }
}