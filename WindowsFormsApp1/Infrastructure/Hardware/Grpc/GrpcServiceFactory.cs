using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1.Infrastructure.Hardware.Grpc
{
    public interface IGrpcServiceFactory
    {
        IGrpcService CreateGrpcService(string endpoint);
    }
    
    public class GrpcServiceFactory : IGrpcServiceFactory
    {
        public IGrpcService CreateGrpcService(string endpoint)
        {
            return new GrpcService(endpoint);
        }
    }
}