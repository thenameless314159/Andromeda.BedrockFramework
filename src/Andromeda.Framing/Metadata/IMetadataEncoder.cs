using System;

namespace Andromeda.Framing.Metadata
{
    public interface IMetadataEncoder
    {
        int GetLength(IMessageMetadata metadata);
        void Write(Span<byte> span, IMessageMetadata metadata);
    }
}
