using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using Andromeda.Framing.Memory;
using Andromeda.Framing.Metadata;

namespace Andromeda.Framing
{
    public static class DecoderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFrameDecoder AsFrameDecoder(this IMetadataDecoder decoder, PipeReader reader) =>
            new PipeFrameDecoder(decoder, reader);

        public static IEnumerable<Frame> GetFramesOf(this IMetadataDecoder decoder, SequenceHolder holder, CancellationToken token = default)
        {
            if(token == default) token = CancellationToken.None;
            while (!token.IsCancellationRequested)
            {
                var buffer = holder.Buffer;
                var reader = new SequenceReader<byte>(buffer);
                if (!decoder.TryParse(ref reader, out var metadata)) break;
                if (metadata.Length < 0) throw new ArgumentOutOfRangeException(
                    nameof(metadata.Length), string.Format(_error, metadata));
                
                if (reader.Remaining < metadata.Length) break;
                var payload = buffer.Slice(reader.Position, metadata.Length);
                yield return new Frame(payload, metadata);
                holder.Buffer = buffer.Slice(payload.End);
            }
        }

        private const string _error = "Invalid length of frame with metadata : {0} !";
    }
}
