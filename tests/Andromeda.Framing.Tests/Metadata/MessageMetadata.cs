using System;
using Andromeda.Framing.Metadata;

namespace Andromeda.Framing.Tests.Metadata
{
    public class MessageMetadata : IMessageMetadata
    {
        public int MessageId { get; }
        public int Length { get; }

        public MessageMetadata(int messageId, int length)
        {
            MessageId = messageId;
            Length = length;
        }

        public MessageMetadata()
        {
        }

        public override bool Equals(object obj) => obj != null
                                                   && obj is MessageMetadata meta
                                                   && meta.MessageId == MessageId
                                                   && meta.Length == Length;

        protected bool Equals(MessageMetadata other) => MessageId == other.MessageId && Length == other.Length;
        public override int GetHashCode() => HashCode.Combine(MessageId, Length);
    }

    internal static class MessageMetadataExtensions
    {
        public static int GetMessageId(this IMessageMetadata m) => ((MessageMetadata) m).MessageId;
    }
}
