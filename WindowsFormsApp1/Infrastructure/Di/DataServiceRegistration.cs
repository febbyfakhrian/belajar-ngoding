using Microsoft.Extensions.DependencyInjection;
using WindowsFormsApp1.Core.Interfaces;
using WindowsFormsApp1.Infrastructure.Data;
using WindowsFormsApp1.Infrastructure.Services;
using System.Data.SQLite;

namespace WindowsFormsApp1.Infrastructure.Di
{
    public static class DataServiceRegistration
    {
        public static void AddDataServices(this IServiceCollection services, string connectionString)
        {
            // Database connection
            services.AddSingleton<SQLiteConnection>(provider => new SQLiteConnection(connectionString));
            
            // Data services
            services.AddSingleton<ISettingsService, SettingsOperation>();
            // Register ImageDbOperation with its required dependency
            services.AddSingleton<ImageDbOperation>(provider => new ImageDbOperation(provider.GetRequiredService<SQLiteConnection>()));
            // Register CycleTimeDbOperation with its required dependency
            services.AddSingleton<CycleTimeDbOperation>(provider => new CycleTimeDbOperation(provider.GetRequiredService<SQLiteConnection>()));
        }
    }
}