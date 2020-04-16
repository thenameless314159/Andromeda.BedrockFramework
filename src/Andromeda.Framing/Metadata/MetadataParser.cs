using System;
using System.Buffers;

namespace Andromeda.Framing.Metadata
{
    public abstract class MetadataParser<TMeta> : IMetadataParser
        where TMeta : class, IMessageMetadata
    {
        public bool TryParse(ref SequenceReader<byte> input, out IMessageMetadata metadata)
        {
            if (!TryParse(ref input, out TMeta meta))
            {
                metadata = default;
                return false;
            }

            metadata = meta;
            return true;
        }

        public void Write(Span<byte> span, IMessageMetadata metadata) => Write(span, (TMeta) metadata);
        public int GetLength(IMessageMetadata metadata) => GetLength((TMeta)metadata);

        protected abstract bool TryParse(ref SequenceReader<byte> input, out TMeta metadata);
        protected abstract void Write(Span<byte> span, TMeta metadata);
        protected abstract int GetLength(TMeta metadata);
    }
}
