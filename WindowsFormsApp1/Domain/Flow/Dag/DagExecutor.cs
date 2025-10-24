using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Domain.Flow.Engine;

namespace WindowsFormsApp1.Domain.Flow.Dag
{
    public sealed class DagExecutor
    {
        private readonly IActionRegistry _registry;
        private readonly IFlowContext _ctx;
        public Func<string, IFlowContext, bool> Evaluate { get; set; } = DefaultEval;

        public DagExecutor(IActionRegistry registry, IFlowContext ctx)
        {
            _registry = registry;
            _ctx = ctx;
        }

        /*----------------------------------------------------------
                                RUN  (n8n style)
        ----------------------------------------------------------*/
        public async Task RunAsync(DagDefinition def, CancellationToken ct = default, int maxDegree = 4)
        {
            Validate(def);

            #region 1. Graph topology from EDGES
            var byId = def.Nodes.ToDictionary(n => n.Id, StringComparer.OrdinalIgnoreCase);

            // out-edges per node
            var children = def.Connections
                              .GroupBy(c => c.From)
                              .ToDictionary(g => g.Key, g => g.ToList());

            // in-degree (how many edges enter a node)
            var indeg = def.Nodes.ToDictionary(n => n.Id,
                                              n => def.Connections.Count(c => c.To == n.Id),
                                              StringComparer.OrdinalIgnoreCase);
            #endregion

            var ready = new ConcurrentQueue<string>(
                            def.Nodes.Where(n => indeg[n.Id] == 0).Select(n => n.Id));

            var done = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            var running = new ConcurrentDictionary<string, Task>(StringComparer.OrdinalIgnoreCase);

            using (var throttle = new SemaphoreSlim(Math.Max(1, maxDegree)))
            {
                /*------------------ 2. Worker per node ------------------*/
                async Task RunNode(DagNode node)
                {
                    await throttle.WaitAsync(ct);
                    try
                    {
                        Debug.WriteLine($"[DAG] >>> ENTER {node.Id}");

                        // 2a. Trigger wait (optional)
                        if (node.Type == "trigger")
                        {
                            var trigName = node.Parameters["triggerName"].ToString();
                            await WaitTriggerAsync(trigName, ct);
                        }

                        // Parsing vars dari node saat ini
                        //ParseVars(node);

                        AutoMapContext(node);


                        // 2b. Execute by TYPE
                        await ExecuteNodeAsync(node, ct);

                        Debug.WriteLine($"[DAG] <<< EXIT  {node.Id}");

                        // 2c. Node finished → decrement children
                        done[node.Id] = true;
                        if (children.TryGetValue(node.Id, out var edges))
                            foreach (var edge in children[node.Id])   // edge bertipe DagEdge
                            {
                                string childId = edge.To;             // ← string
                                int oldVal = indeg[childId];
                                int newVal = Interlocked.Decrement(ref oldVal);
                                indeg[childId] = newVal;
                                if (newVal == 0) ready.Enqueue(childId);
                            }
                    }
                    finally { throttle.Release(); }
                }

                /*------------------ 3. Main scheduler loop --------------*/
                while (done.Count < def.Nodes.Count)
                {
                    while (ready.TryDequeue(out var id))
                        running[id] = RunNode(byId[id]);

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
                            EXECUTE BY NODE TYPE
        ============================================================*/
        private async Task ExecuteNodeAsync(DagNode node, CancellationToken ct)
        {
            switch (node.Type)
            {
                case "action":
                    {
                        var key = node.Parameters["actionKey"].ToString();
                        if (!_registry.TryGet(key, out var act))
                            throw new InvalidOperationException($"Action {key} not registered");
                        await act.ExecuteAsync(_ctx, ct);
                        break;
                    }
                case "condition":
                    {
                        var conds = (Dictionary<string, object>)node.Parameters["conditions"];
                        foreach (var kv in conds)
                        {
                            bool val = (bool)ResolveExpression(kv.Value.ToString());
                            _ctx.Conditions[kv.Key] = val;
                        }
                        break;
                    }
                case "trigger":
                    await WaitTriggerAsync(node.Parameters["triggerName"].ToString(), ct);
                    break;

                case "delay":
                    {
                        int ms = Convert.ToInt32(node.Parameters["milliseconds"]);
                        await Task.Delay(ms, ct);
                        break;
                    }

                // tambah type baru di sini (webhook, email, dll)
                default:
                    throw new NotSupportedException($"Unknown node type {node.Type}");
            }
        }

        /*============================================================
                            HELPERS
        ============================================================*/
        private async Task WaitTriggerAsync(string triggerName, CancellationToken ct)
        {
            while (triggerName != _ctx.Trigger)
            {
                await Task.Delay(50, ct);
                ct.ThrowIfCancellationRequested();
            }
        }

        private void ParseVars(DagNode node)
        {
            if (!node.Parameters.ContainsKey("vars")) return;
            var vars = (Dictionary<string, string>)node.Parameters["vars"];
            foreach (var kvp in vars)
                _ctx.Vars[kvp.Key] = kvp.Value;   // tambah / update
        }

        private void AutoMapContext(DagNode node)
        {
            if (!node.Parameters.ContainsKey("autoMap") ||
                !(bool)node.Parameters["autoMap"]) return;

            var map = (Dictionary<string, string>)node.Parameters["map"];
            var ctxType = typeof(IFlowContext);

            foreach (var kvp in map)
            {
                string varKey = kvp.Key;           // key luar (bebas)
                string ctxProp = kvp.Value;        // nama property di IFlowContext

                var prop = ctxType.GetProperty(ctxProp);
                if (prop == null) continue;

                object value = prop.GetValue(_ctx);
                _ctx.Vars[varKey] = value;         // masukkan ke Vars
            }
        }

        private bool EvaluateEdge(DagEdge edge)
        {
            if (string.IsNullOrEmpty(edge.When)) return true;
            return Evaluate(edge.When, _ctx);
        }

        private static bool DefaultEval(string expr, IFlowContext ctx)
        {
            switch (expr)
            {
                case "Context.FinalLabel == true": return ctx.FinalLabel == true;
                case "Context.FinalLabel == false": return ctx.FinalLabel == false;
                default:
                    // contoh sederhana – bisa pakai DynamicExpresso dsb
                    if (expr.StartsWith("Context.Conditions"))
                        return (bool)ResolveExpression(expr);
                    return false;
            }
        }

        private static object ResolveExpression(string expr) => throw new NotImplementedException("Plug your evaluator");

        /*------------------------------------------------------------
                            VALIDATION
        ------------------------------------------------------------*/
        private static void Validate(DagDefinition def)
        {
            var ids = def.Nodes.Select(n => n.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var c in def.Connections)
            {
                if (!ids.Contains(c.From)) throw new InvalidOperationException($"Edge from unknown {c.From}");
                if (!ids.Contains(c.To)) throw new InvalidOperationException($"Edge to unknown {c.To}");
            }

            // quick cycle check (DFS)
            var temp = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var perm = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            bool Visit(string id)
            {
                if (perm.Contains(id)) return true;
                if (!temp.Add(id)) return false;
                foreach (var e in def.Connections.Where(x => x.From == id))
                    if (!Visit(e.To)) return false;
                temp.Remove(id); perm.Add(id);
                return true;
            }
            foreach (var n in def.Nodes) if (!Visit(n.Id)) throw new InvalidOperationException("Cycle detected");
        }
    }
}