using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Activities
{
    public class ScriptActivity : IActivity
    {
        public Task ExecuteAsync(NodeContext ctx, CancellationToken ct)
        {
            // Ambil parameter 'source' (mis. fetch.body)
            var source = ctx.Node.Params != null && ctx.Node.Params.ContainsKey("source")
                ? ctx.Node.Params["source"].ToString()
                : null;

            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Missing 'source' param");

            if (ctx.Data.TryGetValue(source, out var val))
            {
                var text = val?.ToString()?.ToUpperInvariant();
                ctx.Data[$"{ctx.Node.Id}.result"] = text;
                Console.WriteLine($"🧩 {ctx.Node.Id} transform done");
            }
            else
            {
                Console.WriteLine($"⚠️  Source '{source}' not found");
            }

            return Task.CompletedTask;
        }
    }
}
