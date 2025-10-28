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
            // Create a mock settings service that provides the endpoint
            var mockSettings = new MockGrpcSettingsService(endpoint);
            return new GrpcService(mockSettings);
        }
        
        // Mock settings service for creating services with a specific endpoint
        private class MockGrpcSettingsService : ISettingsService
        {
            private readonly string _endpoint;
            
            public MockGrpcSettingsService(string endpoint)
            {
                _endpoint = endpoint;
            }
            
            public T GetSetting<T>(string groupName, string key)
            {
                if (groupName == "grpc" && key == "server_url" && typeof(T) == typeof(string))
                {
                    return (T)(object)_endpoint;
                }
                return default(T);
            }
            
            public void SetSetting(string groupName, string key, object value)
            {
                // No-op for mock service
            }
        }
    }
}