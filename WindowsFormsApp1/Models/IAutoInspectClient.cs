using Api;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Models
{
    public interface IAutoInspectClient
    {
        Task<ImageResponse> ProcessImageAsync(ImageRequest req, CancellationToken ct = default);
        AsyncDuplexStreamingCall<ImageRequest, ImageResponse> ProcessImageStream(CancellationToken ct = default);
        Task<DefaultResponse> UpdateConfigAsync(ConfigRequest req, CancellationToken ct = default);
    }
}
