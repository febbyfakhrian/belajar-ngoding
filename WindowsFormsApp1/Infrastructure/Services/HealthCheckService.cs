using System;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1.Infrastructure.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ICameraService _cameraService;
        private readonly IPlcService _plcService;
        private readonly IGrpcService _grpcService;
        
        public HealthCheckService(ICameraService cameraService, IPlcService plcService, IGrpcService grpcService)
        {
            _cameraService = cameraService;
            _plcService = plcService;
            _grpcService = grpcService;
        }
        
        public Task<bool> CheckCameraHealthAsync()
        {
            try
            {
                // Check if camera SDK is available
                return Task.FromResult(_cameraService.IsCameraSdkAvailable);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
        
        public Task<bool> CheckPlcHealthAsync()
        {
            try
            {
                // Check if PLC is connected
                return Task.FromResult(_plcService.IsOpen);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
        
        public Task<bool> CheckGrpcHealthAsync()
        {
            try
            {
                // For now, we'll just return true as we don't have a specific health check method
                // In a real implementation, you would call a health check endpoint
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
        
        public async Task<HealthStatus> GetOverallHealthStatusAsync()
        {
            var cameraHealthy = await CheckCameraHealthAsync();
            var plcHealthy = await CheckPlcHealthAsync();
            var grpcHealthy = await CheckGrpcHealthAsync();
            
            return new HealthStatus
            {
                CameraHealthy = cameraHealthy,
                PlcHealthy = plcHealthy,
                GrpcHealthy = grpcHealthy,
                StatusMessage = $"Camera: {(cameraHealthy ? "OK" : "ERROR")}, " +
                               $"PLC: {(plcHealthy ? "OK" : "ERROR")}, " +
                               $"gRPC: {(grpcHealthy ? "OK" : "ERROR")}"
            };
        }
    }
}