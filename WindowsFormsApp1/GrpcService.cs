using Api;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public sealed class GrpcService : IDisposable, IAsyncDisposable
    {
        private readonly string _host;
        private Channel _channel;
        private AutoInspect.AutoInspectClient _client;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private bool _isStopping;
        private bool _isDisposed;

        public event Action<string> BoxesReceived;
        public event Action OnDisconnected;

        public GrpcService(string host = "localhost:50052")
            => _host = host ?? throw new ArgumentNullException(nameof(host));

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

        public AsyncDuplexStreamingCall<ImageRequest, ImageResponse> CreateImageStream(CancellationToken ct = default(CancellationToken))
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

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            try
            {
                await StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Grpc] Error during async shutdown: {ex.Message}");
            }
            finally
            {
                try { _cts?.Dispose(); } catch { }
                _cts = null;
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
