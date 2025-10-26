using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        // Track active loops and their states
        private readonly ConcurrentDictionary<string, LoopState> _activeLoops = new ConcurrentDictionary<string, LoopState>();

        // Store graph topology for loop handling
        private Dictionary<string, List<DagEdge>> _children = new Dictionary<string, List<DagEdge>>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, int> _indeg = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Node lookup by id
        private Dictionary<string, DagNode> _byId = new Dictionary<string, DagNode>(StringComparer.OrdinalIgnoreCase);

        // Ready queue exposed so loop-end can enqueue next wait-trigger
        private ConcurrentQueue<string> _ready;

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
            _byId = def.Nodes.ToDictionary(n => n.Id, StringComparer.OrdinalIgnoreCase);

            // out-edges per node
            _children = def.Connections
                           .GroupBy(c => c.From, StringComparer.OrdinalIgnoreCase)
                           .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            // in-degree (how many edges enter a node)
            _indeg = new ConcurrentDictionary<string, int>(
                def.Nodes.ToDictionary(
                    n => n.Id,
                    n => def.Connections.Count(c => string.Equals(c.To, n.Id, StringComparison.OrdinalIgnoreCase)),
                    StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase
            );
            #endregion

            foreach (var kvp in _indeg)
                Debug.WriteLine($"[DAG] Node {kvp.Key} in-degree: {kvp.Value}");

            var ready = new ConcurrentQueue<string>(
                def.Nodes.Where(n => _indeg[n.Id] == 0).Select(n => n.Id)
            );
            _ready = ready;

            Debug.WriteLine($"[DAG] Initially ready nodes: {string.Join(", ", ready)}");

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

                        // 2a. Trigger wait (optional) BEFORE executing
                        if (node.Type == "trigger")
                        {
                            var trigName = GetStringValue(node.Parameters, "triggerName", "UNKNOWN_TRIGGER");
                            await WaitTriggerAsync(trigName, node.Id, ct);
                        }

                        AutoMapContext(node);

                        // 2b. Execute by TYPE
                        await ExecuteNodeAsync(node, ct);

                        Debug.WriteLine($"[DAG] <<< EXIT  {node.Id}");

                        // 2c. Node finished → decrement children
                        done[node.Id] = true;

                        if (node.Type == "loop-start")
                        {
                            if (_children.TryGetValue(node.Id, out var edges))
                            {
                                Debug.WriteLine($"[DAG] Processing {edges.Count} children for loop-start node {node.Id}");
                                foreach (var edge in edges)
                                {
                                    var childId = edge.To;
                                    Debug.WriteLine($"[DAG] Immediately enqueuing child {childId} of loop-start node {node.Id}");
                                    // dorong ke antrian agar alur loop pertama mulai ke wait-trigger
                                    _ready.Enqueue(childId);
                                }
                            }
                        }
                        else
                        {
                            if (_children.TryGetValue(node.Id, out var edges))
                            {
                                Debug.WriteLine($"[DAG] Processing {edges.Count} children for node {node.Id}");
                                foreach (var edge in edges)
                                {
                                    string childId = edge.To;

                                    // Jika child ini adalah wait-trigger milik loop aktif, JANGAN turunkan indeg.
                                    bool isActiveLoopWaitTrigger = _activeLoops.Values.Any(ls =>
                                        !string.IsNullOrEmpty(ls.WaitTriggerId) &&
                                        string.Equals(ls.WaitTriggerId, childId, StringComparison.OrdinalIgnoreCase));

                                    if (isActiveLoopWaitTrigger)
                                    {
                                        Debug.WriteLine($"[DAG] Skip decrement for loop wait-trigger {childId}");
                                        continue;
                                    }

                                    int newVal = _indeg.AddOrUpdate(childId, 0, (_, old) => old - 1);
                                    Debug.WriteLine($"[DAG] Child {childId} in-degree → {newVal}");

                                    if (newVal == 0)
                                    {
                                        Debug.WriteLine($"[DAG] Enqueue child {childId}");
                                        _ready.Enqueue(childId);
                                    }
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"[DAG] No children found for node {node.Id}");
                            }
                        }
                    }
                    finally { throttle.Release(); }
                }

                /*------------------ 3. Main scheduler loop --------------*/
                while (done.Count < def.Nodes.Count || _activeLoops.Count > 0)
                {
                    while (_ready.TryDequeue(out var id))
                    {
                        running[id] = RunNode(_byId[id]);
                    }

                    if (running.Count > 0)
                    {
                        var finished = await Task.WhenAny(running.Values);
                        var id = running.First(kv => kv.Value == finished).Key;
                        running.TryRemove(id, out _);
                    }
                    else
                    {
                        Debug.WriteLine("[DAG] No running tasks, delaying...");
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
            try
            {
                switch (node.Type)
                {
                    case "action":
                        {
                            var key = GetStringValue(node.Parameters, "actionKey", "UnknownAction");
                            if (!_registry.TryGet(key, out var act))
                                throw new InvalidOperationException($"Action {key} not registered");

                            await act.ExecuteAsync(_ctx, ct);

                            // Jangan reset trigger kalau node ini berada di dalam loop aktif
                            bool insideActiveLoop = _activeLoops.Values
                                .Any(ls => ls.LoopNodes.Contains(node.Id, StringComparer.OrdinalIgnoreCase));

                            if (!insideActiveLoop)
                                _ctx.Trigger = null;

                            break;
                        }
                    case "condition":
                        {
                            if (node.Parameters.ContainsKey("conditions"))
                            {
                                try
                                {
                                    object conditionsObj = node.Parameters["conditions"];
                                    Dictionary<string, object> conds;

                                    if (conditionsObj is JsonElement jsonElement)
                                    {
                                        string json = jsonElement.GetRawText();
                                        conds = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                                    }
                                    else
                                    {
                                        conds = (Dictionary<string, object>)conditionsObj;
                                    }

                                    foreach (var kv in conds)
                                    {
                                        try
                                        {
                                            bool val = (bool)ResolveExpression(kv.Value.ToString());
                                            _ctx.Conditions[kv.Key] = val;
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"[DAG] Error processing condition {kv.Key} for node {node.Id}: {ex.Message}");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"[DAG] Error converting conditions for condition node {node.Id}: {ex.Message}");
                                }
                            }
                            break;
                        }
                    case "trigger":
                        {
                            var triggerName = GetStringValue(node.Parameters, "triggerName", "UNKNOWN_TRIGGER");
                            await WaitTriggerAsync(triggerName, node.Id, ct);
                            break;
                        }
                    case "loop":
                        {
                            // Legacy loop support
                            var loopId = node.Id;
                            var maxIterations = GetIntValue(node.Parameters, "maxIterations", 5);
                            var triggerName = GetStringValue(node.Parameters, "triggerName", "PLC_READ_RECEIVED");

                            var loopState = new LoopState
                            {
                                IterationCount = 0,
                                MaxIterations = maxIterations,
                                StartNodeId = loopId,
                                TriggerName = triggerName
                            };
                            IdentifyLoopNodes(loopState);
                            _activeLoops[loopId] = loopState;
                            break;
                        }
                    case "loop-start":
                        {
                            var loopId = node.Id;
                            var maxIterations = GetIntValue(node.Parameters, "maxIterations", -1); // -1 = infinite

                            var loopState = new LoopState
                            {
                                IterationCount = 0,
                                MaxIterations = maxIterations,
                                StartNodeId = loopId
                            };

                            IdentifyLoopNodes(loopState);
                            _activeLoops[loopId] = loopState;

                            // loop-start selesai instansiasi; flow lanjut
                            break;
                        }
                    case "loop-end":
                        {
                            var loopId = GetStringValue(node.Parameters, "loopRef", node.Id);

                            if (_activeLoops.TryGetValue(loopId, out var loopState))
                            {
                                loopState.IterationCount++;

                                if (loopState.MaxIterations == -1 || loopState.IterationCount < loopState.MaxIterations)
                                {
                                    // Reset in-degree dan siapkan iterasi berikutnya
                                    ResetLoopInDegrees(loopState);

                                    // Pastikan kita menunggu trigger PLC lagi:
                                    _ctx.Trigger = null;

                                    // Pastikan wait-trigger siap dijadwalkan & akan blocking sampai PLC datang
                                    if (!string.IsNullOrEmpty(loopState.WaitTriggerId))
                                    {
                                        _indeg[loopState.WaitTriggerId] = 0;
                                        _ready.Enqueue(loopState.WaitTriggerId);
                                        Debug.WriteLine($"[DAG] Loop continue → enqueue wait-trigger {loopState.WaitTriggerId}");
                                    }
                                }
                                else
                                {
                                    // Selesai loop
                                    _activeLoops.TryRemove(loopId, out _);
                                }
                            }
                            break;
                        }
                    case "delay":
                        {
                            int ms = GetIntValue(node.Parameters, "milliseconds", 1000);
                            await Task.Delay(ms, ct);
                            break;
                        }
                    default:
                        Debug.WriteLine($"[DAG] Unknown node type {node.Type} for node {node.Id}, skipping");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DAG] Error executing node {node.Id} of type {node.Type}: {ex.Message}");
                throw;
            }
        }

        /*============================================================
                            PARAMETER EXTRACTION HELPERS
        ============================================================*/
        private string GetStringValue(Dictionary<string, object> parameters, string key, string defaultValue)
        {
            if (!parameters.ContainsKey(key))
                return defaultValue;

            try
            {
                var value = parameters[key];
                if (value is JsonElement jsonElement)
                {
                    return jsonElement.GetString() ?? defaultValue;
                }
                else
                {
                    return value?.ToString() ?? defaultValue;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DAG] Error converting {key} to string: {ex.Message}, using default value {defaultValue}");
                return defaultValue;
            }
        }

        private int GetIntValue(Dictionary<string, object> parameters, string key, int defaultValue)
        {
            if (!parameters.ContainsKey(key))
                return defaultValue;

            try
            {
                var value = parameters[key];
                if (value is JsonElement jsonElement)
                {
                    return jsonElement.GetInt32();
                }
                else
                {
                    return Convert.ToInt32(value);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DAG] Error converting {key} to int: {ex.Message}, using default value {defaultValue}");
                return defaultValue;
            }
        }

        /*============================================================
                            HELPERS
        ============================================================*/
        private async Task WaitTriggerAsync(string triggerName, string waitingNodeId, CancellationToken ct)
        {
            // 1) Fast-path
            if (string.Equals(_ctx.Trigger, triggerName, StringComparison.OrdinalIgnoreCase))
                return;

            // 2) LOOP_CONTINUE_xx → JANGAN auto-continue; kita ingin menunggu PLC lagi
            // (Jadi tidak ada consumption otomatis di sini)

            // 3) Normal wait loop
            while (!string.Equals(triggerName, _ctx.Trigger, StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(50, ct);
                ct.ThrowIfCancellationRequested();
            }

            // 4) Reset trigger hanya jika node penunggu BUKAN bagian dari loop aktif
            bool belongsToActiveLoop = _activeLoops.Values.Any(ls =>
                ls.LoopNodes.Contains(waitingNodeId, StringComparer.OrdinalIgnoreCase) ||
                string.Equals(ls.WaitTriggerId, waitingNodeId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ls.TriggerName, triggerName, StringComparison.OrdinalIgnoreCase));

            if (!belongsToActiveLoop)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(50);
                    if (string.Equals(_ctx.Trigger, triggerName, StringComparison.OrdinalIgnoreCase))
                        _ctx.Trigger = null;
                });
            }
        }

        private void ParseVars(DagNode node)
        {
            if (!node.Parameters.ContainsKey("vars")) return;
            var vars = (Dictionary<string, string>)node.Parameters["vars"];
            foreach (var kvp in vars)
                _ctx.Vars[kvp.Key] = kvp.Value;
        }

        private void AutoMapContext(DagNode node)
        {
            if (!node.Parameters.ContainsKey("autoMap") ||
                !(bool)node.Parameters["autoMap"]) return;

            var map = (Dictionary<string, string>)node.Parameters["map"];
            var ctxType = typeof(IFlowContext);

            foreach (var kvp in map)
            {
                string varKey = kvp.Key;
                string ctxProp = kvp.Value;

                var prop = ctxType.GetProperty(ctxProp);
                if (prop == null) continue;

                object value = prop.GetValue(_ctx);
                _ctx.Vars[varKey] = value;
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
            // Cycle check skipped to allow loop-start/loop-end
        }

        /*------------------------------------------------------------
                            LOOP NODE DETECTION
        ------------------------------------------------------------*/
        private void IdentifyLoopNodes(LoopState loopState)
        {
            // BFS dari loop start
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>();
            queue.Enqueue(loopState.StartNodeId);
            visited.Add(loopState.StartNodeId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                loopState.LoopNodes.Add(currentId);

                // Simpan original indegree
                if (_indeg.TryGetValue(currentId, out var indeg))
                    loopState.OriginalInDegrees[currentId] = indeg;

                // Cari wait-trigger pertama di dalam loop
                if (_byId.TryGetValue(currentId, out DagNode node) && node.Type == "trigger")
                {
                    var trigName = GetStringValue(node.Parameters, "triggerName", "UNKNOWN_TRIGGER");

                    if (string.IsNullOrEmpty(loopState.WaitTriggerId))
                        loopState.WaitTriggerId = currentId;

                    if (string.IsNullOrEmpty(loopState.TriggerName))
                        loopState.TriggerName = trigName;
                }

                if (_children.TryGetValue(currentId, out var edges))
                {
                    foreach (var edge in edges)
                    {
                        var childId = edge.To;

                        if (string.Equals(childId, loopState.StartNodeId, StringComparison.OrdinalIgnoreCase))
                        {
                            // kembali ke start → loop tertutup
                            loopState.LoopNodes.Add(childId);
                            if (_indeg.TryGetValue(childId, out var indegStart))
                                loopState.OriginalInDegrees[childId] = indegStart;
                            continue;
                        }

                        if (!visited.Contains(childId))
                        {
                            visited.Add(childId);
                            queue.Enqueue(childId);
                        }
                    }
                }
            }
        }

        /*------------------------------------------------------------
                            LOOP IN-DEGREE RESET
        ------------------------------------------------------------*/
        private void ResetLoopInDegrees(LoopState loopState)
        {
            foreach (var nodeId in loopState.LoopNodes)
            {
                // Skip loop-start; yang lain direset
                if (string.Equals(nodeId, loopState.StartNodeId, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (loopState.OriginalInDegrees.TryGetValue(nodeId, out var orig))
                {
                    _indeg[nodeId] = orig;
                    Debug.WriteLine($"[DAG] Reset indegree {nodeId} → {orig}");
                }
            }

            // Pastikan wait-trigger siap dijadwalkan (indegree 0) sehingga akan blok menunggu PLC
            if (!string.IsNullOrEmpty(loopState.WaitTriggerId))
            {
                _indeg[loopState.WaitTriggerId] = 0;
                Debug.WriteLine($"[DAG] Force indegree wait-trigger {loopState.WaitTriggerId} → 0");
            }
        }
    }

    // Internal class to track loop state
    internal class LoopState
    {
        public int IterationCount { get; set; }
        public int MaxIterations { get; set; }
        public string StartNodeId { get; set; }
        public string TriggerName { get; set; }

        // Node trigger penunggu dalam loop
        public string WaitTriggerId { get; set; }

        // Store the nodes that are part of this loop for in-degree reset
        public HashSet<string> LoopNodes { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Store original in-degrees for reset
        public Dictionary<string, int> OriginalInDegrees { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }
}
