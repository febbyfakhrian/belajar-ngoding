using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Core.Entities.Models
{
    public class InspectionContext
    {
        public byte[] FrameBytes { get; set; }
        public string ImageId { get; set; }
        public Root AIResult { get; set; }   // hasil inference
        public string PLCCommand { get; set; }
        public bool FinalLabel { get; set; }
    }

    public interface IInspectionNode
    {
        string NodeId { get; }                // unique di workflow
        string NodeName { get; }                // nama tampilan designer
        Task<InspectionContext> ExecuteAsync(InspectionContext ctx);
    }
}
