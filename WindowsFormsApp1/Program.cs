using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using WindowsFormsApp1;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Services;
using WindowsFormsApp1.Services.StateMachine;
using WorkflowCore.Interface;
using WorkflowCore.Services.DefinitionStorage;
using YamlDotNet.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Google.Protobuf;
using System.Threading;

namespace AutoInspectionPlatform
{
    static class Program
    {
        public static ImageDbOperation DbHelper;
        public static SQLiteConnection DbConnection;
        public static PlcOperation PlcHelper;
        private static SettingsOperation SettingsOperation;

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
            // Buka DB langsung dari file yang dipilih
            DbConnection = new SQLiteConnection($"Data Source={GetDatabasePath()};Version=3;");
            DbConnection.Open();

            DbHelper = new ImageDbOperation(DbConnection);
            DbHelper.CreateTableIfNotExists();

            SettingsOperation = new SettingsOperation(DbConnection);

            try
            {
                string plcPort = SettingsOperation.GetSetting<string>("plc", "serial_port");
                int baudRate = SettingsOperation.GetSetting<int>("plc", "baud_rate");

                // Cek apakah setting PLC valid
                if (!string.IsNullOrWhiteSpace(plcPort) && baudRate > 0)
                {
                    PlcHelper = new PlcOperation(plcPort, baudRate);

                    // Coba tes koneksi ringan tanpa langsung Open()
                    if (PlcHelper.DeviceExists())
                    {
                        PlcHelper.Open();
                        Console.WriteLine("PLC terhubung dan berhasil dibuka.");
                    }
                    else
                    {
                        Console.WriteLine("PLC tidak terdeteksi, aplikasi akan tetap berjalan tanpa PLC.");
                        PlcHelper = null;
                    }
                }
                else
                {
                    PlcHelper = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gagal inisialisasi PLC: {ex.Message}");
                PlcHelper = null;
            }


            var yaml = File.ReadAllText("flowconfig.yaml");
            var cfg = new DeserializerBuilder()
                           .Build()
                           .Deserialize<FlowConfig>(yaml);
            object node = new CheckSignalReadNode();

            var engine = new FlowEngine(cfg, node);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainDashboard(engine));
        }

        public static string GetDatabasePath()
        {
            string rawPath = ConfigurationManager.AppSettings["DbPath"];

            // 1️  Ekspansi environment variable seperti %USERPROFILE%, %APPDATA%, dsb
            rawPath = Environment.ExpandEnvironmentVariables(rawPath);

            // 2️ Ekspansi simbol ~ (misalnya "~/Desktop/data.db")
            if (rawPath.StartsWith("~"))
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string relativePart = rawPath.Substring(1).TrimStart('/', '\\');
                rawPath = Path.Combine(home, relativePart);
            }

            // 3️  Pastikan direktori tempat file-nya ada
            string dir = Path.GetDirectoryName(rawPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return rawPath;
        }
    }
}