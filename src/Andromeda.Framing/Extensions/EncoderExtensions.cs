using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Andromeda.Framing.Metadata;

namespace Andromeda.Framing.Extensions
{
    public static class EncoderExtensions
    {
        public static ValueTask<FlushResult> WriteFrameAsync(this IMetadataEncoder encoder, PipeWriter writer, Frame frame, CancellationToken token = default)
        {
            encoder.WriteMetadata(writer, frame);
            return writer.WriteByChunkAsync(frame.Payload, token);
        }

        public static void WriteMetadata(this IMetadataEncoder encoder, IBufferWriter<byte> writer, Frame frame)
        {
            var metaLength = encoder.GetLength(frame.Metadata);
            var metaSpan = writer.GetSpan(metaLength);

            encoder.Write(metaSpan, frame.Metadata);
            writer.Advance(metaLength);
        }
    }
}
