using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1;
using WindowsFormsApp1.Domain.Actions;
using WindowsFormsApp1.Domain.Flow.Dag;
using WindowsFormsApp1.Domain.Flow.Engine;
using WindowsFormsApp1.Helpers;
using WindowsFormsApp1.Infrastructure.Di;
using WindowsFormsApp1.Services;

namespace AutoInspectionPlatform
{
    static class Program
    {
        public static SQLiteConnection DbConnection; // masih dipakai form lama

        [STAThread]
        static void Main()
        {
            // 1. Build services once
            var services = new ServiceCollection();
            ConfigureSharedServices(services);     // DB + Settings
            ConfigurePlcServices(services);        // PLC
            ConfigureCameraServices(services);     // Camera
            ConfigureAiServices(services);         // AI / gRPC
            ConfigureFlowServices(services);       // DAG context & registry

            var provider = services.BuildServiceProvider();

            // 2. Jalankan DAG (fire-and-forget) → 1 baris
            _ = provider.RunDagInBackground("flowtest.json",
                                            maxDegree: 4,
                                            CancellationToken.None);

            // 3. Start UI
            Application.Run(new MainDashboard(provider));
        }

        /*-------------- modular service registration --------------*/
        static void ConfigureSharedServices(IServiceCollection s)
        {
            DbConnection = new SQLiteConnection($"Data Source={GetDatabasePath()};Version=3;");
            DbConnection.Open();

            s.AddSingleton(DbConnection);
            s.AddSingleton(new ImageDbOperation(DbConnection));
            s.AddSingleton<SettingsOperation>();
            s.AddSingleton<FileUtils>();
        }

        static void ConfigurePlcServices(IServiceCollection s)
        {
            var settings = s.BuildServiceProvider().GetRequiredService<SettingsOperation>();
            var port = settings.GetSetting<string>("plc", "serial_port");
            var baud = settings.GetSetting<int>("plc", "baud_rate");

            PlcOperation plc = null;
            if (!string.IsNullOrWhiteSpace(port) && baud > 0)
            {
                plc = new PlcOperation(port, baud);
                if (plc.DeviceExists()) plc.Open();
            }
            s.AddSingleton(plc);
            s.AddSingleton<PlcReadSubscription>();

            // ➜➜➜  action-nya baru diregister di sini
            s.AddSingleton<IFlowAction, PlcSubscribeReadAction>();
        }

        static void ConfigureCameraServices(IServiceCollection s)
        {
            s.AddSingleton<CameraManager>(_ => new CameraManager(new System.Collections.Concurrent.ConcurrentQueue<byte[]>()));
        }

        static void ConfigureAiServices(IServiceCollection s)
        {
            s.AddSingleton<GrpcService>();
        }

        static void ConfigureFlowServices(IServiceCollection s)
        {
            // context & infra
            s.AddSingleton<IFlowContext, FlowContext>();
            s.AddSingleton<IActionRegistry, ActionRegistry>();
            s.AddSingleton<PlcReadSubscription>();

            // ---------- CORE ACTIONS (jangan lupa!) ----------
            s.AddSingleton<IFlowAction, PlcLampOnAction>();
            s.AddSingleton<IFlowAction, PlcSendPassAction>();
            s.AddSingleton<IFlowAction, PlcSendFailAction>();
            s.AddSingleton<IFlowAction, CameraPrepareAction>();
            s.AddSingleton<IFlowAction, CameraCaptureFrameAction>();
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