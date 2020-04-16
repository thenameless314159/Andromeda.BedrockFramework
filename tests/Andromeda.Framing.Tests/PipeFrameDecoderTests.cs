using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Andromeda.Framing.Extensions;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Tests.Helpers;
using Andromeda.Framing.Tests.Metadata;
using Xunit;

namespace Andromeda.Framing.Tests
{
    public class PipeFrameDecoderTests
    {
        [Theory,
         InlineData(1, 2, 16),
         InlineData(2, 4, 32, 1024),
         InlineData(3, 8, 16, 4096, 8192)]
        public async Task ReadFramesAsync_ShouldParseAll_OnValidFrame(short messageId, params int[] framesLength)
        {
            var random = new Random();
            var (decoder, pipe) = CreateConsumer();
            var enumerator = decoder.ReadFramesAsync().GetAsyncEnumerator();
            foreach (var len in framesLength)
            {
                var meta = new MessageMetadata(messageId, len);
                var encoded = FrameProvider.GetRandomAsBuffer(messageId, len, random);
                await pipe.Writer.WriteAsync(encoded).ConfigureAwait(false);

                Assert.True(await enumerator.MoveNextAsync());
                Assert.Equal(meta, enumerator.Current.Metadata);
                Assert.Equal(encoded.Slice(_parser.GetLength(meta)).ToArray(), 
                    enumerator.Current.Payload.ToArray());
            }

            decoder.Dispose(); 
            Assert.False(await enumerator.MoveNextAsync()); // will break since tryAdvance returns false whenever the pipe is default
        }

        [Theory, InlineData(1, 4, 32, 1024)]
        public async Task ReadFramesAsync_ShouldBreak_OnReaderCompletedAtLast(short messageId, params int[] framesLength)
        {
            var random = new Random();
            var (decoder, pipe) = CreateConsumer();
            var enumerator = decoder.ReadFramesAsync().GetAsyncEnumerator();
            foreach (var len in framesLength)
            {
                var meta = new MessageMetadata(messageId, len);
                var encoded = FrameProvider.GetRandomAsBuffer(messageId, len, random);
                await pipe.Writer.WriteAsync(encoded).ConfigureAwait(false);

                Assert.True(await enumerator.MoveNextAsync());
                Assert.Equal(meta, enumerator.Current.Metadata);
                Assert.Equal(encoded.Slice(_parser.GetLength(meta)).ToArray(),
                    enumerator.Current.Payload.ToArray());
            }

            pipe.Reader.Complete();
            Assert.False(await enumerator.MoveNextAsync()); // will break since tryAdvance returns false whenever the pipe is completed (by throwing an InvalidOperationException)
        }

        [Fact]
        public async Task ReadFramesAsync_ShouldThrow_OnEmptyDecoder()
        {
            var decoder = _parser.AsFrameDecoder(default);
            var enumerator = decoder.ReadFramesAsync().GetAsyncEnumerator();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => {
                await enumerator.MoveNextAsync();
            });
        }
        
        [Fact]
        public async Task ReadFramesAsync_ShouldThrow_OnDisposedDecoder()
        {
            var (decoder, _) = CreateConsumer();
            var enumerator = decoder.ReadFramesAsync().GetAsyncEnumerator();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                decoder.Dispose();
                await enumerator.MoveNextAsync();
            });
        }

        private static readonly IMetadataParser _parser = new MessageMetadataParser();
        private static (IFrameDecoder, Pipe) CreateConsumer()
        {
            var pipe = new Pipe();
            return (_parser.AsFrameDecoder(pipe.Reader), pipe);
        }
    }
}
