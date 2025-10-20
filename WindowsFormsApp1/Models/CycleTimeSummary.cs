using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.Models
{
    public class CycleTimeSummary
    {
        public DateTime Timestamp { get; set; }
        public List<InspectionResult> Logs { get; set; } = new List<InspectionResult>();
        public double AverageMs { get; set; }
        public double MinMs { get; set; }
        public double MaxMs { get; set; }
    }
}
