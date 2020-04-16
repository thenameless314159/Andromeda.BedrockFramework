using System.Buffers;

namespace Andromeda.Framing.Metadata
{
    public interface IMetadataDecoder
    {
        bool TryParse(ref SequenceReader<byte> input, out IMessageMetadata metadata);
    }
}
