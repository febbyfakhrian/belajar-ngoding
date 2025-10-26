using Microsoft.Extensions.DependencyInjection;
using System;
using WindowsFormsApp1.Core.Domain.Flow.Engine;

namespace WindowsFormsApp1.Infrastructure.Di
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services, string connectionString)
        {
            // Register all service groups
            services.AddDomainServices();
            services.AddHardwareServices();
            services.AddDataServices(connectionString);
            services.AddCommonServices();
            services.AddLoggingServices();
        }
        
        public static void PopulateActionRegistry(this IServiceProvider provider)
        {
            var reg = provider.GetRequiredService<IActionRegistry>();
            foreach (var a in provider.GetServices<IFlowAction>())
                reg.Register(a);
        }
    }
}