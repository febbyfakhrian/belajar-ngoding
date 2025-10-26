namespace WindowsFormsApp1.Core.Common.Configuration
{
    public class AppSettings
    {
        public PlcSettings Plc { get; set; } = new PlcSettings();
        public CameraSettings Camera { get; set; } = new CameraSettings();
        public GrpcSettings Grpc { get; set; } = new GrpcSettings();
        public DatabaseSettings Database { get; set; } = new DatabaseSettings();
    }
    
    public class PlcSettings
    {
        public string SerialPort { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public string Parity { get; set; } = "None";
        public string StopBits { get; set; } = "One";
    }
    
    public class CameraSettings
    {
        public string Device { get; set; } = "";
        public string AcquisitionMode { get; set; } = "Continuous";
        public string TriggerMode { get; set; } = "Off";
    }
    
    public class GrpcSettings
    {
        public string Endpoint { get; set; } = "localhost:50052";
        public int MaxSendMessageLength { get; set; } = 104857600; // 100 MB
        public int MaxReceiveMessageLength { get; set; } = 104857600; // 100 MB
    }
    
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = "Data Source=app.db;Version=3;";
        public int MaxPoolSize { get; set; } = 100;
    }
}