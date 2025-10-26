using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services.StateMachine
{
    public class FlowEngine
    {
        private readonly StateMachine<string, string> _machine;
        private readonly FlowConfig _config;
        private readonly object _nodeInstance; // instance class yang punya method

        public string CurrentState => _machine.State;

        // ctor baru – terima instance node (bisa DI resolve nanti)
        public FlowEngine(FlowConfig config, object nodeInstance)
        {
            _config = config;
            _nodeInstance = nodeInstance ?? throw new ArgumentNullException(nameof(nodeInstance));
            _machine = new StateMachine<string, string>(config.States.First());

            // 1. Pasang SEMUA YAML : OnEntry + InternalTransition + From→To + Guard
            ApplyYaml(_machine, _nodeInstance, _config);

            // 2. Log transisi (opsional)
            _machine.OnTransitioned(t =>
                Console.WriteLine($"[STATE] {t.Source} -> {t.Destination} via {t.Trigger}"));
        }

        public void Fire(string trigger)
        {
            try { _machine.Fire(trigger); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        #region ---------- YamlHelper (dinamis) ----------
        private static void ApplyYaml(StateMachine<string, string> machine,
                                      object nodeInstance,
                                      FlowConfig cfg)
        {
            // 1. OnEntry
            foreach (string state in cfg.States)
            {
                var stateCfg = machine.Configure(state);
                if (cfg.OnEntry != null && cfg.OnEntry.TryGetValue(state, out var onEntry))
                    foreach (string act in onEntry)
                        stateCfg.OnEntry(() => Invoke(nodeInstance, act));
            }

            // 2. InternalTransition
            //foreach (string state in cfg.States)
            //{
            //    var stateCfg = machine.Configure(state);
            //    if (cfg.InternalTransition != null &&
            //        cfg.InternalTransition.TryGetValue(state, out var internals))
            //        foreach (var it in internals)
            //            stateCfg.InternalTransition(it.Trigger,
            //                _ => Invoke(nodeInstance, it.Method));
            //}

            // 3. From → To (dengan guard jika ada)
            foreach (var t in cfg.Transitions)
            {
                var fromCfg = machine.Configure(t.From);

                if (string.IsNullOrEmpty(t.Guard))
                {
                    // tanpa guard
                    fromCfg.Permit(t.Trigger, t.To);
                }
                else
                {
                    // dengan guard → PermitIf
                    fromCfg.PermitIf(t.Trigger, t.To,
                        () => EvaluateGuard(nodeInstance, t.Guard));
                }
            }
        }

        private static void Invoke(object instance, string methodName)
        {
            MethodInfo mi = instance.GetType().GetMethod(methodName,
                              BindingFlags.Public | BindingFlags.Instance,
                              null, Type.EmptyTypes, null);
            if (mi == null)
                throw new MissingMethodException(methodName);
            mi.Invoke(instance, null);
        }

        private static bool EvaluateGuard(object instance, string guard)
        {
            // contoh sederhana: True / False
            if (bool.TryParse(guard, out bool b)) return b;

            // atau panggil method bool di instance
            MethodInfo mi = instance.GetType().GetMethod(guard,
                              BindingFlags.Public | BindingFlags.Instance,
                              null, Type.EmptyTypes, null);
            if (mi != null && mi.ReturnType == typeof(bool))
                return (bool)mi.Invoke(instance, null);

            throw new NotSupportedException($"Guard '{guard}' tidak dikenal");
        }
        #endregion
    }
}