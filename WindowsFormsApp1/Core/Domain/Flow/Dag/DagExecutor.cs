using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Domain.Flow.Engine;

namespace WindowsFormsApp1.Core.Domain.Flow.Dag
{
    public sealed class DagExecutor
    {
        private readonly IActionRegistry _registry;
        private readonly IFlowContext _ctx;
        public Func<string, IFlowContext, bool> Evaluate { get; set; } = DefaultEval;

        // ----------  loop-support removed  ----------
        // private readonly ConcurrentDictionary<string, LoopState> _activeLoops = new();
        // --------------------------------------------

        private Dictionary<string, List<DagEdge>> _children =
    new Dictionary<string, List<DagEdge>>(StringComparer.OrdinalIgnoreCase);

        private ConcurrentDictionary<string, int> _indeg =
            new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, DagNode> _byId =
            new Dictionary<string, DagNode>(StringComparer.OrdinalIgnoreCase);

        private ConcurrentQueue<string> _ready;

        public DagExecutor(IActionRegistry registry, IFlowContext ctx)
        {
            _registry = registry;
            _ctx = ctx;
        }

        /*----------------------------------------------------------
                                RUN
        ----------------------------------------------------------*/
        public async Task RunAsync(DagDefinition def, CancellationToken ct = default,
                                   int maxDegree = 4)
        {
            Validate(def);

            /*  1.  build topology  */
            _byId = def.Nodes.ToDictionary(n => n.Id, StringComparer.OrdinalIgnoreCase);

            _children = def.Connections
                           .GroupBy(c => c.From, StringComparer.OrdinalIgnoreCase)
                           .ToDictionary(g => g.Key, g => g.ToList(),
                                         StringComparer.OrdinalIgnoreCase);

            _indeg = new ConcurrentDictionary<string, int>(
                def.Nodes.ToDictionary(
                    n => n.Id,
                    n => def.Connections.Count(c =>
                        string.Equals(c.To, n.Id, StringComparison.OrdinalIgnoreCase)),
                    StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in _indeg)
                Debug.WriteLine($"[DAG] Node {kvp.Key} in-degree: {kvp.Value}");

            _ready = new ConcurrentQueue<string>(
                def.Nodes.Where(n => _indeg[n.Id] == 0).Select(n => n.Id));

            Debug.WriteLine($"[DAG] Initially ready: {string.Join(", ", _ready)}");

            var done = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            var running = new ConcurrentDictionary<string, Task>(StringComparer.OrdinalIgnoreCase);

            using (var throttle = new SemaphoreSlim(Math.Max(1, maxDegree)))
            {
                /*  2.  worker per node  */
                async Task RunNode(DagNode node)
                {
                    await throttle.WaitAsync(ct);
                    try
                    {
                        Debug.WriteLine($"[DAG] >>> ENTER {node.Id}");

                        if (node.Type == "trigger")
                        {
                            var trigName = GetStringValue(node.Parameters,
                                                          "triggerName",
                                                          "UNKNOWN_TRIGGER");
                            await WaitTriggerAsync(trigName, node.Id, ct);
                        }

                        AutoMapContext(node);
                        await ExecuteNodeAsync(node, ct);

                        Debug.WriteLine($"[DAG] <<< EXIT  {node.Id}");
                        done[node.Id] = true;

                        /*  3.  decrement ren  */
                        if (_children.TryGetValue(node.Id, out var edges))
                        {
                            foreach (var edge in edges)
                            {
                                string childId = edge.To;

                                _ready.Enqueue(childId);
                                //if (done.ContainsKey(childId)) continue;

                                //int newVal = _indeg.AddOrUpdate(childId, 0, (_, old) => old - 1);
                                //Debug.WriteLine($"[DAG] Child {childId} in-degree → {newVal}");
                                //if (newVal == 0)
                                //    _ready.Enqueue(childId);
                            }
                        }
                    }
                    finally { throttle.Release(); }
                }

                /*  4.  scheduler loop  */
                while (done.Count < def.Nodes.Count)
                {
                    while (_ready.TryDequeue(out var id))
                        running[id] = RunNode(_byId[id]);

                    if (running.Count > 0)
                    {
                        var finished = await Task.WhenAny(running.Values);
                        var id = running.First(kv => kv.Value == finished).Key;
                        running.TryRemove(id, out _);
                    }
                    else
                    {
                        await Task.Delay(10, ct);
                    }
                }
            }

        }

        /*============================================================
                            EXECUTE BY NODE TYPE  (loop types removed)
        ============================================================*/
        private async Task ExecuteNodeAsync(DagNode node, CancellationToken ct)
        {
            try
            {
                switch (node.Type)
                {
                    case "action":
                        {
                            var key = GetStringValue(node.Parameters,
                                                     "actionKey",
                                                     "UnknownAction");
                            if (!_registry.TryGet(key, out var act))
                                throw new InvalidOperationException(
                                    $"Action {key} not registered");

                            await act.ExecuteAsync(_ctx, ct);

                            /*  no loop-awareness → always reset trigger  */
                            _ctx.Trigger = null;
                            break;
                        }
                    case "condition":
                        {
                            if (!node.Parameters.ContainsKey("conditions"))
                                break;

                            object o = node.Parameters["conditions"];
                            var conds = o is JsonElement je
                                ? JsonSerializer.Deserialize<Dictionary<string, object>>(
                                      je.GetRawText())
                                : (Dictionary<string, object>)o;

                            foreach (var kv in conds)
                            {
                                try
                                {
                                    bool v = (bool)ResolveExpression(kv.Value.ToString());
                                    _ctx.Conditions[kv.Key] = v;
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(
                                        $"[DAG] condition {kv.Key} error: {ex.Message}");
                                }
                            }
                            break;
                        }
                    case "trigger":
                        {
                            var name = GetStringValue(node.Parameters,
                                                      "triggerName",
                                                      "UNKNOWN_TRIGGER");
                            await WaitTriggerAsync(name, node.Id, ct);
                            break;
                        }
                    case "delay":
                        {
                            int ms = GetIntValue(node.Parameters, "milliseconds", 1000);
                            await Task.Delay(ms, ct);
                            break;
                        }
                    default:
                        Debug.WriteLine(
                            $"[DAG] Unknown node type {node.Type} for {node.Id}, skipped");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[DAG] Error executing {node.Id} ({node.Type}): {ex.Message}");
                throw;
            }
        }

        /*============================================================
                            PARAMETER HELPERS
        ============================================================*/
        private string GetStringValue(Dictionary<string, object> p,
                                      string key,
                                      string defVal)
        {
            if (!p.ContainsKey(key)) return defVal;
            try
            {
                var v = p[key];
                return v is JsonElement je ? je.GetString() ?? defVal
                                           : v?.ToString() ?? defVal;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[DAG] string convert error on {key}: {ex.Message}");
                return defVal;
            }
        }

        private int GetIntValue(Dictionary<string, object> p,
                                string key,
                                int defVal)
        {
            if (!p.ContainsKey(key)) return defVal;
            try
            {
                var v = p[key];
                return v is JsonElement je ? je.GetInt32()
                                           : Convert.ToInt32(v);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[DAG] int convert error on {key}: {ex.Message}");
                return defVal;
            }
        }

        /*============================================================
                            WAIT TRIGGER  (simplified)
        ============================================================*/
        private async Task WaitTriggerAsync(string triggerName,
                                            string waitingNodeId,
                                            CancellationToken ct)
        {
            // fast path
            if (string.Equals(_ctx.Trigger, triggerName,
                              StringComparison.OrdinalIgnoreCase))
                return;

            while (!string.Equals(_ctx.Trigger, triggerName,
                                  StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(50, ct);
                ct.ThrowIfCancellationRequested();
            }

            /*  no loop-awareness → always consume trigger  */
            _ = Task.Run(async () =>
            {
                await Task.Delay(50);
                if (string.Equals(_ctx.Trigger, triggerName,
                                  StringComparison.OrdinalIgnoreCase))
                    _ctx.Trigger = null;
            });
        }

        private void AutoMapContext(DagNode node)
        {
            if (!node.Parameters.ContainsKey("autoMap") ||
                !(bool)node.Parameters["autoMap"]) return;

            var map = (Dictionary<string, string>)node.Parameters["map"];
            var ctxType = typeof(IFlowContext);

            foreach (var kv in map)
            {
                var prop = ctxType.GetProperty(kv.Value);
                if (prop == null) continue;
                _ctx.Vars[kv.Key] = prop.GetValue(_ctx);
            }
        }

        private static bool DefaultEval(string expr, IFlowContext ctx)
        {
            switch (expr)
            {
                case "Context.FinalLabel == true":
                    return ctx.FinalLabel == true;

                case "Context.FinalLabel == false":
                    return ctx.FinalLabel == false;

                default:
                    if (expr.StartsWith("Context.Conditions"))
                        return (bool)ResolveExpression(expr);
                    return false;
            }
        }

        private static object ResolveExpression(string expr)
            => throw new NotImplementedException("Plug your evaluator");

        /*------------------------------------------------------------
                            VALIDATION
        ------------------------------------------------------------*/
        private static void Validate(DagDefinition def)
        {
            var ids = def.Nodes.Select(n => n.Id)
                               .ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var c in def.Connections)
            {
                if (!ids.Contains(c.From))
                    throw new InvalidOperationException(
                        $"Edge from unknown {c.From}");
                if (!ids.Contains(c.To))
                    throw new InvalidOperationException(
                        $"Edge to unknown {c.To}");
            }
            /*  cycle check skipped – cycles are allowed  */
        }
    }
}