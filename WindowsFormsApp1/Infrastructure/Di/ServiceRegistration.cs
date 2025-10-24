using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WindowsFormsApp1.Domain.Actions;
using WindowsFormsApp1.Domain.Flow.Engine;

namespace WindowsFormsApp1.Infrastructure.Di
{
    public static class ServiceRegistration
    {
        public static void PopulateActionRegistry(this IServiceProvider provider)
        {
            var reg = provider.GetRequiredService<IActionRegistry>();
            foreach (var a in provider.GetServices<IFlowAction>())
                reg.Register(a);
        }
    }
}