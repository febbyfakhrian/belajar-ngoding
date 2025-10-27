// ResultDTO.cs
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WindowsFormsApp1
{
    public class ResultDTO
    {
        [JsonProperty("boxes")]
        public List<float[]> Boxes { get; set; }

        [JsonProperty("scores")]
        public List<float> Scores { get; set; }

        [JsonProperty("labels")]
        public List<string> Labels { get; set; }
    }
}