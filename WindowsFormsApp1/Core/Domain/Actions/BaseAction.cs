using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Domain.Flow.Engine;

namespace WindowsFormsApp1.Core.Domain.Actions
{
    /// <summary>
    /// Base class for all flow actions providing common functionality
    /// </summary>
    public abstract class BaseAction : IFlowAction
    {
        public abstract string Key { get; }
        
        /// <summary>
        /// Executes the action with the given context
        /// </summary>
        /// <param name="ctx">The flow context</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public abstract Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default);
        
        /// <summary>
        /// Logs a message with the action key prefix
        /// </summary>
        /// <param name="message">Message to log</param>
        protected void LogInfo(string message)
        {
            Console.WriteLine($"[{Key}] {message}");
        }
        
        /// <summary>
        /// Logs an error message with the action key prefix
        /// </summary>
        /// <param name="message">Error message to log</param>
        protected void LogError(string message)
        {
            Console.WriteLine($"[{Key}][ERROR] {message}");
        }
        
        /// <summary>
        /// Logs a warning message with the action key prefix
        /// </summary>
        /// <param name="message">Warning message to log</param>
        protected void LogWarning(string message)
        {
            Console.WriteLine($"[{Key}][WARN] {message}");
        }
    }
}