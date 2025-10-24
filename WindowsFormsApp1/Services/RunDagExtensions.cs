using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Domain.Flow.Dag;
using WindowsFormsApp1.Domain.Flow.Engine;
using WindowsFormsApp1.Infrastructure.Di;

namespace WindowsFormsApp1.Services
{
    public static class RunDagExtensions
    {
        public static async Task RunDagInBackground(this IServiceProvider provider,
                                                     string jsonFile,
                                                     int maxDegree = 4,
                                                     CancellationToken ct = default)
        {
            var loader = new DagFlowLoader();
            var dag = DagFlowLoader.LoadJson(jsonFile);

            provider.PopulateActionRegistry();
            var registry = provider.GetRequiredService<IActionRegistry>();
            var context = provider.GetRequiredService<IFlowContext>();
            var executor = new DagExecutor(registry, context);

            await executor.RunAsync(dag, ct, maxDegree);
        }
    }
}
