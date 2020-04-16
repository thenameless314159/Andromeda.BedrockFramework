using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Andromeda.Framing.Extensions;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Tests.Metadata;
using Xunit;

namespace Andromeda.Framing.Tests
{
    public class PipeFrameEncoderTests
    {
        [Fact]
        public async Task WriteAsync_ShouldThrow_OnEmptyEncoder()
        {
            var encoder = _parser.AsFrameEncoder(default);
            await WriteEmptyAsync(encoder).ConfigureAwait(false);
        }

        [Fact]
        public async Task WriteAsync_ShouldThrow_OnDisposedEncoder()
        {
            var (encoder, _) = CreateProducer();
            encoder.Dispose();
            await WriteEmptyAsync(encoder).ConfigureAwait(false);
        }

        private static async Task WriteEmptyAsync(IFrameEncoder encoder)
        {
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                { await encoder.WriteAsync(in Frame.Empty); }).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                { await encoder.WriteAsync(Frame.EmptyFrames); }).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                { await encoder.WriteAsync(AsyncEnumerable.Empty<Frame>()); }).ConfigureAwait(false);
        }

        private static readonly IMetadataParser _parser = new MessageMetadataParser();
        private static (IFrameEncoder, Pipe) CreateProducer(CancellationToken token = default)
        {
            var pipe = new Pipe();
            return (_parser.AsFrameEncoder(pipe.Writer, token), pipe);
        }
    }
}
