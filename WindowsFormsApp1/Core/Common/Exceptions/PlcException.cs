using System;

namespace WindowsFormsApp1.Core.Common.Exceptions
{
    public class PlcException : Exception
    {
        public PlcException() : base() { }
        
        public PlcException(string message) : base(message) { }
        
        public PlcException(string message, Exception innerException) : base(message, innerException) { }
    }
}