using System;
using System.Collections.Concurrent;
using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1.Infrastructure.Hardware.Camera
{
    public interface ICameraServiceFactory
    {
        ICameraService CreateCameraService();
    }
    
    public class CameraServiceFactory : ICameraServiceFactory
    {
        public ICameraService CreateCameraService()
        {
            return new CameraManager(new ConcurrentQueue<byte[]>());
        }
    }
}