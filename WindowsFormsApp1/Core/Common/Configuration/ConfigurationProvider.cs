using System;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Core.Common.Configuration
{
    public interface IConfigurationProvider
    {
        AppSettings GetAppSettings();
        void SaveAppSettings(AppSettings settings);
    }
    
    public class ConfigurationProvider : IConfigurationProvider
    {
        private const string ConfigFileName = "appsettings.json";
        
        public AppSettings GetAppSettings()
        {
            // Try to load from JSON file first
            if (File.Exists(ConfigFileName))
            {
                try
                {
                    var json = File.ReadAllText(ConfigFileName);
                    return JsonConvert.DeserializeObject<AppSettings>(json);
                }
                catch (Exception ex)
                {
                    // Log error and fall back to defaults
                    Console.WriteLine($"Error loading configuration from file: {ex.Message}");
                }
            }
            
            // Fall back to app.config or defaults
            return LoadFromAppConfig();
        }
        
        public void SaveAppSettings(AppSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(ConfigFileName, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration to file: {ex.Message}");
                throw;
            }
        }
        
        private AppSettings LoadFromAppConfig()
        {
            var settings = new AppSettings();
            
            try
            {
                // Load from app.config if available
                settings.Plc.SerialPort = ConfigurationManager.AppSettings["PlcSerialPort"] ?? settings.Plc.SerialPort;
                settings.Plc.BaudRate = int.TryParse(ConfigurationManager.AppSettings["PlcBaudRate"], out int baud) ? baud : settings.Plc.BaudRate;
                settings.Grpc.Endpoint = ConfigurationManager.AppSettings["GrpcEndpoint"] ?? settings.Grpc.Endpoint;
                settings.Database.ConnectionString = ConfigurationManager.AppSettings["DbPath"] ?? settings.Database.ConnectionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration from app.config: {ex.Message}");
            }
            
            return settings;
        }
    }
}