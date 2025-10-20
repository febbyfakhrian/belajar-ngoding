using Polly.Retry;
using Polly;
using QuikGraph.Algorithms.TopologicalSort;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static WindowsFormsApp1.Models.WorkflowModels;

namespace WindowsFormsApp1.Activities
{
    public class WorkflowExecutor
    {
        private readonly Dictionary<string, IActivity> _activities;
        private readonly AsyncRetryPolicy _retryPolicy;

        public WorkflowExecutor(Dictionary<string, IActivity> activities)
        {
            _activities = activities;

            // Policy Polly: retry max 3x (0.5s, 1s, 2s)
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    new[]
                    {
                        TimeSpan.FromMilliseconds(500),
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2)
                    },
                    (ex, t, count, _) =>
                    {
                        Console.WriteLine($"Retry {count}: {ex.Message}");
                    });
        }

        public async Task<IDictionary<string, object>> RunAsync(
            WorkflowDef wf, CancellationToken ct = default)
        {
            var graph = new AdjacencyGraph<NodeDef, Edge<NodeDef>>();
            foreach (var node in wf.Nodes)
                graph.AddVertex(node);

            foreach (var node in wf.Nodes)
                foreach (var dep in node.DependsOn)
                    graph.AddEdge(new Edge<NodeDef>(
                        wf.Nodes.First(n => n.Id == dep), node));

            // Topological sort: urutkan node tanpa siklus
            var sorter = new TopologicalSortAlgorithm<NodeDef, Edge<NodeDef>>(graph);
            sorter.Compute();
            var order = sorter.SortedVertices.ToList();

            var data = new Dictionary<string, object>();

            foreach (var node in order)
            {
                Console.WriteLine($"➡️  {node.Type} ({node.Id})");

                var ctx = new NodeContext(node, data);

                if (!_activities.TryGetValue(node.Type, out var act))
                    throw new InvalidOperationException($"Unknown activity {node.Type}");

                // Jalankan dengan Polly retry
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await act.ExecuteAsync(ctx, ct);
                });
            }

            return data;
        }
    }
}
