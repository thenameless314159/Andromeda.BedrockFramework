using Microsoft.AspNetCore.Connections;

namespace Andromeda.Framing
{
    public interface IFrameDecoderFactory
    {
        IFrameDecoder Create(ConnectionContext fromContext);
    }
}
