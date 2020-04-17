using System;
using System.Buffers;
using Andromeda.Framing.Extensions.Infrastructure;
using Andromeda.Framing.Metadata;

namespace Andromeda.Framing.Protocol
{
    public abstract class MessageAsFrameWriter : IMessageWriter
    {
        private readonly IMetadataEncoder _metaEncoder;

        protected MessageAsFrameWriter(IMetadataEncoder metaEncoder) =>
            _metaEncoder = metaEncoder;

        protected abstract IMessageMetadata MessageAsMetadata<T>(in T message);
        protected abstract void WriteMessageAsPayload<T>(IBufferWriter<byte> writer, in T message);

        public void Encode<T>(IBufferWriter<byte> writer, in T message)
        {
            var metadata = MessageAsMetadata(in message);
            if(metadata == null) throw new ArgumentException("Invalid message provided !", typeof(T).GetTypeName());
            var metaLength = _metaEncoder.GetLength(metadata);
            var metaSpan = writer.GetSpan(metaLength);
            _metaEncoder.Write(metaSpan, metadata);
            writer.Advance(metaLength);

            WriteMessageAsPayload(writer, in message);
        }
    }
}
