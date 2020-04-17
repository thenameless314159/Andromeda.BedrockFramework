using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace Andromeda.Framing
{
    public interface IFrameEncoderFactory
    {
        IFrameEncoder Create(ConnectionContext fromContext);
    }
}
