using System;
using System.Buffers;
using System.Buffers.Binary;
using Andromeda.Framing.Metadata;

namespace Andromeda.Framing.Tests.Metadata
{
    public class MessageMetadataParser : MetadataParser<MessageMetadata>
    {
        protected override bool TryParse(ref SequenceReader<byte> input, out MessageMetadata metadata)
        {
            metadata = default;
            if (!input.TryReadBigEndian(out short messageId)) return false;
            if (!input.TryReadBigEndian(out int length)) return false;
            metadata = new MessageMetadata(messageId, length);
            return true;
        }

        protected override void Write(Span<byte> span, MessageMetadata metadata)
        {
            BinaryPrimitives.WriteInt16BigEndian(span, (short)metadata.MessageId);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(2), metadata.Length);
        }

        protected override int GetLength(MessageMetadata metadata) => 
            /*messageId:*/sizeof(short) + /*length:*/ sizeof(int);
    }
}
