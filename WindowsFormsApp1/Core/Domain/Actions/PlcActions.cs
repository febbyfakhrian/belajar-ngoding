using PLCCommunication;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Domain.Flow.Engine;
using WindowsFormsApp1.Core.Interfaces;
using WindowsFormsApp1.Infrastructure.Hardware.PLC;

namespace WindowsFormsApp1.Core.Domain.Actions
{
    public sealed class PlcLampOnAction : BaseAction
    {
        public override string Key => "Plc.LampOn";
        private readonly IPlcService _plc;
        public PlcLampOnAction(IPlcService plc) => _plc = plc;
        
        public override async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            try
            {
                LogInfo("Turning on lamp");
                if (_plc?.IsOpen == true) await _plc.SendCommandAsync(WritePLCAddress.NEXT);
                LogInfo("Lamp turned on");
            }
            catch (Exception ex)
            {
                LogError($"Failed to turn on lamp: {ex.Message}");
                throw;
            }
        }
    }

    public sealed class PlcSendPassAction : BaseAction
    {
        public override string Key => "Plc.SendPass";
        private readonly IPlcService _plc;
        public PlcSendPassAction(IPlcService plc) => _plc = plc;
        
        public override async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            try
            {
                LogInfo("Sending PASS command");
                if (_plc?.IsOpen == true) await _plc.SendCommandAsync(WritePLCAddress.PASS);
                LogInfo("PASS command sent");
            }
            catch (Exception ex)
            {
                LogError($"Failed to send PASS command: {ex.Message}");
                throw;
            }
        }
    }

    public sealed class PlcSendFailAction : BaseAction
    {
        public override string Key => "Plc.SendFail";
        private readonly IPlcService _plc;
        public PlcSendFailAction(IPlcService plc) => _plc = plc;
        
        public override async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            try
            {
                LogInfo("Sending FAIL command");
                if (_plc?.IsOpen == true) await _plc.SendCommandAsync(WritePLCAddress.FAIL);
                LogInfo("FAIL command sent");
            }
            catch (Exception ex)
            {
                LogError($"Failed to send FAIL command: {ex.Message}");
                throw;
            }
        }
    }

    // Enable the PLC READ listener
    public sealed class PlcSubscribeReadAction : BaseAction
    {
        public override string Key => "Plc.SubscribeRead";
        private readonly PlcReadSubscription _sub;

        public PlcSubscribeReadAction(PlcReadSubscription sub) => _sub = sub;

        public override Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            try
            {
                LogInfo("Subscribing to PLC read events");
                _sub.Subscribe();        // mulai listening
                LogInfo("Subscribed to PLC read events");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogError($"Failed to subscribe to PLC read events: {ex.Message}");
                throw;
            }
        }
    }

    // Disable the PLC READ listener
    public sealed class PlcUnsubscribeReadAction : BaseAction
    {
        public override string Key => "Plc.UnsubscribeRead";
        private readonly PlcReadSubscription _sub;

        public PlcUnsubscribeReadAction(PlcReadSubscription sub)
        {
            _sub = sub;
        }

        public override Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                LogInfo("Unsubscribing from PLC read events");
                _sub.Unsubscribe();
                LogInfo("Unsubscribed from PLC read events");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogError($"Failed to unsubscribe from PLC read events: {ex.Message}");
                throw;
            }
        }
    }

    public sealed class PlcReadSubscription
    {
        private readonly IPlcService _plc;
        private readonly byte[] _readBytes;
        private Action<string> _attachedHandler;
        private readonly IFlowContext _ctx;

        public PlcReadSubscription(IPlcService plc, IFlowContext ctx)
        {
            _plc = plc;
            _readBytes = WritePLCAddress.READ;
            _ctx = ctx;
        }

        public void Subscribe()
        {
            if (_plc == null || _attachedHandler != null) return;

            // 1.  ASK the PLC to start sending READ lines
            _plc.SendCommandAsync(_readBytes);   // <-- missing line

            // 2.  now listen for the answers
            _attachedHandler = async lineRaw => await HandleLineAsync(lineRaw);
            _plc.LineReceived += _attachedHandler;
        }

        public void Unsubscribe()
        {
            if (_plc == null) return;
            if (_attachedHandler == null) return;

            _plc.LineReceived -= _attachedHandler;
            _attachedHandler = null;
        }

        private Task HandleLineAsync(string lineRaw)
        {
            try
            {
                Debug.WriteLine($"lineRaw {lineRaw}");
                string cmd = Encoding.ASCII.GetString(_readBytes).TrimEnd('\r', '\n');
                lineRaw = lineRaw.Replace("\\r", "\r").Replace("\\n", "\n");
                string lineClean = lineRaw.TrimEnd('\r', '\n');

                Debug.WriteLine($"lineClean {lineClean}");
                Debug.WriteLine($"cmd {cmd}");
                _ctx.Trigger = "PLC_READ_RECEIVED";
                if (cmd.ToUpper().Equals(lineClean.ToUpper(), StringComparison.Ordinal))
                {
                    Debug.WriteLine("read signal");
                    _ctx.Trigger = "PLC_READ_RECEIVED";
                }
            }
            catch
            {
                Debug.WriteLine("sdfsdf");
                // swallow – avoid breaking the subscription loop on sporadic parse errors
            }

            return Task.CompletedTask;
        }
    }
}