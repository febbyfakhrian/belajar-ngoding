﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Domain.Flow.Engine;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApp1.Core.Domain.Flow.Dag
{
    public sealed class DagExecutor
    {
        private readonly IActionRegistry _registry;
        private readonly IFlowContext _ctx;
        public Func<string, IFlowContext, bool> Evaluate { get; set; } = DefaultEval;

        // Support for split-batches node type
        private readonly ConcurrentDictionary<string, SplitBatchState> _activeSplitBatches =
    new ConcurrentDictionary<string, SplitBatchState>();

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
            Debug.WriteLine("[DAG] DagExecutor created");
        }

        /*----------------------------------------------------------
                                RUN
        ----------------------------------------------------------*/
        public async Task RunAsync(DagDefinition def, CancellationToken ct = default,
                                   int maxDegree = 4)
        {
            Debug.WriteLine($"[DAG] RunAsync started with {def.Nodes.Count} nodes and {def.Connections.Count} connections");
            Validate(def);

            /*  1.  build topology  */
            // Build _byId dictionary safely to avoid duplicate key issues
            _byId = new Dictionary<string, DagNode>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in def.Nodes)
            {
                if (!string.IsNullOrEmpty(node.Id) && !_byId.ContainsKey(node.Id))
                {
                    _byId[node.Id] = node;
                    Debug.WriteLine($"[DAG] Added node to _byId: '{node.Id}' (Name: '{node.Name}', Type: '{node.Type}')");
                }
                else if (string.IsNullOrEmpty(node.Id))
                {
                    Debug.WriteLine($"[DAG] Warning: Skipping node with empty ID (Name: '{node.Name}')");
                }
                else
                {
                    Debug.WriteLine($"[DAG] Warning: Duplicate node ID '{node.Id}' found, using first occurrence");
                }
            }

            // Group connections by From node, handling potential duplicates
            _children = new Dictionary<string, List<DagEdge>>(StringComparer.OrdinalIgnoreCase);
            foreach (var connection in def.Connections)
            {
                if (!string.IsNullOrEmpty(connection.From))
                {
                    if (!_children.ContainsKey(connection.From))
                    {
                        _children[connection.From] = new List<DagEdge>();
                    }
                    _children[connection.From].Add(connection);
                    Debug.WriteLine($"[DAG] Added connection to _children: '{connection.From}' -> '{connection.To}'");
                }
                else
                {
                    Debug.WriteLine($"[DAG] Warning: Skipping connection with empty From field -> '{connection.To}'");
                }
            }

            // Build in-degree dictionary - Fixed to avoid duplicate key issues
            _indeg = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            
            // Initialize all nodes with 0 in-degree
            foreach (var node in def.Nodes)
            {
                if (!string.IsNullOrEmpty(node.Id))
                {
                    _indeg.TryAdd(node.Id, 0);
                    Debug.WriteLine($"[DAG] Initialized in-degree for node '{node.Id}': 0");
                }
            }
            
            // Count incoming edges for each node
            foreach (var connection in def.Connections)
            {
                if (!string.IsNullOrEmpty(connection.To) && _indeg.ContainsKey(connection.To))
                {
                    _indeg[connection.To]++;
                    Debug.WriteLine($"[DAG] Incremented in-degree for node '{connection.To}': {_indeg[connection.To]}");
                }
            }

            foreach (var kvp in _indeg)
                Debug.WriteLine($"[DAG] Node {kvp.Key} in-degree: {kvp.Value}");

            _ready = new ConcurrentQueue<string>();
            foreach (var node in def.Nodes)
            {
                if (!string.IsNullOrEmpty(node.Id) && _indeg.ContainsKey(node.Id) && _indeg[node.Id] == 0)
                {
                    _ready.Enqueue(node.Id);
                    Debug.WriteLine($"[DAG] Added node '{node.Id}' to ready queue (in-degree: 0)");
                }
            }

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
                        Debug.WriteLine($"[DAG] >>> ENTER {node.Id} (Name: {node.Name}, Type: {node.Type})");

                        if (node.Type == "trigger")
                        {
                            var trigName = GetStringValue(node.Parameters,
                                                          "triggerName",
                                                          "UNKNOWN_TRIGGER");
                            Debug.WriteLine($"[DAG] Waiting for trigger: {trigName}");
                            await WaitTriggerAsync(trigName, node.Id, ct);
                        }

                        AutoMapContext(node);
                        await ExecuteNodeAsync(node, ct);

                        Debug.WriteLine($"[DAG] <<< EXIT  {node.Id}");
                        done[node.Id] = true;

                        /*  3.  decrement ren  */
                        if (_children.TryGetValue(node.Id, out var edges))
                        {
                            // Filter edges based on split-batches logic if applicable
                            var filteredEdges = edges;
                            
                            // Special handling for split-batches nodes
                            if (node.Type == "split-batches")
                            {
                                // Increment the batch counter when the node is executed
                                if (_activeSplitBatches.TryGetValue(node.Id, out var state))
                                {
                                    // Increment the batch counter
                                    state.CurrentBatch++;
                                    
                                    // Determine which path to take based on current batch count
                                    string requiredKey = (state.CurrentBatch >= state.BatchSize) ? "done" : "main";
                                    
                                    // Filter edges to only include those with the required key
                                    filteredEdges = edges.Where(e => e.Key == requiredKey).ToList();
                                    
                                    Debug.WriteLine($"[DAG] Split-batches node {node.Id} taking '{requiredKey}' path (batch {state.CurrentBatch}/{state.BatchSize})");
                                }
                            }
                            
                            foreach (var edge in filteredEdges)
                            {
                                string childId = edge.To;
                                Debug.WriteLine($"[DAG] Enqueuing child node: {childId} via '{edge.Key}' path");
                                _ready.Enqueue(childId);
                            }
                        }
                    }
                    finally { throttle.Release(); }
                }

                /*  4.  scheduler loop  */
                while (done.Count < def.Nodes.Count)
                {
                    while (_ready.TryDequeue(out var id))
                    {
                        if (_byId.TryGetValue(id, out var node))
                        {
                            running[id] = RunNode(node);
                            Debug.WriteLine($"[DAG] Started running node: {id}");
                        }
                        else
                        {
                            Debug.WriteLine($"[DAG] Warning: Could not find node with ID '{id}' in _byId dictionary");
                        }
                    }

                    if (running.Count > 0)
                    {
                        var finished = await Task.WhenAny(running.Values);
                        var id = running.First(kv => kv.Value == finished).Key;
                        running.TryRemove(id, out _);
                        Debug.WriteLine($"[DAG] Node finished: {id}");
                    }
                    else
                    {
                        await Task.Delay(10, ct);
                    }
                }
            }
            Debug.WriteLine("[DAG] RunAsync completed");

        }

        /*============================================================
                            EXECUTE BY NODE TYPE
        ============================================================*/
        private async Task ExecuteNodeAsync(DagNode node, CancellationToken ct)
        {
            try
            {
                Debug.WriteLine($"[DAG] ExecuteNodeAsync for node {node.Id} (Type: {node.Type})");
                switch (node.Type)
                {
                    case "action":
                        {
                            var key = GetStringValue(node.Parameters,
                                                     "actionKey",
                                                     "UnknownAction");
                            Debug.WriteLine($"[DAG] Executing action with key: {key}");
                            if (!_registry.TryGet(key, out var act))
                            {
                                Debug.WriteLine($"[DAG] ERROR: Action {key} not registered");
                                throw new InvalidOperationException(
                                    $"Action {key} not registered");
                            }

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
                            var conds = o is Newtonsoft.Json.Linq.JToken token ? token.ToObject<Dictionary<string, object>>() : (Dictionary<string, object>)o;

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
                            Debug.WriteLine($"[DAG] Processing trigger node with name: {name}");
                            await WaitTriggerAsync(name, node.Id, ct);
                            break;
                        }
                    case "delay":
                        {
                            int ms = GetIntValue(node.Parameters, "milliseconds", 1000);
                            Debug.WriteLine($"[DAG] Delaying for {ms} ms");
                            await Task.Delay(ms, ct);
                            break;
                        }
                    case "split-batches":
                        {
                            // Handle split-batches node type
                            int batchSize = GetIntValue(node.Parameters, "batchSize", 1);
                            
                            // Get or create the state for this split-batches node
                            var state = _activeSplitBatches.GetOrAdd(node.Id, new SplitBatchState { BatchSize = batchSize, CurrentBatch = 0 });
                            
                            // Log the current state
                            Debug.WriteLine($"[DAG] Split-batches node {node.Id} has batch counter at {state.CurrentBatch} of {state.BatchSize}");
                            
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
                return v is Newtonsoft.Json.Linq.JToken tok ? tok.Value<string>() ?? defVal
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
                return v is Newtonsoft.Json.Linq.JToken token ? token.ToObject<int>(): Convert.ToInt32(v);
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
            Debug.WriteLine($"[DAG] WaitTriggerAsync for '{triggerName}' on node '{waitingNodeId}'");
            // fast path
            if (string.Equals(_ctx.Trigger, triggerName,
                              StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"[DAG] Trigger '{triggerName}' already set, continuing immediately");
                return;
            }

            Debug.WriteLine($"[DAG] Waiting for trigger '{triggerName}'...");
            while (!string.Equals(_ctx.Trigger, triggerName,
                                  StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(50, ct);
                ct.ThrowIfCancellationRequested();
            }
            Debug.WriteLine($"[DAG] Trigger '{triggerName}' received");

            /*  no loop-awareness → always consume trigger  */
            _ = Task.Run(async () =>
            {
                await Task.Delay(50);
                if (string.Equals(_ctx.Trigger, triggerName,
                                  StringComparison.OrdinalIgnoreCase))
                {
                    _ctx.Trigger = null;
                    Debug.WriteLine($"[DAG] Consumed trigger '{triggerName}'");
                }
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
            Debug.WriteLine($"[DAG] Validating DAG with {def.Nodes.Count} nodes and {def.Connections.Count} connections");
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in def.Nodes)
            {
                if (!string.IsNullOrEmpty(node.Id))
                {
                    ids.Add(node.Id);
                }
            }
                               
            // Log all node IDs for debugging
            Debug.WriteLine("[DAG] Valid node IDs:");
            foreach (var id in ids)
            {
                Debug.WriteLine($"  '{id}'");
            }
            
            Debug.WriteLine($"[DAG] Checking {def.Connections.Count} connections...");
            
            foreach (var c in def.Connections)
            {
                Debug.WriteLine($"[DAG] Checking connection: '{c.From}' -> '{c.To}'");
                
                if (string.IsNullOrEmpty(c.From))
                {
                    Debug.WriteLine($"[DAG] ERROR: Edge from is null or empty");
                    throw new InvalidOperationException("Edge from is null or empty");
                }
                
                if (string.IsNullOrEmpty(c.To))
                {
                    Debug.WriteLine($"[DAG] ERROR: Edge to is null or empty");
                    throw new InvalidOperationException("Edge to is null or empty");
                }
                
                if (!ids.Contains(c.From))
                {
                    Debug.WriteLine($"[DAG] ERROR: Edge from unknown node ID '{c.From}'");
                    // Let's also check if this might be a node name
                    var nodeNameMatch = def.Nodes.FirstOrDefault(n => n.Name.Equals(c.From, StringComparison.OrdinalIgnoreCase));
                    if (nodeNameMatch != null)
                    {
                        Debug.WriteLine($"[DAG] NOTE: Found node with name '{c.From}', but expected ID '{nodeNameMatch.Id}'");
                    }
                    throw new InvalidOperationException(
                        $"Edge from unknown node ID '{c.From}'");
                }
                if (!ids.Contains(c.To))
                {
                    Debug.WriteLine($"[DAG] ERROR: Edge to unknown node ID '{c.To}'");
                    // Let's also check if this might be a node name
                    var nodeNameMatch = def.Nodes.FirstOrDefault(n => n.Name.Equals(c.To, StringComparison.OrdinalIgnoreCase));
                    if (nodeNameMatch != null)
                    {
                        Debug.WriteLine($"[DAG] NOTE: Found node with name '{c.To}', but expected ID '{nodeNameMatch.Id}'");
                    }
                    throw new InvalidOperationException(
                        $"Edge to unknown node ID '{c.To}'");
                }
            }
            Debug.WriteLine("[DAG] All connections validated successfully");
            /*  cycle check skipped – cycles are allowed  */
        }
        
        // State class for split-batches nodes
        private class SplitBatchState
        {
            public int BatchSize { get; set; }
            public int CurrentBatch { get; set; }
        }
    }
}