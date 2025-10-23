using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.Models
{
    class CycleTimeReport
    {
        public DateTime ReportDate { get; set; }
        public List<CycleLog> Logs { get; set; } = new List<CycleLog>();
        public double AverageMs { get; set; }
        public double MinMs { get; set; }
        public double MaxMs { get; set; }
    }
}
