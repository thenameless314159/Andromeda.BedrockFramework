using System;
using System.Buffers;

namespace Andromeda.Framing.Protocol
{
    public interface IMessageWriter
    {
        void Encode<T>(IBufferWriter<byte> writer, in T message);
    }
}
