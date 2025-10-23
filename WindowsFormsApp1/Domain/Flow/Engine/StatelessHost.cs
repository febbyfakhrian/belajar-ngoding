using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Domain.Flow.Yaml;

namespace WindowsFormsApp1.Domain.Flow.Engine
{
    public sealed class StatelessHost
    {
        private readonly StateMachine<string, string> _sm;
        private readonly IActionRegistry _actions;
        private readonly IFlowContext _ctx;

        private readonly List<YamlAutoTrigger> _autoTriggers; // ALWAYS initialized

        public event Action<string> StateChanged;
        public event Action<string> TransitionCompleted;
        public string State { get { return _sm.State; } }

        public Func<string, IFlowContext, bool> Evaluate = DefaultEval;

        // Expose a read-only view so no one can null it out
        public IList<YamlAutoTrigger> AutoTriggers { get { return _autoTriggers; } }

        public StatelessHost(YamlFlowDefinition def, IActionRegistry actions, IFlowContext ctx)
        {
            _actions = actions;
            _ctx = ctx;
            _autoTriggers = new List<YamlAutoTrigger>();

            _sm = new StateMachine<string, string>(def.States[0]);

            foreach (var tr in def.Transitions)
            {
                var fromCfg = _sm.Configure(tr.From).Permit(tr.Trigger, tr.To);

                if (tr.OnExit != null && tr.OnExit.Count > 0)
                    fromCfg.OnExitAsync(_ => RunActions(tr.OnExit));

                if (tr.OnEnter != null && tr.OnEnter.Count > 0)
                    _sm.Configure(tr.To).OnEntryAsync(_ => RunActions(tr.OnEnter));
            }

            _sm.OnTransitioned(t => { if (StateChanged != null) StateChanged(t.Destination); });
            _sm.OnTransitionCompleted(t => { if (TransitionCompleted != null) TransitionCompleted(t.Trigger); });
        }

        public System.Threading.Tasks.Task FireAsync(string trigger)
        {
            return _sm.FireAsync(trigger);
        }

        public void LoadAutoTriggers(IEnumerable<YamlAutoTrigger> autos)
        {
            if (autos == null) return;           // <— guard
            _autoTriggers.AddRange(autos);
        }

        private async System.Threading.Tasks.Task RunActions(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                IFlowAction action;
                if (!_actions.TryGet(key, out action))
                    throw new InvalidOperationException("Action not registered: " + key);

                await action.ExecuteAsync(_ctx);
            }
            await TryAutoEmitAsync();
        }

        private async System.Threading.Tasks.Task TryAutoEmitAsync()
        {
            foreach (var at in _autoTriggers)    // <— use backing list
            {
                bool allowed = at.AllowedFrom != null &&
                               at.AllowedFrom.Any(s => string.Equals(s, State, StringComparison.OrdinalIgnoreCase));

                if (allowed && Evaluate(at.When, _ctx))
                {
                    await FireAsync(at.Emit);
                    break;
                }
            }
        }

        private static bool DefaultEval(string expr, IFlowContext ctx)
        {
            switch (expr)
            {
                case "Context.FinalLabel == true": return ctx.FinalLabel == true;
                case "Context.FinalLabel == false": return ctx.FinalLabel == false;
                default: return false;
            }
        }
    }
}
