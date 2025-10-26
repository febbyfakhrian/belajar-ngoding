using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using WindowsFormsApp1.Core.Common.Logging;

namespace WindowsFormsApp1.Infrastructure.Di
{
    public static class LoggingServiceRegistration
    {
        public static void AddLoggingServices(this IServiceCollection services)
        {
            Debug.WriteLine("Registering logging services...");
            services.AddSingleton<ILogger, ConsoleLogger>(provider => new ConsoleLogger("Application"));
            Debug.WriteLine("Logging services registered.");
        }
    }
}