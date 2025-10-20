using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Activities
{
    public class StartActivity : IActivity
    {
        public Task ExecuteAsync(NodeContext ctx, CancellationToken ct)
        {
            Console.WriteLine("🔹 Workflow started");
            return Task.CompletedTask;
        }
    }
}
