using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1.Presentation.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        
        private string _plcPort;
        private int _plcBaudRate;
        private string _cameraDevice;
        private string _grpcEndpoint;
        
        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadSettings();
        }
        
        public string PlcPort
        {
            get => _plcPort;
            set => SetField(ref _plcPort, value);
        }
        
        public int PlcBaudRate
        {
            get => _plcBaudRate;
            set => SetField(ref _plcBaudRate, value);
        }
        
        public string CameraDevice
        {
            get => _cameraDevice;
            set => SetField(ref _cameraDevice, value);
        }
        
        public string GrpcEndpoint
        {
            get => _grpcEndpoint;
            set => SetField(ref _grpcEndpoint, value);
        }
        
        public void LoadSettings()
        {
            PlcPort = _settingsService.GetSetting<string>("plc", "serial_port");
            PlcBaudRate = _settingsService.GetSetting<int>("plc", "baud_rate");
            CameraDevice = _settingsService.GetSetting<string>("camera", "device");
            GrpcEndpoint = _settingsService.GetSetting<string>("grpc", "endpoint");
        }
        
        public void SaveSettings()
        {
            _settingsService.SetSetting("plc", "serial_port", PlcPort);
            _settingsService.SetSetting("plc", "baud_rate", PlcBaudRate);
            _settingsService.SetSetting("camera", "device", CameraDevice);
            _settingsService.SetSetting("grpc", "endpoint", GrpcEndpoint);
        }
    }
}