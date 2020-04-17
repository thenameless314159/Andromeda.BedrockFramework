using System.Threading.Tasks;

namespace Andromeda.Framing
{
    public interface IMessageEncoder : IFrameEncoder
    {
        ValueTask WriteAsync<T>(in T message);
    }
}
