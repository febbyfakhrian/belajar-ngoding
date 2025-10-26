using System;
using System.Drawing;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Core.Interfaces
{
    /// <summary>
    /// Interface for camera service operations
    /// </summary>
    public interface ICameraService : IDisposable
    {
        /// <summary>
        /// Event triggered when a frame is ready for gRPC processing
        /// </summary>
        event Action<byte[]> FrameReadyForGrpc;
        
        /// <summary>
        /// Event triggered when an error occurs
        /// </summary>
        event Action<string> Error;
        
        /// <summary>
        /// Event triggered when a frame is ready for preview
        /// </summary>
        event Action<Bitmap> FramePreview;
        
        /// <summary>
        /// Gets or sets the display handle for camera preview
        /// </summary>
        IntPtr DisplayHandle { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether the camera SDK is available
        /// </summary>
        bool IsCameraSdkAvailable { get; }
        
        /// <summary>
        /// Opens the camera connection asynchronously
        /// </summary>
        /// <returns>True if the camera was opened successfully, false otherwise</returns>
        Task<bool> OpenAsync();
        
        /// <summary>
        /// Starts the camera capture process asynchronously
        /// </summary>
        /// <returns>The result code from the camera SDK</returns>
        Task<int> StartAsync();
        
        /// <summary>
        /// Stops the camera capture process asynchronously
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task StopAsync();
        
        /// <summary>
        /// Closes the camera connection asynchronously
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task CloseAsync();
    }
}