using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WindowsFormsApp1;
using WindowsFormsApp1.Domain.Flow.Engine;
using WindowsFormsApp1.Domain.Flow.Yaml;
using WindowsFormsApp1.Infrastructure.Di;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Services;
using WindowsFormsApp1.Services.StateMachine;
using WorkflowCore.Interface;
using WorkflowCore.Services.DefinitionStorage;
using YamlDotNet.Serialization;

namespace AutoInspectionPlatform
{
    static class Program
    {
        public static SQLiteConnection DbConnection;
        public static ImageDbOperation DbHelper;
        public static PlcOperation PlcHelper;
        public static SettingsOperation SettingsOperation;

        public class CheckSignalReadNode
        {
            public void OnEnterCheckSignalRead()
            {
                LampOn();
                SendData();
                LogState();
            }

            // tetap dipanggil reflection
            public void LampOn() => Console.WriteLine("[CheckSignalRead] LampOn");
            public void SendData() => Console.WriteLine("[CheckSignalRead] SendData");
            public void LogState() => Console.WriteLine("[CheckSignalRead] LogState");
        }

        [STAThread]
        static void Main()
        {
            // DB
            DbConnection = new SQLiteConnection($"Data Source={GetDatabasePath()};Version=3;");
            DbConnection.Open();
            DbHelper = new ImageDbOperation(DbConnection); // :contentReference[oaicite:19]{index=19}
            DbHelper.CreateTableIfNotExists();

            // Settings + PLC
            SettingsOperation = new SettingsOperation(DbConnection); // :contentReference[oaicite:20]{index=20}
            var plcPort = SettingsOperation.GetSetting<string>("plc", "serial_port");
            var baud = SettingsOperation.GetSetting<int>("plc", "baud_rate");
            PlcHelper = (!string.IsNullOrWhiteSpace(plcPort) && baud > 0) ? new PlcOperation(plcPort, baud) : null; // :contentReference[oaicite:21]{index=21}
            if (PlcHelper?.DeviceExists() == true) PlcHelper.Open();

            // YAML
            var def = YamlFlowLoader.Load("flowconfig.yaml");
            FlowValidator.Validate(def);

            // DI
            var services = new ServiceCollection();
            services.AddAppServices(DbConnection, PlcHelper);
            var provider = services.BuildServiceProvider();
            var registry = provider.GetRequiredService<WindowsFormsApp1.Domain.Flow.Engine.IActionRegistry>();
            provider.PopulateActionRegistry(); // registers “base” actions: PLC send pass/fail, camera, grpc, etc.

            // Host
            var host = new WindowsFormsApp1.Domain.Flow.Engine.StatelessHost(
                def,
                registry,
                provider.GetRequiredService<WindowsFormsApp1.Domain.Flow.Engine.IFlowContext>());

            // UI form
            var dashboard = new MainDashboard();

            // Register UI render action (needs form + delegate)
            //registry.Register(new WindowsFormsApp1.Domain.Actions.UiRenderLastResultAction(
            //    dashboard,
            //    json => dashboard.RenderComponentsUI(json, dashboard.flowLayoutPanel1)
            //));

            // Register PLC subscribe/unsubscribe actions (need the host-bound bus)
            var bus = new WindowsFormsApp1.Domain.Flow.Engine.HostTriggerBus(host);
            var plcSubSvc = provider.GetRequiredService<WindowsFormsApp1.Domain.Actions.PlcReadSubscription>();

            registry.Register(new WindowsFormsApp1.Domain.Actions.PlcSubscribeReadAction(plcSubSvc, bus));     // <-- Plc.SubscribeRead
            registry.Register(new WindowsFormsApp1.Domain.Actions.PlcUnsubscribeReadAction(plcSubSvc));        // <-- Plc.UnsubscribeRead

            // Now auto-triggers are safe to load and triggers are safe to fire
            host.LoadAutoTriggers(def.AutoTriggers);

            dashboard.Tag = host;
            Application.Run(dashboard);
        }

        static string GetDatabasePath()
        {
            var rawPath = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["DbPath"]);
            if (rawPath.StartsWith("~"))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                rawPath = Path.Combine(home, rawPath.Substring(1).TrimStart('\\', '/'));
            }
            Directory.CreateDirectory(Path.GetDirectoryName(rawPath));
            return rawPath;
        }
    }
}