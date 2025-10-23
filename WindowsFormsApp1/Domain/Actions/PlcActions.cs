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
            Debug.WriteLine("sdfsd");
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
        public string Key { get { return "Plc.SubscribeRead"; } }
        private readonly PlcReadSubscription _sub;
        private readonly ITriggerBus _bus;

        public PlcSubscribeReadAction(PlcReadSubscription sub, ITriggerBus bus)
        {
            _sub = sub; _bus = bus;
        }

        public Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default(CancellationToken))
        {
            _sub.Subscribe(_bus);
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
        private Action<string> _attachedHandler; // wrap to a stable delegate

        public PlcReadSubscription(PlcOperation plc)
        {
            _plc = plc;
            _readBytes = WritePLCAddress.READ;
        }

        public void Subscribe(ITriggerBus bus)
        {
            if (_plc == null) return;
            if (_attachedHandler != null) return; // already subscribed
            Debug.WriteLine("sfdf");

            _attachedHandler = (lineRaw) =>
            {
                // Always hop to fire triggers asynchronously
                _ = HandleLineAsync(bus, lineRaw);
            };

            _plc.LineReceived += _attachedHandler;
        }

        public void Unsubscribe()
        {
            if (_plc == null) return;
            if (_attachedHandler == null) return;

            _plc.LineReceived -= _attachedHandler;
            _attachedHandler = null;
        }

        private async Task HandleLineAsync(ITriggerBus bus, string lineRaw)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lineRaw)) return;

                string cmd = Encoding.ASCII.GetString(_readBytes).TrimEnd('\r', '\n');
                string lineClean = lineRaw.Replace("\\r", "\r").Replace("\\n", "\n").TrimEnd('\r', '\n');

                if (string.Equals(cmd, lineClean, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("read signal");
                    // keep the original behavior: immediately run Inspect flow
                    await bus.FireAsync("SignalRead");   // go WaitingSignal -> Grabbing (Camera.Prepare)
                    await bus.FireAsync("InspectFrame"); // go Grabbing -> Inspecting (CaptureFrame+Grpc+Render)
                }
            }
            catch
            {
                // swallow – avoid breaking the subscription loop on sporadic parse errors
            }
        }
    }
}
