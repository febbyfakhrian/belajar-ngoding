﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Core.Domain.Flow.Dag
{
    public sealed class DagDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("nodes")]
        public List<DagNode> Nodes { get; set; } = new List<DagNode>();
        
        // This will be populated from the JSON connections structure
        [JsonIgnore] // Ignore direct serialization/deserialization as it's handled manually
        public List<DagEdge> Connections { get; set; } = new List<DagEdge>();
        
        // Add a property to hold the raw connections structure from JSON
        [JsonProperty("connections")]
        internal object RawConnections { get; set; }
    }

    public sealed class DagNode
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("type")]
        public string Type { get; set; } = "action"; // action | condition | trigger | delay | split-batches
        
        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public sealed class DagEdge
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty; // "main", "done", etc.
        public string When { get; set; } = string.Empty; // optional bool expression
    }
    
    // Add classes to represent the connection structure in JSON
    public class ConnectionTarget
    {
        [JsonProperty("node")]
        public string Node { get; set; }
        
        [JsonProperty("key")]
        public string Key { get; set; }
        
        [JsonProperty("index")]
        public int Index { get; set; }
        
        [JsonProperty("when", NullValueHandling = NullValueHandling.Ignore)]
        public string When { get; set; }
    }
}