using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Andromeda.Framing.Extensions;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Tests.Helpers;
using Xunit;

namespace Andromeda.Framing.Tests.Metadata
{
    public class MetadataEncoderTests
    {
        private readonly IMetadataEncoder _encoder = new MessageMetadataParser();

        [Theory, InlineData(1, 128), InlineData(2, 1024), InlineData(3, 2048)]
        public async Task WriteFrameAsync_ShouldSerializeCorrectly(short messageId, int length)
        {
            var encodedFrame = FrameProvider.GetRandom(messageId, length);
            var metadata = new MessageMetadata(messageId, length);
            var frame = new Frame(encodedFrame.Slice(
                _encoder.GetLength(metadata)), metadata);

            var pipe = new Pipe();
            var writeResult = await _encoder.WriteFrameAsync(pipe.Writer, frame);
            Assert.True(!writeResult.IsCompleted && !writeResult.IsCanceled);

            pipe.Reader.TryRead(out var result);
            var expected = encodedFrame.ToArray();
            var actual = result.Buffer.ToArray();
            Assert.Equal(expected, actual);
        }
    }
}
