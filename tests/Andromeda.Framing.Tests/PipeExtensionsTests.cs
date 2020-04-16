using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Andromeda.Framing.Extensions;
using Xunit;

namespace Andromeda.Framing.Tests
{
    public class PipeExtensionsTests
    {
        [Theory, InlineData(2), InlineData(4), InlineData(6)]
        public async Task AsAsyncEnumerable_ShouldConsumeBuffers(int nb)
        {
            const int len = 256;
            var pipe = new Pipe();
            var enumerator = pipe.Reader.AsAsyncEnumerable().GetAsyncEnumerator();
            var moveNext = enumerator.MoveNextAsync();
            for (var i = 0; i < nb; i++)
            {
                await pipe.Writer.WriteAsync(new byte[len]);
                Assert.True(await moveNext);
                Assert.Equal(len, enumerator.Current.Length);
            }

            pipe.Reader.Complete();
            Assert.False(await enumerator.MoveNextAsync());
        }

        [Fact]
        public async Task AsAsyncEnumerable_ShouldBreak_OnCancelled()
        {
            var pipe = new Pipe();
            using var cts = new CancellationTokenSource();
            var enumerator = pipe.Reader.AsAsyncEnumerable(cts.Token).GetAsyncEnumerator(cts.Token);
            cts.Cancel(); Assert.False(await enumerator.MoveNextAsync());
        }

        [Fact]
        public async Task AsAsyncEnumerable_ShouldBreak_OnCompleted()
        {
            var pipe = new Pipe();
            var enumerator = pipe.Reader.AsAsyncEnumerable().GetAsyncEnumerator();

            pipe.Reader.Complete();
            Assert.False(await enumerator.MoveNextAsync());
        }
        
        private const int _chunkMaxSize = 1024 * 8;

        [Theory, InlineData(1024), InlineData(2048), InlineData(4096), InlineData(_chunkMaxSize)]
        public async Task WriteSeqByChunkAsync_ShouldWriteSingleChunk(int onLength)
        {
            var buffer = new byte[onLength];
            new Random().NextBytes(buffer);
            var pipe = new Pipe();

            var writeResult = await pipe.Writer.WriteByChunkAsync(new ReadOnlySequence<byte>(buffer));
            Assert.True(!writeResult.IsCanceled && !writeResult.IsCompleted);

            var result = await pipe.Reader.ReadAsync();
            Assert.Equal(buffer, result.Buffer.ToArray());
        }

        [Theory, InlineData(1024), InlineData(2048), InlineData(4096), InlineData(_chunkMaxSize)]
        public async Task WriteMemByChunkAsync_ShouldWriteSingleChunk(int onLength)
        {
            var buffer = new byte[onLength];
            new Random().NextBytes(buffer);
            var pipe = new Pipe();

            var writeResult = await pipe.Writer.WriteByChunkAsync(buffer);
            Assert.True(!writeResult.IsCanceled && !writeResult.IsCompleted);

            var result = await pipe.Reader.ReadAsync();
            Assert.Equal(buffer, result.Buffer.ToArray());
        }

        [Theory, InlineData(2), InlineData(4), InlineData(6)]
        public async Task WriteSeqByChunkAsync_ShouldWriteChunks(int nb)
        {
            var buffer = new byte[nb * _chunkMaxSize];
            new Random().NextBytes(buffer);
            var pipe = new Pipe();

            var writeResult = await pipe.Writer.WriteByChunkAsync(new ReadOnlySequence<byte>(buffer));
            Assert.True(!writeResult.IsCanceled && !writeResult.IsCompleted);

            var result = await pipe.Reader.ReadAsync();
            Assert.Equal(buffer, result.Buffer.ToArray());
        }

        [Theory, InlineData(2), InlineData(4), InlineData(6)]
        public async Task WriteMemByChunkAsync_ShouldWriteChunks(int nb)
        {
            var buffer = new byte[nb * _chunkMaxSize];
            new Random().NextBytes(buffer);
            var pipe = new Pipe();

            var writeResult = await pipe.Writer.WriteByChunkAsync(buffer);
            Assert.True(!writeResult.IsCanceled && !writeResult.IsCompleted);

            var result = await pipe.Reader.ReadAsync();
            Assert.Equal(buffer, result.Buffer.ToArray());
        }
    }
}
