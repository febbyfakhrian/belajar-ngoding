using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.Core.Entities.Models
{
    public class ComponentItem
    {
        [JsonProperty("boxes")]
        public List<int> Boxes { get; set; }

        [JsonProperty("score")]
        public double? Score { get; set; }

        [JsonProperty("value")]
        public String Value { get; set; }

        [JsonProperty("label")]
        public bool Label { get; set; }

        [JsonProperty("image_id")]
        public String ImageId { get; set; }
    }
}
