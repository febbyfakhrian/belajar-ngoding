using Microsoft.Extensions.DependencyInjection;
using WindowsFormsApp1.Core.Common.Helpers;
using WindowsFormsApp1.Infrastructure.Data;

namespace WindowsFormsApp1.Infrastructure.Di
{
    public static class CommonServiceRegistration
    {
        public static void AddCommonServices(this IServiceCollection services)
        {
            // Common services
            services.AddSingleton<FileUtils>(provider => 
                new FileUtils(provider.GetRequiredService<ImageDbOperation>()));
            services.AddSingleton<ImageGrabber>();
            services.AddSingleton<VideoGrabber>();
        }
    }
}