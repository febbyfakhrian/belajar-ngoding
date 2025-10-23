// ResultDTO.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WindowsFormsApp1
{
    public class ResultDTO
    {
        [JsonPropertyName("boxes")]
        public List<float[]> Boxes { get; set; }

        [JsonPropertyName("scores")]
        public List<float> Scores { get; set; }

        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; }
    }
}