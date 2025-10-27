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
                
                Console.WriteLine("Registering application services...");
                services.AddApplicationServices(connectionString); // Register our new services
                
                Console.WriteLine("Configuring PLC services...");
                ConfigurePlcServices(services);        // PLC

                var provider = services.BuildServiceProvider();

                // Configure PLC after service provider is built
                ConfigurePlcAfterBuild(provider);

                var grpc = provider.GetRequiredService<IGrpcService>();
                if (!grpc.StartAsync().Result) Console.WriteLine("gRPC not ready");

                // 2. Jalankan DAG (fire-and-forget) → 1 baris
                _ = provider.RunDagInBackground("inspectionflow.json",
                                      maxDegree: 4,
                                      CancellationToken.None);

                // 3. Start UI
                Application.Run(new MainDashboard(provider));
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

        static void TestDagLoading()
        {
            try
            {
                Console.WriteLine("Testing DAG JSON loading...");
                var dag = DagFlowLoader.LoadJson("inspectionflow.json");
                Console.WriteLine($"Successfully loaded DAG: {dag.Name}");
                Console.WriteLine($"Nodes count: {dag.Nodes.Count}");
                Console.WriteLine($"Connections count: {dag.Connections.Count}");
                
                foreach (var connection in dag.Connections)
                {
                    Console.WriteLine($"  {connection.From} -> {connection.To}");
                }
                
                Console.WriteLine("DAG loading test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DAG loading test: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        static void ConfigurePlcServices(IServiceCollection s)
        {
            Console.WriteLine("Registering IPlcService...");
            s.AddSingleton<IPlcService, PlcOperation>();
            Console.WriteLine("Registering PlcReadSubscription...");
            s.AddSingleton<PlcReadSubscription>((provider) => 
                new PlcReadSubscription(provider.GetService<IPlcService>(), provider.GetRequiredService<IFlowContext>()));
        }
        
        static void ConfigurePlcAfterBuild(IServiceProvider provider)
        {
            Console.WriteLine("Configuring PLC after build...");
            var plc = provider.GetService<IPlcService>() as PlcOperation;
            if (plc != null)
            {
                Console.WriteLine("PLC service found, getting settings...");
                var settings = provider.GetRequiredService<ISettingsService>();
                var port = settings.GetSetting<string>("plc", "serial_port");
                var baud = settings.GetSetting<int>("plc", "baud_rate");
                
                Console.WriteLine($"PLC settings - Port: {port}, Baud: {baud}");
                if (!string.IsNullOrWhiteSpace(port) && baud > 0)
                {
                    Console.WriteLine("Setting PLC config...");
                    plc.SetConfig(port, baud);
                    Console.WriteLine("Checking if PLC device exists...");
                    if (plc.DeviceExists()) 
                    {
                        Console.WriteLine("Opening PLC connection...");
                        plc.Open();
                    }
                    else
                    {
                        Console.WriteLine("PLC device does not exist, skipping open.");
                    }
                }
                else
                {
                    Console.WriteLine("PLC settings are invalid, skipping configuration.");
                }
            }
            else
            {
                Console.WriteLine("PLC service not found or not PlcOperation type.");
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