using Newtonsoft.Json;

namespace WindowsFormsApp1.Core.Entities.Models
{
    class ApiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
