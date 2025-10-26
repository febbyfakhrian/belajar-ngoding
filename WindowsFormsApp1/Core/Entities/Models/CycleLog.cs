using System;

namespace WindowsFormsApp1.Core.Entities.Models
{
    class CycleLog
    {
        public int TransactionId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public double CycleTimeMs { get; set; } // dalam ms
        public string Response { get; set; }
    }
}
