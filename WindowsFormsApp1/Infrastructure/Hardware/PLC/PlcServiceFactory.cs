using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1.Infrastructure.Hardware.PLC
{
    public interface IPlcServiceFactory
    {
        IPlcService CreatePlcService(string portName, int baudRate);
    }
    
    public class PlcServiceFactory : IPlcServiceFactory
    {
        public IPlcService CreatePlcService(string portName, int baudRate)
        {
            return new PlcOperation(portName, baudRate);
        }
    }
}