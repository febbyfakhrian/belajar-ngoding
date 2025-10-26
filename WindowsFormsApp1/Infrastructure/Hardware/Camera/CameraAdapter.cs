using MvCamCtrl.NET;
using System;
using System.Drawing;
using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1.Infrastructure.Hardware.Camera
{
    /// <summary>
    /// Adapter for the third-party camera SDK
    /// </summary>
    public class CameraAdapter
    {
        private readonly MyCamera _camera;
        
        public CameraAdapter()
        {
            _camera = new MyCamera();
        }
        
        public int CreateDevice(ref MyCamera.MV_CC_DEVICE_INFO deviceInfo)
        {
            return _camera.MV_CC_CreateDevice_NET(ref deviceInfo);
        }
        
        public int OpenDevice()
        {
            return _camera.MV_CC_OpenDevice_NET();
        }
        
        public int CloseDevice()
        {
            return _camera.MV_CC_CloseDevice_NET();
        }
        
        public int DestroyDevice()
        {
            return _camera.MV_CC_DestroyDevice_NET();
        }
        
        public int StartGrabbing()
        {
            return _camera.MV_CC_StartGrabbing_NET();
        }
        
        public int StopGrabbing()
        {
            return _camera.MV_CC_StopGrabbing_NET();
        }
        
        public int SetEnumValue(string key, uint value)
        {
            return _camera.MV_CC_SetEnumValue_NET(key, value);
        }
    }
}