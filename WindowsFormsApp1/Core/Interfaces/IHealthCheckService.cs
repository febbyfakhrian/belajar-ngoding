using System.Threading.Tasks;

namespace WindowsFormsApp1.Core.Interfaces
{
    public interface IHealthCheckService
    {
        Task<bool> CheckCameraHealthAsync();
        Task<bool> CheckPlcHealthAsync();
        Task<bool> CheckGrpcHealthAsync();
        Task<HealthStatus> GetOverallHealthStatusAsync();
    }
    
    public class HealthStatus
    {
        public bool CameraHealthy { get; set; }
        public bool PlcHealthy { get; set; }
        public bool GrpcHealthy { get; set; }
        public string StatusMessage { get; set; }
    }
}