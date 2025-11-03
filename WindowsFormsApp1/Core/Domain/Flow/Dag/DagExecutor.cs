﻿using System;
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

                        // Handle trigger nodes directly to avoid duplication with ExecuteNodeAsync
                        if (node.Type == "trigger")
                        {
                            var trigName = GetStringValue(node.Parameters,
                                                          "triggerName",
                                                          "UNKNOWN_TRIGGER");
                            Debug.WriteLine($"[DAG] Waiting for trigger: {trigName}");
                            await WaitTriggerAsync(trigName, node.Id, ct);
                            Debug.WriteLine($"[DAG] Trigger {trigName} received for node {node.Id}");
                        }
                        else
                        {
                            // AutoMap and execute the node for non-trigger nodes
                            AutoMapContext(node);
                            await ExecuteNodeAsync(node, ct);
                        }

                        Debug.WriteLine($"[DAG] <<< EXIT  {node.Id}");
                        done[node.Id] = true;
                        Debug.WriteLine($"[DAG] Node {node.Id} marked as done. Total completed: {done.Count}/{def.Nodes.Count}");

                        /*  3.  decrement ren  */
                        if (_children.TryGetValue(node.Id, out var edges))
                        {
                            // Filter edges based on split-batches logic if applicable
                            var filteredEdges = edges;
                            
                            // Special handling for split-batches nodes
                            if (node.Type == "split-batches")
                            {
                                // Get or create the state for this split-batches node
                                // Only initialize if it doesn't exist yet
                                int batchSize = GetIntValue(node.Parameters, "batchSize", 1);
                                bool isUnlimited = batchSize <= 0;
                                var state = _activeSplitBatches.GetOrAdd(node.Id, id => new SplitBatchState { BatchSize = batchSize, CurrentBatch = 0 });
                                
                                // Increment the batch counter
                                state.CurrentBatch++;

                                // Determine which path to take based on current batch count
                                // For batchSize=4, we want to execute 4 times, then take "done" path
                                // So we take "main" path when CurrentBatch < BatchSize
                                string requiredKey = (isUnlimited || state.CurrentBatch < batchSize) ? "main" : "done";

                                // Filter edges to only include those with the required key
                                filteredEdges = edges.Where(e => e.Key == requiredKey).ToList();
                                
                                Debug.WriteLine($"[DAG] Split-batches node {node.Id} taking '{requiredKey}' path (batch {state.CurrentBatch}/{state.BatchSize})");
                            }
                            
                            // Evaluate conditional expressions on edges
                            var executableEdges = new List<DagEdge>();
                            foreach (var edge in filteredEdges)
                            {
                                // If there's no condition, or if the condition evaluates to true, add the edge
                                if (string.IsNullOrEmpty(edge.When) || EvaluateCondition(edge.When))
                                {
                                    executableEdges.Add(edge);
                                }
                                else
                                {
                                    Debug.WriteLine($"[DAG] Skipping edge from {node.Id} to {edge.To} because condition '{edge.When}' evaluated to false");
                                }
                            }
                            
                            foreach (var edge in executableEdges)
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
                                // Simpan status gagal
                                _ctx.Vars[$"node_{node.Id}_executed"] = true;
                                _ctx.Vars[$"node_{node.Id}_success"] = false;
                                throw new InvalidOperationException(
                                    $"Action {key} not registered");
                            }

                            await act.ExecuteAsync(_ctx, ct);
                            
                                // Simpan status sukses
                            _ctx.Vars[$"node_{node.Id}_executed"] = true;
                            _ctx.Vars[$"node_{node.Id}_success"] = true;

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
                            // Trigger nodes are handled directly in RunNode to avoid duplication
                            // This case should never be reached, but included for completeness
                            var name = GetStringValue(node.Parameters,
                                                      "triggerName",
                                                      "UNKNOWN_TRIGGER");
                            Debug.WriteLine($"[DAG] Processing trigger node with name: {name} (should not happen)");
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
                            // State is now managed in the RunNode method
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
            // fast path - check if trigger is already set
            lock (_ctx)
            {
                if (string.Equals(_ctx.Trigger, triggerName,
                                  StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"[DAG] Fast path: Trigger '{triggerName}' already set for node {waitingNodeId}");
                    _ctx.Trigger = null; // Consume the trigger immediately
                    return;
                }
            }

            Debug.WriteLine($"[DAG] Node {waitingNodeId} waiting for trigger '{triggerName}'...");
            
            while (true)
            {
                // Check if this node should consume the trigger
                bool shouldConsume = false;
                lock (_ctx)
                {
                    if (string.Equals(_ctx.Trigger, triggerName,
                                      StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine($"[DAG] Trigger '{triggerName}' received by node {waitingNodeId}");
                        shouldConsume = true;
                        _ctx.Trigger = null; // Consume the trigger immediately
                    }
                }
                
                if (shouldConsume)
                {
                    return; // Trigger consumed, exit
                }
                
                await Task.Delay(50, ct);
                ct.ThrowIfCancellationRequested();
            }
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

        private bool EvaluateCondition(string expr)
        {
            try
            {
                Debug.WriteLine($"[DAG] Evaluating condition: {expr}");
                // Use the existing Evaluate function if available, otherwise use DefaultEval
                bool result = Evaluate != null ? Evaluate(expr, _ctx) : DefaultEval(expr, _ctx);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DAG] Error evaluating condition '{expr}': {ex.Message}");
                return false; // Default to false if evaluation fails
            }
        }

        private static bool DefaultEval(string expr, IFlowContext ctx)
        {
            // Handle equality expressions
            if (expr.Contains(" == "))
            {
                var parts = expr.Split(new string[] { " == " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    var left = parts[0].Trim();
                    var right = parts[1].Trim();
                    
                    // Evaluate left side
                    bool leftValue = EvaluateExpressionPart(left, ctx);
                    
                    // Evaluate right side
                    bool rightValue = false;
                    if (right == "true")
                        rightValue = true;
                    else if (right == "false")
                        rightValue = false;
                    else
                        rightValue = EvaluateExpressionPart(right, ctx);
                    
                    return leftValue == rightValue;
                }
            }
            
            // Handle simple expressions (backward compatibility)
            switch (expr)
            {
                case "Context.FinalLabel == true":
                    // Handle nullable boolean properly - default to false if null
                    Debug.WriteLine($"[DAG] Evaluating 'Context.FinalLabel == true' (backward compatibility) - FinalLabel value: {ctx.FinalLabel?.ToString() ?? "null"}");
                    // Use the same logic as the general equality expression parser for consistency
                    bool resultTrue = (ctx.FinalLabel ?? false) == true;
                    Debug.WriteLine($"[DAG] Result for 'Context.FinalLabel == true': {resultTrue}");
                    return resultTrue;

                case "Context.FinalLabel == false":
                    // Handle nullable boolean properly - only true when explicitly false
                    Debug.WriteLine($"[DAG] Evaluating 'Context.FinalLabel == false' (backward compatibility) - FinalLabel value: {ctx.FinalLabel?.ToString() ?? "null"}");
                    // Use the same logic as the general equality expression parser for consistency
                    bool resultFalse = (ctx.FinalLabel ?? false) == false;
                    Debug.WriteLine($"[DAG] Result for 'Context.FinalLabel == false': {resultFalse}");
                    return resultFalse;

                default:
                    // Handle Context.Conditions expressions
                    if (expr.StartsWith("Context.Conditions."))
                    {
                        // Extract condition name (e.g., "Context.Conditions.loopSuccess" -> "loopSuccess")
                        var conditionName = expr.Substring("Context.Conditions.".Length);
                        
                        // Check if the condition exists and return its value
                        if (ctx.Conditions.TryGetValue(conditionName, out bool value))
                        {
                            return value;
                        }
                        return false;
                    }
                    
                    // Handle Context.Vars expressions
                    if (expr.StartsWith("Context.Vars."))
                    {
                        // Extract variable name (e.g., "Context.Vars.node_123_success" -> "node_123_success")
                        var varName = expr.Substring("Context.Vars.".Length);
                        
                        // Check if the variable exists and return its value
                        if (ctx.Vars.TryGetValue(varName, out object value))
                        {
                            if (value is bool boolValue)
                            {
                                return boolValue;
                            }
                            else if (value is string stringValue)
                            {
                                return bool.TryParse(stringValue, out bool result) ? result : false;
                            }
                            else
                            {
                                return Convert.ToBoolean(value);
                            }
                        }
                        return false;
                    }
                    
                    return false;
            }
        }
        
        private static bool EvaluateExpressionPart(string expr, IFlowContext ctx)
        {
            // Handle Context.FinalLabel expressions
            if (expr == "Context.FinalLabel")
            {
                // Return the value of FinalLabel, defaulting to false if null
                bool finalLabelValue = ctx.FinalLabel ?? false;
                Debug.WriteLine($"[DAG] Context.FinalLabel expression evaluated to: {finalLabelValue} (actual FinalLabel: {ctx.FinalLabel?.ToString() ?? "null"})");
                return finalLabelValue;
            }
            
            // Handle Context.Conditions expressions
            if (expr.StartsWith("Context.Conditions."))
            {
                // Extract condition name (e.g., "Context.Conditions.loopSuccess" -> "loopSuccess")
                var conditionName = expr.Substring("Context.Conditions.".Length);
                
                // Check if the condition exists and return its value
                if (ctx.Conditions.TryGetValue(conditionName, out bool value))
                {
                    return value;
                }
                return false;
            }
            
            // Handle Context.Vars expressions
            if (expr.StartsWith("Context.Vars."))
            {
                // Extract variable name (e.g., "Context.Vars.node_123_success" -> "node_123_success")
                var varName = expr.Substring("Context.Vars.".Length);
                
                // Check if the variable exists and return its value
                if (ctx.Vars.TryGetValue(varName, out object value))
                {
                    if (value is bool boolValue)
                    {
                        return boolValue;
                    }
                    else if (value is string stringValue)
                    {
                        return bool.TryParse(stringValue, out bool result) ? result : false;
                    }
                    else
                    {
                        return Convert.ToBoolean(value);
                    }
                }
                return false;
            }
            
            // Handle boolean literals
            if (expr == "true")
                return true;
            if (expr == "false")
                return false;
                
            return false;
        }

        private static object ResolveExpression(string expr)
            => throw new NotImplementedException("Plug your evaluator");

        /*------------------------------------------------------------
                            VALIDATION
        ------------------------------------------------------------*/
        private static void Validate(DagDefinition def)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in def.Nodes)
            {
                if (!string.IsNullOrEmpty(node.Id))
                {
                    ids.Add(node.Id);
                }
            }
                               
            foreach (var c in def.Connections)
            {
                if (string.IsNullOrEmpty(c.From))
                {
                    throw new InvalidOperationException("Edge from is null or empty");
                }
                
                if (string.IsNullOrEmpty(c.To))
                {
                    throw new InvalidOperationException("Edge to is null or empty");
                }
                
                if (!ids.Contains(c.From))
                {
                    // Let's also check if this might be a node name
                    var nodeNameMatch = def.Nodes.FirstOrDefault(n => n.Name.Equals(c.From, StringComparison.OrdinalIgnoreCase));
                    throw new InvalidOperationException(
                        $"Edge from unknown node ID '{c.From}'");
                }
                if (!ids.Contains(c.To))
                {
                    // Let's also check if this might be a node name
                    var nodeNameMatch = def.Nodes.FirstOrDefault(n => n.Name.Equals(c.To, StringComparison.OrdinalIgnoreCase));
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