using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Protocol;
using Protocol.Models;

namespace Protocol
{
    public class MessageWriter : MessageAsFrameWriter
    {
        public MessageWriter(IMetadataEncoder metaEncoder) : base(metaEncoder)
        {
        }

        protected override IMessageMetadata MessageAsMetadata<T>(in T message)
        {
            if(!(message is IMessage msg)) 
                throw new InvalidOperationException();
                
            return new MessageMetadata(msg.MessageId, msg.GetLength());
        }

        protected override void WriteMessageAsPayload<T>(IBufferWriter<byte> writer, in T message)
        {
            if(message is HandshakeMessage handshake)
                WriteUtf(writer, handshake.Message);
            if(message is HelloServerMessage hello)
                WriteUtf(writer, hello.Response);
        }

        private static void WriteUtf(IBufferWriter<byte> writer, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var bytes = Encoding.UTF8.GetBytes(value);
            var span = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);

            writer.Advance(bytes.Length);
        }
    }
}
