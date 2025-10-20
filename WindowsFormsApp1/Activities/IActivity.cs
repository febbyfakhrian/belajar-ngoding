using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static WindowsFormsApp1.Models.WorkflowModels;

namespace WindowsFormsApp1.Activities
{
    // 🔧 Tambahkan 'public' di sini
    public interface IActivity
    {
        Task ExecuteAsync(NodeContext ctx, CancellationToken ct);
    }

    // 🔧 Juga pastikan class ini public
    public class NodeContext
    {
        public NodeDef Node { get; set; }
        public IDictionary<string, object> Data { get; set; }

        public NodeContext(NodeDef node, IDictionary<string, object> data)
        {
            Node = node;
            Data = data;
        }
    }
}
