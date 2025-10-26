using System;

namespace WindowsFormsApp1.Core.Common.Exceptions
{
    public class CameraException : Exception
    {
        public CameraException() : base() { }
        
        public CameraException(string message) : base(message) { }
        
        public CameraException(string message, Exception innerException) : base(message, innerException) { }
    }
}