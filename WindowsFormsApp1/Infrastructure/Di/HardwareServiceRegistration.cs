using Microsoft.Extensions.DependencyInjection;
using WindowsFormsApp1.Core.Interfaces;
using WindowsFormsApp1.Infrastructure.Hardware.Camera;
using WindowsFormsApp1.Infrastructure.Hardware.PLC;
using System.Collections.Concurrent;
using System.Diagnostics; // Add this using statement

namespace WindowsFormsApp1.Infrastructure.Di
{
    public static class HardwareServiceRegistration
    {
        public static void AddHardwareServices(this IServiceCollection services)
        {
            Debug.WriteLine("Registering hardware services...");
            // Hardware services
            // Register CameraManager directly with its required dependency
            services.AddSingleton<CameraManager>(provider => 
                new CameraManager(new ConcurrentQueue<byte[]>()));
            services.AddSingleton<ICameraService>(provider => provider.GetRequiredService<CameraManager>());
            // IPlcService registration moved to Program.cs to handle configuration properly
            services.AddSingleton<IGrpcService>(provider => 
                new Infrastructure.Hardware.Grpc.GrpcService(provider.GetService<ISettingsService>())); // Provide settings service to GrpcService
            Debug.WriteLine("Hardware services registered.");
        }
    }
}