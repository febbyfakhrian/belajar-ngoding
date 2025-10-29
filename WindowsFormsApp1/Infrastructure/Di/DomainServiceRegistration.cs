using Microsoft.Extensions.DependencyInjection;
using WindowsFormsApp1.Core.Domain.Flow.Engine;
using WindowsFormsApp1.Core.Domain.Flow.Dag;
using WindowsFormsApp1.Core.Domain.Actions;

namespace WindowsFormsApp1.Infrastructure.Di
{
    public static class DomainServiceRegistration
    {
        public static void AddDomainServices(this IServiceCollection services)
        {
            // Flow engine services
            services.AddSingleton<IFlowContext, FlowContext>();
            services.AddSingleton<IActionRegistry, ActionRegistry>();
            
            // DAG services
            services.AddTransient<DagExecutor>();
            services.AddTransient<DagFlowLoader>();
            
            // Domain actions
            services.AddTransient<IFlowAction, PlcLampOnAction>();
            services.AddTransient<IFlowAction, PlcLampOffAction>();
            services.AddTransient<IFlowAction, PlcSendPassAction>();
            services.AddTransient<IFlowAction, PlcSendFailAction>();
            services.AddTransient<IFlowAction, CameraPrepareAction>();
            services.AddTransient<IFlowAction, CameraCaptureFrameAction>();
            services.AddTransient<IFlowAction, GrpcProcessImageAction>();
            services.AddTransient<IFlowAction, PlcSubscribeReadAction>();
            services.AddTransient<IFlowAction, PlcUnsubscribeReadAction>();
            
            // Loop control actions
            services.AddTransient<IFlowAction, LoopStartAction>();
            services.AddTransient<IFlowAction, LoopEndAction>();
            services.AddTransient<IFlowAction, FinalizeLoopAction>(); // Add the missing Finalize.Loop action
            services.AddTransient<IFlowAction, LoopControllerAction>();
            services.AddTransient<IFlowAction, LoopResetAction>();
        }
    }
}