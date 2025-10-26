using System;

namespace WindowsFormsApp1.Core.Common.Exceptions
{
    public class GrpcException : Exception
    {
        public GrpcException() : base() { }
        
        public GrpcException(string message) : base(message) { }
        
        public GrpcException(string message, Exception innerException) : base(message, innerException) { }
    }
}