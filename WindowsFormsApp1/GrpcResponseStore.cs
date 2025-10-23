using Api;

namespace WindowsFormsApp1
{
    class GrpcResponseStore
    {
        public static ImageResponse LastResponse { get; set; } = new ImageResponse();
    }
}
