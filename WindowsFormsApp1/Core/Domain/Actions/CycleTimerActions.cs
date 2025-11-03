using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Domain.Flow.Engine;
using WindowsFormsApp1.Infrastructure.Services;

namespace WindowsFormsApp1.Core.Domain.Actions
{
    /// <summary>
    /// Action to start the cycle timer
    /// </summary>
    public sealed class CycleTimerStartAction : BaseAction
    {
        public override string Key => "CycleTimer.Start";

        public override Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            try
            {
                // Record the start time in the context
                ctx.Vars["CycleStartTime"] = DateTime.Now;
                
                // Generate a transaction ID if not already present
                if (!ctx.Vars.ContainsKey("TransactionId"))
                {
                    ctx.Vars["TransactionId"] = Guid.NewGuid().ToString();
                }
                
                LogInfo($"Cycle timer started. Transaction ID: {ctx.Vars["TransactionId"]}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to start cycle timer: {ex.Message}");
                throw;
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Action to end the cycle timer and save the results to the database
    /// </summary>
    public sealed class CycleTimerEndAction : BaseAction
    {
        private readonly CycleTimeDbOperation _cycleTimeDb;

        public override string Key => "CycleTimer.End";

        public CycleTimerEndAction(CycleTimeDbOperation cycleTimeDb)
        {
            _cycleTimeDb = cycleTimeDb ?? throw new ArgumentNullException(nameof(cycleTimeDb));
        }

        public override Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            try
            {
                // Ensure the cycle_times table exists
                _cycleTimeDb.CreateTableIfNotExists();

                // Get the start time from context
                if (!ctx.Vars.TryGetValue("CycleStartTime", out var startTimeObj) || !(startTimeObj is DateTime startTime))
                {
                    LogError("Cycle start time not found in context");
                    throw new InvalidOperationException("Cycle start time not found in context");
                }

                // Get transaction ID from context
                if (!ctx.Vars.TryGetValue("TransactionId", out var transactionIdObj) || !(transactionIdObj is string transactionId))
                {
                    LogError("Transaction ID not found in context");
                    throw new InvalidOperationException("Transaction ID not found in context");
                }

                // Record the end time
                DateTime endTime = DateTime.Now;
                
                // Calculate cycle time in milliseconds
                int cycleTimeMs = (int)(endTime - startTime).TotalMilliseconds;

                // Determine pass/fail status (default to false if not set)
                bool pass = ctx.FinalLabel ?? false;

                // Get image ID if available
                string imageId = ctx.LastImageId;

                // Get raw response if available
                string rawResponse = ctx.LastGrpcJson;

                // Insert the cycle time record into the database
                _cycleTimeDb.InsertCycleTime(
                    transactionId,
                    startTime,
                    endTime,
                    cycleTimeMs,
                    rawResponse,
                    pass,
                    imageId);

                LogInfo($"Cycle timer ended. Transaction ID: {transactionId}, Cycle Time: {cycleTimeMs}ms, Pass: {pass}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to end cycle timer: {ex.Message}");
                throw;
            }

            return Task.CompletedTask;
        }
    }
}