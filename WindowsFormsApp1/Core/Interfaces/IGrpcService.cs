using Api;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Entities.Models;

namespace WindowsFormsApp1.Core.Interfaces
{
    /// <summary>
    /// Interface for gRPC service operations
    /// </summary>
    public interface IGrpcService : IDisposable
    {
        /// <summary>
        /// Event triggered when boxes are received from the gRPC service
        /// </summary>
        event Action<string> BoxesReceived;
        
        /// <summary>
        /// Event triggered when the gRPC service is disconnected
        /// </summary>
        event Action OnDisconnected;
        
        /// <summary>
        /// Gets a value indicating whether the gRPC service is connected
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// Starts the gRPC service asynchronously
        /// </summary>
        /// <returns>True if the service started successfully, false otherwise</returns>
        Task<bool> StartAsync();
        
        /// <summary>
        /// Processes an image via gRPC asynchronously
        /// </summary>
        /// <param name="jpeg">The JPEG image data to process</param>
        /// <param name="ct">Cancellation token</param>
        /// <param name="imageId">Optional image identifier</param>
        /// <returns>The image response from the gRPC service</returns>
        Task<ImageResponse> ProcessImageAsync(byte[] jpeg, CancellationToken ct = default, string imageId = "");
        
        /// <summary>
        /// Processes a stream of images via gRPC asynchronously
        /// </summary>
        /// <param name="images">The images to process</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The image response from the gRPC service</returns>
        Task<ImageResponse> ProcessImageStreamAsync(IEnumerable<byte[]> images, CancellationToken ct = default);
        
        /// <summary>
        /// Updates the gRPC service configuration asynchronously
        /// </summary>
        /// <param name="path">The path to the configuration file</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The default response from the gRPC service</returns>
        Task<DefaultResponse> UpdateConfigAsync(string path, CancellationToken ct = default);
        
        /// <summary>
        /// Stops the gRPC service asynchronously
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task StopAsync();
    }
}