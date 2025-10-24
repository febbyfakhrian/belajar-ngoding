using PLCCommunication;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Domain.Flow.Engine;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1.Domain.Actions
{
    public sealed class PlcLampOnAction : Flow.Engine.IFlowAction
    {
        public string Key => "Plc.LampOn";
        private readonly PlcOperation _plc;
        public PlcLampOnAction(PlcOperation plc) => _plc = plc;
        public Task ExecuteAsync(Flow.Engine.IFlowContext ctx, CancellationToken ct = default)
        {
            Debug.WriteLine("ini lampu hidup");
            if (_plc?.IsOpen == true) _plc.SendCommand(WritePLCAddress.NEXT);
            return Task.CompletedTask;
        }
    }

    public sealed class PlcSendPassAction : Flow.Engine.IFlowAction
    {
        public string Key => "Plc.SendPass";
        private readonly PlcOperation _plc;
        public PlcSendPassAction(PlcOperation plc) => _plc = plc;
        public Task ExecuteAsync(Flow.Engine.IFlowContext ctx, CancellationToken ct = default)
        {
            if (_plc?.IsOpen == true) _plc.SendCommand(WritePLCAddress.PASS);
            return Task.CompletedTask;
        }
    }

    public sealed class PlcSendFailAction : Flow.Engine.IFlowAction
    {
        public string Key => "Plc.SendFail";
        private readonly PlcOperation _plc;
        public PlcSendFailAction(PlcOperation plc) => _plc = plc;
        public Task ExecuteAsync(Flow.Engine.IFlowContext ctx, CancellationToken ct = default)
        {
            if (_plc?.IsOpen == true) _plc.SendCommand(WritePLCAddress.FAIL);
            return Task.CompletedTask;
        }
    }

    // Enable the PLC READ listener
    public sealed class PlcSubscribeReadAction : IFlowAction
    {
        public string Key => "Plc.SubscribeRead";
        private readonly PlcReadSubscription _sub;

        public PlcSubscribeReadAction(PlcReadSubscription sub) => _sub = sub;

        public Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            _sub.Subscribe();        // mulai listening
            return Task.CompletedTask;
        }
    }

    // Disable the PLC READ listener
    public sealed class PlcUnsubscribeReadAction : IFlowAction
    {
        public string Key { get { return "Plc.UnsubscribeRead"; } }
        private readonly PlcReadSubscription _sub;

        public PlcUnsubscribeReadAction(PlcReadSubscription sub)
        {
            _sub = sub;
        }

        public Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default(CancellationToken))
        {
            _sub.Unsubscribe();
            return Task.CompletedTask;
        }
    }

    public sealed class PlcReadSubscription
    {
        private readonly PlcOperation _plc;
        private readonly byte[] _readBytes;
        private Action<string> _attachedHandler;
        private readonly IFlowContext _ctx;

        public PlcReadSubscription(PlcOperation plc, IFlowContext ctx)
        {
            _plc = plc;
            _readBytes = WritePLCAddress.READ;
            _ctx = ctx;
        }

        public void Subscribe()
        {
            if (_plc == null) return;
            if (_attachedHandler != null) return; // sudah subscribe

            // 1. buat delegate dulu
            _attachedHandler = async (lineRaw) =>
            {
                await HandleLineAsync(lineRaw);
            };

            // 2. baru pasang ke event
            _plc.LineReceived += _attachedHandler;
        }

        public void Unsubscribe()
        {
            if (_plc == null) return;
            if (_attachedHandler == null) return;

            _plc.LineReceived -= _attachedHandler;
            _attachedHandler = null;
        }

        private async Task HandleLineAsync(string lineRaw)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lineRaw)) return;

                string cmd = Encoding.ASCII.GetString(_readBytes).TrimEnd('\r', '\n');
                lineRaw = lineRaw.Replace("\\r", "\r").Replace("\\n", "\n");
                string lineClean = lineRaw.TrimEnd('\r', '\n');

                if (cmd.ToUpper().Equals(lineClean.ToUpper(), StringComparison.Ordinal))
                {
                    Debug.WriteLine("read signal");
                    _ctx.Trigger = "PLC_READ_RECEIVED";
                    // keep the original behavior: immediately run Inspect flow
                }
            }
            catch
            {
                // swallow – avoid breaking the subscription loop on sporadic parse errors
            }
        }
    }
}
