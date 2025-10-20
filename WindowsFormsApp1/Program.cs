using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using WindowsFormsApp1;
using WindowsFormsApp1.Services;

namespace AutoInspectionPlatform
{
    static class Program
    {
        public static ImageDbOperation DbHelper;
        public static SQLiteConnection DbConnection;
        public static PlcOperation PlcHelper;
        private static SettingsOperation SettingsOperation;

        [STAThread]
        static void Main()
        {
            // Buka DB langsung dari file yang dipilih
            DbConnection = new SQLiteConnection($"Data Source={GetDatabasePath()};Version=3;");
            DbConnection.Open();

            DbHelper = new ImageDbOperation(DbConnection);
            DbHelper.CreateTableIfNotExists();

            SettingsOperation = new SettingsOperation(DbConnection);

            PlcHelper = new PlcOperation(SettingsOperation.GetSetting<string>("plc", "serial_port"), SettingsOperation.GetSetting<int>("plc", "baud_rate"));

            if (PlcHelper.IsOpen)
            {
                PlcHelper.Close();
            }

            // Buat instance baru (atau reuse) dan buka
            PlcHelper.Open();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainDashboard());
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