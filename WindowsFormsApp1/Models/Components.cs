using Newtonsoft.Json;
using System.Collections.Generic;

namespace WindowsFormsApp1.Models
{
    public class Components
    {
        [JsonProperty("fiducial")]
        public List<ComponentItem> Fiducial { get; set; }

        [JsonProperty("screw")]
        public List<ComponentItem> Screw { get; set; }

        [JsonProperty("connector")]
        public List<ComponentItem> Connector { get; set; }

    }
}
