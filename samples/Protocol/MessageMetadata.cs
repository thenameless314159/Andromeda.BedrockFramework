using System;
using System.Collections.Generic;
using System.Text;
using Andromeda.Framing.Metadata;

namespace Protocol
{
    public class MessageMetadata : MessageMetadataWithId
    {
        public override int MessageId { get; }
        public override int Length { get; }

        public MessageMetadata(int messageId, int length)
        {
            MessageId = messageId;
            Length = length;
        }

        public override string ToString() => $"MessageId={MessageId}, PayloadLength={Length}";
    }
}
