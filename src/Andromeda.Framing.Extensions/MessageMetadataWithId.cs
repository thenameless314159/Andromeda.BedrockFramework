namespace Andromeda.Framing.Metadata
{
    public abstract class MessageMetadataWithId : IMessageMetadata
    {
        public abstract int MessageId { get; }
        public abstract int Length { get; }
    }
}
