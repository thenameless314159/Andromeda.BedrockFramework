using Andromeda.Framing.Metadata;

namespace Andromeda.Framing.Extensions.Tests.Infrastructure
{
    public class TestMessageMetadata : MessageMetadataWithId
    {
        public override int MessageId { get; }
        public override int Length { get; }

        public TestMessageMetadata(int messageId, int length)
        {
            MessageId = messageId;
            Length = length;
        }
    }
}
