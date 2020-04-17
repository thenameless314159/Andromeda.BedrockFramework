using System.Buffers;

namespace Andromeda.Framing.Protocol
{
    public interface IMessageReader
    {
        bool TryParse<T>(in ReadOnlySequence<byte> payload, T message);
    }
}
