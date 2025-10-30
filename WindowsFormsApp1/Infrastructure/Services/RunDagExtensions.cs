using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Domain.Flow.Dag;
using WindowsFormsApp1.Core.Domain.Flow.Engine;
using WindowsFormsApp1.Infrastructure.Di;

namespace WindowsFormsApp1.Infrastructure.Services.Services
{
    public static class RunDagExtensions
    {
        public static async Task RunDagInBackground(this IServiceProvider provider,
                                                     string jsonFile,
                                                     int maxDegree = 4,
                                                     CancellationToken ct = default)
        {
            try
            {
                var dag = DagFlowLoader.LoadJson(jsonFile);

                provider.PopulateActionRegistry();
                var registry = provider.GetRequiredService<IActionRegistry>();
                
                var context = provider.GetRequiredService<IFlowContext>();

                var executor = new DagExecutor(registry, context);

                await executor.RunAsync(dag, ct, maxDegree);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RunDagInBackground: {ex}");
                Console.WriteLine($"Exception type: {ex.GetType()}");
                Console.WriteLine($"Exception message: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}