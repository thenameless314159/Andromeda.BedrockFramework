using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Linq;
using Andromeda.Framing.Extensions;
using Andromeda.Framing.Memory;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Tests.Helpers;
using Xunit;

namespace Andromeda.Framing.Tests.Metadata
{
    public class MetadataDecoderTests
    {
        private readonly IMetadataDecoder _decoder = new MessageMetadataParser();
        
        [Theory, 
         InlineData(1, 2, 16), 
         InlineData(2, 4, 32, 1024),
         InlineData(3, 8, 16, 4096, 8192)]
        public void GetFramesOf_ShouldParseAll_OnValidFrame(int messageId, params int[] framesLength)
        {
            var framesBuffer = FrameProvider.GetMultiplesRandom(messageId, framesLength);
            var frames = _decoder.GetFramesOf(new SequenceHolder(framesBuffer)).ToArray();

            Assert.Equal(framesLength.Length, frames.Length);
            Assert.All(frames, f => Assert.Equal(f.Metadata.GetMessageId(), messageId));
            Assert.All(framesLength, len => Assert.Contains(frames, f => f.Metadata.Length == len));
        }

        [Fact]
        public void GetFramesOf_ShouldThrow_OnNegativeMetaLength()
        {
            Memory<byte> buffer = new byte[6];
            BinaryPrimitives.WriteInt16BigEndian(buffer.Span, 5);
            BinaryPrimitives.WriteInt32BigEndian(buffer.Span.Slice(2), int.MinValue);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _decoder.GetFramesOf(new SequenceHolder(new ReadOnlySequence<byte>(buffer)))
                    .ToArray());
        }

        [Fact]
        public void GetFramesOf_ShouldBreak_OnValidButIncompletePayload()
        {
            Memory<byte> buffer = new byte[16];
            BinaryPrimitives.WriteInt16BigEndian(buffer.Span, 5);
            BinaryPrimitives.WriteInt32BigEndian(buffer.Span.Slice(2), 16);
            var holder = new SequenceHolder(new ReadOnlySequence<byte>(buffer));
            var frames = _decoder.GetFramesOf(holder)
                .ToArray();

            Assert.Empty(frames);
        }
        
        [Fact]
        public void GetFramesOf_ShouldBreak_OnOneAndHalfFrame()
        {
            var buffer = FrameProvider.GetMultiplesRandom(5, 4, 6).Slice(0, 14);
            var holder = new SequenceHolder(buffer);
            var frames = _decoder.GetFramesOf(holder)
                .ToArray();

            Assert.Single(frames); 
            Assert.Equal(buffer.GetPosition(10), holder.Buffer.Start);
        }
    }
}
