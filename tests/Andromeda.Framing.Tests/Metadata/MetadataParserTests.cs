using System.Buffers;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Tests.Helpers;
using Xunit;

namespace Andromeda.Framing.Tests.Metadata
{
    public class MetadataParserTests
    {
        private readonly IMetadataParser _parser = new MessageMetadataParser();
        
        [Theory, InlineData(1,8), InlineData(2, 64), InlineData(3, 4096)]
        public void TryParse_ShouldReturnTrue_OnValidFrame(short messageId, int length)
        {
            var frame = FrameProvider.GetRandom(messageId, length);
            var reader = new SequenceReader<byte>(frame);
            var couldParse = _parser.TryParse(ref reader, out var metadata);

            Assert.True(couldParse);
            Assert.NotNull(metadata);
            Assert.Equal(metadata.Length, length);
            Assert.Equal(metadata.GetMessageId(), messageId);
        }
        
        [Theory, InlineData(1, 8), InlineData(2, 64), InlineData(3, 4096)]
        public void Write_ShouldSerializeCorrectly(short messageId, int length)
        {
            var metadata = FrameProvider.GetRandom(messageId, length)
                .Slice(0, 6).ToArray();

            var meta = new MessageMetadata(messageId, length);
            var writtenMetadata = new byte[_parser.GetLength(meta)];
            _parser.Write(writtenMetadata, meta);

            Assert.Equal(metadata, writtenMetadata);
        }

        [Fact]
        public void TryParse_ShouldReturnFalse_OnIncompleteMetadata()
        {
            var frame = FrameProvider.GetRandom(1, 5);
            var reader = new SequenceReader<byte>(frame.Slice(0, 5));
            var couldParse = _parser.TryParse(ref reader, out _);
            Assert.False(couldParse);
        }

        
    }
}
