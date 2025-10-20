using Newtonsoft.Json;
using System.Collections.Generic;

namespace WindowsFormsApp1.Models
{
    class Root
    {
        [JsonProperty("components")]
        public Dictionary<string, List<ComponentItem>> Components { get; set; }

        [JsonProperty("step_index")]
        public int StepIndex { get; set; }

        [JsonProperty("final_label")]
        public bool FinalLabel { get; set; }
    }
}
