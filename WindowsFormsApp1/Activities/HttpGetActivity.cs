using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Activities
{
    public class HttpGetActivity : IActivity
    {
        private readonly HttpClient _client = new HttpClient();

        public async Task ExecuteAsync(NodeContext ctx, CancellationToken ct)
        {
            var url = ctx.Node.Params != null && ctx.Node.Params.ContainsKey("url")
                ? ctx.Node.Params["url"].ToString()
                : null;

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Missing parameter 'url'");

            Console.WriteLine($"🌐 GET {url}");
            var response = await _client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            ctx.Data[$"{ctx.Node.Id}.body"] = body;

            Console.WriteLine($"✅ {ctx.Node.Id} OK ({body.Length} chars)");
        }
    }
}
