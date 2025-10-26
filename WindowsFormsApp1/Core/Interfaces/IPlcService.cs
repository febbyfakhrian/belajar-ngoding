using System;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Core.Interfaces
{
    /// <summary>
    /// Interface for PLC (Programmable Logic Controller) service operations
    /// </summary>
    public interface IPlcService : IDisposable
    {
        /// <summary>
        /// Event triggered when a line is received from the PLC
        /// </summary>
        event Action<string> LineReceived;
        
        /// <summary>
        /// Gets a value indicating whether the PLC connection is open
        /// </summary>
        bool IsOpen { get; }
        
        /// <summary>
        /// Opens the PLC connection asynchronously
        /// </summary>
        /// <returns>True if the connection was opened successfully, false otherwise</returns>
        Task<bool> OpenAsync();
        
        /// <summary>
        /// Closes the PLC connection asynchronously
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task CloseAsync();
        
        /// <summary>
        /// Sends a command to the PLC asynchronously
        /// </summary>
        /// <param name="command">The command bytes to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendCommandAsync(byte[] command);
        
        /// <summary>
        /// Reads data from the PLC asynchronously
        /// </summary>
        /// <returns>The data read from the PLC</returns>
        Task<string> ReadDataAsync();
    }
}