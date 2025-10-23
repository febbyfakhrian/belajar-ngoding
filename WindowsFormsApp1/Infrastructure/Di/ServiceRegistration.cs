using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Domain.Actions;
using WindowsFormsApp1.Domain.Flow.Engine;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1.Infrastructure.Di
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddAppServices(this IServiceCollection s, SQLiteConnection db, PlcOperation plc)
        {
            s.AddSingleton(db);
            s.AddSingleton(new ImageDbOperation(db));                       // :contentReference[oaicite:13]{index=13}
            s.AddSingleton(plc);                                             // :contentReference[oaicite:14]{index=14}
            s.AddSingleton<CameraManager>(_ => new CameraManager(new System.Collections.Concurrent.ConcurrentQueue<byte[]>())); // :contentReference[oaicite:15]{index=15}
            s.AddSingleton<GrpcService>();                                   // :contentReference[oaicite:16]{index=16}

            s.AddSingleton<IFlowContext, FlowContext>();
            s.AddSingleton<IActionRegistry, ActionRegistry>();

            // Core actions
            s.AddSingleton<IFlowAction, PlcLampOnAction>();
            s.AddSingleton<IFlowAction, PlcSendPassAction>();
            s.AddSingleton<IFlowAction, PlcSendFailAction>();
            s.AddSingleton<IFlowAction, CameraPrepareAction>();
            s.AddSingleton<IFlowAction, CameraCaptureFrameAction>();
            s.AddSingleton<IFlowAction, GrpcProcessImageAction>();

            s.AddSingleton<PlcReadSubscription>();
            return s;
        }

        public static void PopulateActionRegistry(this ServiceProvider sp)
        {
            var reg = sp.GetRequiredService<IActionRegistry>();
            foreach (var a in sp.GetServices<IFlowAction>()) reg.Register(a);
        }
    }
}
