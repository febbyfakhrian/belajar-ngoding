using Microsoft.Extensions.DependencyInjection;
using WindowsFormsApp1.Core.Common.Helpers;
using WindowsFormsApp1.Infrastructure.Data;
using WindowsFormsApp1.Core.Interfaces; // Added for ISettingsService

namespace WindowsFormsApp1.Infrastructure.Di
{
    public static class CommonServiceRegistration
    {
        public static void AddCommonServices(this IServiceCollection services)
        {
            // Common services
            services.AddSingleton<FileUtils>(provider => 
                new FileUtils(
                    provider.GetRequiredService<ImageDbOperation>(),
                    provider.GetService<ISettingsService>())); // Added settings service
            services.AddSingleton<ImageGrabber>();
            services.AddSingleton<VideoGrabber>();
        }
    }
}