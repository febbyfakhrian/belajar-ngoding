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
                Console.WriteLine($"Loading DAG from {jsonFile}...");
                var dag = DagFlowLoader.LoadJson(jsonFile);
                Console.WriteLine("DAG loaded successfully.");

                Console.WriteLine("Populating action registry...");
                provider.PopulateActionRegistry();
                Console.WriteLine("Action registry populated.");

                Console.WriteLine("Getting action registry...");
                var registry = provider.GetRequiredService<IActionRegistry>();
                Console.WriteLine("Action registry obtained.");

                Console.WriteLine("Getting flow context...");
                var context = provider.GetRequiredService<IFlowContext>();
                Console.WriteLine("Flow context obtained.");

                Console.WriteLine("Creating DAG executor...");
                var executor = new DagExecutor(registry, context);
                Console.WriteLine("DAG executor created.");

                Console.WriteLine("Running DAG executor...");
                await executor.RunAsync(dag, ct, maxDegree);
                Console.WriteLine("DAG executor completed.");
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