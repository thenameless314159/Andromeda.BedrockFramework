using System.Buffers;
using Andromeda.Framing.Extensions.Tests.Infrastructure.Models;
using Andromeda.Framing.Protocol;

namespace Andromeda.Framing.Extensions.Tests.Infrastructure
{
    public class TestMessageReader : IMessageReader
    {
        public bool TryParse<T>(in ReadOnlySequence<byte> payload, T message)
        {
            if (message is EmptyMessage) return true;
            if (!(message is HandshakeMessage handshake)) return false;
            handshake.Message = payload.AsString();
            return true;

        }
    }
}
