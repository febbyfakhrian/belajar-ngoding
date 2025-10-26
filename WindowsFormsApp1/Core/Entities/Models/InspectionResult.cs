using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.Core.Entities.Models
{
    public class InspectionResult
    {
        public string TransactionId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Pass { get; set; }
        public double CycleTimeMs { get; set; }
        public string RawResponse { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
