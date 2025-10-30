﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WindowsFormsApp1;
using WindowsFormsApp1.Core.Domain.Actions;
using WindowsFormsApp1.Core.Domain.Flow.Engine;
using WindowsFormsApp1.Infrastructure.Di;
using WindowsFormsApp1.Infrastructure.Hardware.PLC;
using WindowsFormsApp1.Core.Interfaces;
using WindowsFormsApp1.Infrastructure.Services.Services;
using WindowsFormsApp1.Core.Domain.Flow.Dag; // Add this using statement

namespace AutoInspectionPlatform
{
    static class Program
    {
        public static SQLiteConnection DbConnection; // masih dipakai form lama

        [STAThread]
        static void Main()
        {
            try
            {
                // 1. Build services once
                var services = new ServiceCollection();
                string connectionString = $"Data Source={GetDatabasePath()};Version=3;";

                services.AddApplicationServices(connectionString); // Register our new services
                
                ConfigurePlcServices(services);        // PLC

                var provider = services.BuildServiceProvider();

                // Configure PLC after service provider is built
                ConfigurePlcAfterBuild(provider);

                var grpc = provider.GetRequiredService<IGrpcService>();
                if (!grpc.StartAsync().Result) Console.WriteLine("gRPC not ready");

                var settingsService = provider.GetRequiredService<ISettingsService>();
                var urlPath = settingsService.GetSetting<string>("workflow", "config_path");

                if (File.Exists(urlPath))
                {
                    // 2. Jalankan DAG (fire-and-forget) → 1 baris
                    _ = provider.RunDagInBackground(urlPath,
                                          maxDegree: 4,
                                          CancellationToken.None);
                }

                // 3. Start UI
                Application.Run(new LoginForm(provider, connectionString));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex}");
                Console.WriteLine($"Exception type: {ex.GetType()}");
                Console.WriteLine($"Exception message: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        static void ConfigurePlcServices(IServiceCollection s)
        {
            s.AddSingleton<IPlcService, PlcOperation>();
            s.AddSingleton<PlcReadSubscription>((provider) => 
                new PlcReadSubscription(provider.GetService<IPlcService>(), provider.GetRequiredService<IFlowContext>()));
        }
        
        static void ConfigurePlcAfterBuild(IServiceProvider provider)
        {
            var plc = provider.GetService<IPlcService>() as PlcOperation;
            if (plc != null)
            {
                var settings = provider.GetRequiredService<ISettingsService>();
                var port = settings.GetSetting<string>("plc", "serial_port");
                var baud = settings.GetSetting<int>("plc", "baud_rate");
                
                if (!string.IsNullOrWhiteSpace(port) && baud > 0)
                {
                    plc.SetConfig(port, baud);
                    if (plc.DeviceExists()) 
                    {
                        plc.Open();
                    }
                }
            }
        }

        /*------------------ utils ------------------*/
        static string GetDatabasePath()
        {
            var raw = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["DbPath"]);
            if (raw.StartsWith("~"))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                raw = Path.Combine(home, raw.Substring(1).TrimStart('/', '\\'));
            }
            Directory.CreateDirectory(Path.GetDirectoryName(raw));
            return raw;
        }
    }
}