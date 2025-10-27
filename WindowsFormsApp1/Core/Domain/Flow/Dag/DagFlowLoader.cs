using System;
using System.IO;
using Newtonsoft;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Core.Domain.Flow.Dag
{
    class DagFlowLoader
    {
        public static DagDefinition LoadJson(string fileName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            Debug.WriteLine($"[DAG] Loading JSON from path: {path}");
            if (!File.Exists(path))
                throw new FileNotFoundException("DAG JSON not found", path);

            var json = File.ReadAllText(path);
            Debug.WriteLine($"[DAG] JSON content length: {json.Length}");

            try
            {
                // Try to deserialize the entire structure first
                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    DateParseHandling = DateParseHandling.None
                };

                var dag = JsonConvert.DeserializeObject<DagDefinition>(json, settings);

                if (dag != null)
                {
                    Debug.WriteLine($"[DAG] Successfully deserialized DAG: {dag.Name} with {dag.Nodes?.Count ?? 0} nodes");
                    
                    // Log nodes for debugging
                    if (dag.Nodes != null)
                    {
                        foreach (var node in dag.Nodes)
                        {
                            Debug.WriteLine($"[DAG] Node - ID: '{node.Id}', Name: '{node.Name}', Type: '{node.Type}'");
                        }
                    }
                    
                    // Parse the connections from the raw connections data
                    Debug.WriteLine("[DAG] Parsing connections from raw data...");
                    var jObject = JObject.Parse(json);
                    var connectionsElement = jObject["connections"];
                    if (connectionsElement != null && dag.Nodes != null)
                    {
                        dag.Connections = ParseConnections(connectionsElement, dag.Nodes);
                    }
                    
                    return dag;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DAG] Error during JSON deserialization: {ex.Message}");
                Debug.WriteLine($"[DAG] Falling back to manual parsing...");
            }
            
            // Fallback to manual parsing
            return LoadJsonManually(json);
        }
        
        private static DagDefinition LoadJsonManually(string json)
        {
            var root = JObject.Parse(json);

            var dag = new DagDefinition();

            if (root.TryGetValue("name", out var nameToken))
            {
                dag.Name = nameToken?.ToString() ?? string.Empty;
            }

            if (root.TryGetValue("id", out var idToken))
            {
                dag.Id = idToken?.ToString() ?? string.Empty;
            }

            if (root.TryGetValue("nodes", out var nodesToken) && nodesToken is JArray nodesArray)
            {
                var nodes = new List<DagNode>();
                foreach (var nodeToken in nodesArray)
                {
                    var node = new DagNode
                    {
                        Id = nodeToken["id"]?.ToString() ?? string.Empty,
                        Name = nodeToken["name"]?.ToString() ?? string.Empty,
                        Type = nodeToken["type"]?.ToString() ?? "action"
                    };

                    var paramsToken = nodeToken["parameters"];
                    if (paramsToken != null && paramsToken.Type != JTokenType.Null)
                    {
                        try
                        {
                            node.Parameters = paramsToken.ToObject<Dictionary<string, object>>()
                                              ?? new Dictionary<string, object>();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[DAG] Error deserializing parameters for node {node.Id}: {ex.Message}");
                            node.Parameters = new Dictionary<string, object>();
                        }
                    }
                    else
                    {
                        node.Parameters = new Dictionary<string, object>();
                    }

                    Debug.WriteLine($"[DAG] Loaded node - ID: '{node.Id}', Name: '{node.Name}', Type: '{node.Type}'");
                    if (!string.IsNullOrEmpty(node.Id)) // Only add nodes with valid IDs
                    {
                        nodes.Add(node);
                    }
                }

                dag.Nodes = nodes;
            }

            // Process connections with the complex structure
            if (root.TryGetValue("connections", out var connectionsToken))
            {
                Debug.WriteLine($"[DAG] Connections token type: {connectionsToken.Type}");
                dag.Connections = ParseConnections(connectionsToken, dag.Nodes ?? new List<DagNode>());
            }

            return dag;
        }
        
        private static List<DagEdge> ParseConnections(JToken connectionsElement, List<DagNode> nodes)
        {
            var edges = new List<DagEdge>();
            
            // Create a mapping from node names to node IDs
            var nodeNameToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in nodes)
            {
                // Handle potential duplicate names by using the first one found
                if (!string.IsNullOrEmpty(node.Name) && !nodeNameToIdMap.ContainsKey(node.Name))
                {
                    nodeNameToIdMap[node.Name] = node.Id;
                    Debug.WriteLine($"[DAG] Mapping node name '{node.Name}' to ID '{node.Id}'");
                }
                else if (!string.IsNullOrEmpty(node.Name))
                {
                    Debug.WriteLine($"[DAG] Duplicate node name found: {node.Name}. Using first occurrence.");
                }
                else
                {
                    Debug.WriteLine($"[DAG] Warning: Node with empty name and ID '{node.Id}' encountered");
                }
            }

            foreach (var connectionProperty in (connectionsElement as JObject).Properties())
            {
                // The connectionProperty.Name is the SOURCE NODE NAME (not ID)
                var fromNodeName = connectionProperty.Name;
                Debug.WriteLine($"[DAG] Processing connections from node name: '{fromNodeName}'");

                // Map the source node name to its actual ID
                if (!nodeNameToIdMap.TryGetValue(fromNodeName, out var fromNodeId))
                {
                    Debug.WriteLine($"[DAG] ERROR: Could not find node with name '{fromNodeName}'");
                    continue;
                }

                Debug.WriteLine($"[DAG] Processing connections from node '{fromNodeName}' (ID: {fromNodeId})");

                // Each connection property contains keys like "main", "done", etc.
                var valueObj = connectionProperty.Value as JObject;
                if (valueObj == null)
                    continue;

                foreach (var keyProperty in valueObj.Properties())
                {
                    Debug.WriteLine($"[DAG] Processing connection key: '{keyProperty.Name}'");

                    // Each key contains an array of arrays of connection targets
                    if (keyProperty.Value is JArray outerArray)
                    {
                        foreach (var connectionArray in outerArray)
                        {
                            if (connectionArray is JArray innerArray)
                            {
                                foreach (var targetElement in innerArray)
                                {
                                    var toNodeName = targetElement["node"]?.ToString() ?? string.Empty;
                                    Debug.WriteLine($"[DAG] Target node name: '{toNodeName}'");

                                    if (!nodeNameToIdMap.TryGetValue(toNodeName, out var toNodeId))
                                    {
                                        Debug.WriteLine($"[DAG] ERROR: Could not find target node with name '{toNodeName}'");
                                        continue;
                                    }

                                    var edge = new DagEdge
                                    {
                                        From = fromNodeId,
                                        To = toNodeId,
                                        Key = keyProperty.Name, // "main", "done", etc.
                                        When = targetElement["when"]?.ToString() ?? string.Empty
                                    };

                                    edges.Add(edge);
                                    Debug.WriteLine($"[DAG] Added connection: '{fromNodeName}' ({fromNodeId}) -> '{toNodeName}' ({toNodeId})");
                                }
                            }
                        }
                    }
                }
            }

            Debug.WriteLine($"[DAG] Total connections parsed: {edges.Count}");
            return edges;
        }
    }
}