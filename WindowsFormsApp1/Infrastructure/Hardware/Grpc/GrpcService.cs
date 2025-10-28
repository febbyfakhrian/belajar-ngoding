using Api;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Core.Interfaces;
using WindowsFormsApp1.Infrastructure.Data;

namespace WindowsFormsApp1.Infrastructure.Hardware.Grpc
{
    public sealed class GrpcService : IGrpcService, IDisposable
    {
        private readonly string _host;
        private Channel _channel;
        private AutoInspect.AutoInspectClient _client;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private bool _isStopping;
        private bool _isDisposed;

        public event Action<string> BoxesReceived;
        public event Action OnDisconnected;

        // Constructor that accepts ISettingsService and retrieves host from settings
        public GrpcService(ISettingsService settingsService)
        {
            // Try to get the gRPC server URL from settings
            string host = "localhost:50052"; // Default fallback
            
            if (settingsService != null)
            {
                try
                {
                    var savedHost = settingsService.GetSetting<string>("grpc", "server_url");
                    if (!string.IsNullOrEmpty(savedHost))
                    {
                        host = savedHost;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Grpc] Error retrieving settings: {ex.Message}");
                    // Fall back to default host
                }
            }
            
            _host = host ?? "localhost:50052";
        }

        // Parameterless constructor for backward compatibility - uses default settings
        public GrpcService() : this(GetDefaultSettingsService())
        {
        }

        // Helper method to get a default settings service
        private static ISettingsService GetDefaultSettingsService()
        {
            try
            {
                // This would typically be resolved through DI in a real application
                // For backward compatibility, we'll return null which will cause it to use the default host
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> StartAsync()
        {
            if (_channel != null) return true;

            try
            {
                var options = new List<ChannelOption>
                {
                    new ChannelOption(ChannelOptions.MaxSendMessageLength, 100 * 1024 * 1024), // 100 MB
                    new ChannelOption(ChannelOptions.MaxReceiveMessageLength, 100 * 1024 * 1024)
                };
                _channel = new Channel(_host, ChannelCredentials.Insecure, options);
                await _channel.ConnectAsync(DateTime.UtcNow.AddSeconds(5)); // Wait until channel is ready
                _client = new AutoInspect.AutoInspectClient(_channel);
                Console.WriteLine("[Grpc] Connection SUCCESS.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Grpc] Connect FAILED: {ex.Message}");
                if (_channel != null)
                {
                    try { await _channel.ShutdownAsync(); } catch { }
                    _channel = null;
                }
                return false;
            }
        }

        public async Task<ImageResponse> ProcessImageAsync(byte[] jpeg, CancellationToken ct = default, string imageId = "")
        {
            if (_client == null) throw new InvalidOperationException("Not connected");
            var req = new ImageRequest
            {
                ImageId = imageId ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                ImageData = ByteString.CopyFrom(jpeg)
            };
            var response = await _client.ProcessImageAsync(req, cancellationToken: ct);
            GrpcResponseStore.LastResponse = response;
            return response;
        }

        public async Task<ImageResponse> ProcessImageStreamAsync(IEnumerable<byte[]> images, CancellationToken ct = default)
        {   
            if (_client == null) throw new InvalidOperationException("Not connected");

            var call = _client.ProcessImageStream(cancellationToken: ct);

            foreach (var jpeg in images)
            {
                await call.RequestStream.WriteAsync(new ImageRequest
                {
                    ImageId = Guid.NewGuid().ToString(),
                    ImageData = ByteString.CopyFrom(jpeg)
                });
            }
            await call.RequestStream.CompleteAsync();

            ImageResponse response = null;
            while (await call.ResponseStream.MoveNext())
            {
                response = call.ResponseStream.Current;

                // store globally
                GrpcResponseStore.LastResponse = response;

                // optional: trigger event
                BoxesReceived?.Invoke(response.Result);
            }

            return response ?? new ImageResponse();
        }

        public AsyncDuplexStreamingCall<ImageRequest, ImageResponse> ProcessImageStream(CancellationToken ct = default(CancellationToken))
        {
            if (_client == null)
                throw new InvalidOperationException("Not connected");

            return _client.ProcessImageStream(cancellationToken: ct);
        }


        public async Task<DefaultResponse> UpdateConfigAsync(string path, CancellationToken ct = default)
        {
            if (_client == null) throw new InvalidOperationException("Not connected");
            return await _client.UpdateConfigAsync(
                new ConfigRequest { ConfigPath = path },
                cancellationToken: ct);
        }

        public async Task StopAsync()
        {
            if (_isStopping) return;
            _isStopping = true;

            try
            {
                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    try { _cts.Cancel(); }
                    catch (ObjectDisposedException) { }
                }

                if (_channel != null)
                {
                    try
                    {
                        Console.WriteLine("[Grpc] Shutting down channel...");
                        await _channel.ShutdownAsync();
                    }
                    catch (ObjectDisposedException) { }
                    finally
                    {
                        _channel = null;
                    }
                }
            }
            finally
            {
                _isStopping = false;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            try
            {
                Task.Run(async () => await StopAsync()).Wait(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Grpc] Error during shutdown: {ex.Message}");
            }
            finally
            {
                try { _cts?.Dispose(); } catch { }
                _cts = null;
            }
        }
    }
}