using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Domain.Flow.Engine;

namespace WindowsFormsApp1.Core.Domain.Actions
{
    // Loop start action that initializes loop state
    public sealed class LoopStartAction : BaseAction
    {
        public override string Key => "Flow.LoopStart";
        
        public override Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            // Loop initialization is now handled by the DAG executor
            // This action exists for compatibility with action-based loop definitions
            LogInfo("Loop start action executed");
            return Task.CompletedTask;
        }
    }
    
    // Loop end action that manages loop completion and continuation
    public sealed class LoopEndAction : BaseAction
    {
        public override string Key => "Flow.LoopEnd";
        
        public override Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            // Loop completion is now handled by the DAG executor
            // This action exists for compatibility with action-based loop definitions
            LogInfo("Loop end action executed");
            return Task.CompletedTask;
        }
    }
    
    // Loop controller that manages iterative execution
    public sealed class LoopControllerAction : BaseAction
    {
        public override string Key => "Flow.LoopController";
        
        public override async Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            // Get loop parameters from context vars
            var loopId = ctx.Vars.ContainsKey("loopId") ? ctx.Vars["loopId"].ToString() : Guid.NewGuid().ToString();
            var maxIterations = ctx.Vars.ContainsKey("maxIterations") ? Convert.ToInt32(ctx.Vars["maxIterations"]) : -1; // -1 for infinite
            var triggerName = ctx.Vars.ContainsKey("triggerName") ? ctx.Vars["triggerName"].ToString() : "LOOP_CONTINUE";
            
            // For backward compatibility, we'll just log and set a trigger
            LogInfo($"Loop controller executed for loop {loopId}");
            
            // In the new approach, the DAG executor handles loop state
            // We'll just set a context variable to indicate loop status
            ctx.Vars["loopActive"] = true;
            ctx.Vars["loopId"] = loopId;
        }
    }
    
    // Reset loop controller to start a new loop
    public sealed class LoopResetAction : BaseAction
    {
        public override string Key => "Flow.LoopReset";
        
        public override Task ExecuteAsync(IFlowContext ctx, CancellationToken ct = default)
        {
            var loopId = ctx.Vars.ContainsKey("loopId") ? ctx.Vars["loopId"].ToString() : Guid.NewGuid().ToString();
            
            // Reset loop state in context
            ctx.Vars["loopId"] = loopId;
            ctx.Vars["loopActive"] = false;
            ctx.Vars["loopCompleted"] = false;
            
            LogInfo($"Loop {loopId} reset");
            return Task.CompletedTask;
        }
    }
}