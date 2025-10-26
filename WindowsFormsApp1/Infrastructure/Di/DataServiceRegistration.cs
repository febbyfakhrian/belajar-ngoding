using Microsoft.Extensions.DependencyInjection;
using WindowsFormsApp1.Core.Interfaces;
using WindowsFormsApp1.Infrastructure.Data;
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
        }
    }
}