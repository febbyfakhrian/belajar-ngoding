using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.Core.Common.Configuration
{
    public interface IConfigurationNotifier
    {
        event Action<AppSettings> ConfigurationChanged;
        void NotifyConfigurationChanged(AppSettings newSettings);
    }
    
    public class ConfigurationNotifier : IConfigurationNotifier
    {
        private AppSettings _currentSettings;
        private readonly object _lock = new object();
        
        public event Action<AppSettings> ConfigurationChanged;
        
        public AppSettings CurrentSettings
        {
            get
            {
                lock (_lock)
                {
                    return _currentSettings?.Clone() ?? new AppSettings();
                }
            }
            private set
            {
                lock (_lock)
                {
                    _currentSettings = value?.Clone();
                }
            }
        }
        
        public ConfigurationNotifier(AppSettings initialSettings)
        {
            CurrentSettings = initialSettings ?? new AppSettings();
        }
        
        public void NotifyConfigurationChanged(AppSettings newSettings)
        {
            if (newSettings == null) return;
            
            bool settingsChanged = false;
            lock (_lock)
            {
                // Check if settings actually changed
                settingsChanged = !_currentSettings.Equals(newSettings);
                if (settingsChanged)
                {
                    _currentSettings = newSettings.Clone();
                }
            }
            
            if (settingsChanged)
            {
                ConfigurationChanged?.Invoke(newSettings.Clone());
            }
        }
    }
    
    // Extension methods for cloning settings
    public static class AppSettingsExtensions
    {
        public static AppSettings Clone(this AppSettings settings)
        {
            if (settings == null) return null;
            
            return new AppSettings
            {
                Plc = new PlcSettings
                {
                    SerialPort = settings.Plc.SerialPort,
                    BaudRate = settings.Plc.BaudRate,
                    DataBits = settings.Plc.DataBits,
                    Parity = settings.Plc.Parity,
                    StopBits = settings.Plc.StopBits
                },
                Camera = new CameraSettings
                {
                    Device = settings.Camera.Device,
                    AcquisitionMode = settings.Camera.AcquisitionMode,
                    TriggerMode = settings.Camera.TriggerMode
                },
                Grpc = new GrpcSettings
                {
                    Endpoint = settings.Grpc.Endpoint,
                    MaxSendMessageLength = settings.Grpc.MaxSendMessageLength,
                    MaxReceiveMessageLength = settings.Grpc.MaxReceiveMessageLength
                },
                Database = new DatabaseSettings
                {
                    ConnectionString = settings.Database.ConnectionString,
                    MaxPoolSize = settings.Database.MaxPoolSize
                }
            };
        }
    }
}