using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Andromeda.Framing.Protocol;
using Protocol.Models;

namespace Protocol
{
    public class MessageReader : IMessageReader
    {
        public bool TryParse<T>(in ReadOnlySequence<byte> payload, T message)
        {
            if (message is HandshakeMessage handshake) {
                handshake.Message = payload.AsString();
                return true;
            }
            if (message is HelloServerMessage hello) {
                hello.Response = payload.AsString();
                return true;
            }
            return false;
        }
    }
}
