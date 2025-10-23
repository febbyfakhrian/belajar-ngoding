using Newtonsoft.Json;

namespace WindowsFormsApp1.Models
{
    class ApiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
