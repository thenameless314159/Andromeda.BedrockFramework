using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PooledAwait;

namespace Andromeda.Framing.Extensions
{
    public static class PipeExtensions
    {
        public static async IAsyncEnumerable<ReadOnlySequence<byte>> AsAsyncEnumerable
            (this PipeReader reader, [EnumeratorCancellation] CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var result = await reader.TryReadAsync(token)
                    .ConfigureAwait(false);

                if (result.IsCanceled) yield break;

                var buffer = result.Buffer;
                if (buffer.IsEmpty && result.IsCompleted) 
                    yield break;
                
                yield return buffer;
                if (result.IsCompleted) yield break;
            }
        }

        public static ValueTask<FlushResult> WriteByChunkAsync(this PipeWriter writer, ReadOnlySequence<byte> buffer,
            CancellationToken token = default) =>
            buffer.IsSingleSegment ? writer.WriteByChunkAsync(buffer.First, token) : writer.WriteBigBufferAsync(buffer, token);

        public static  ValueTask<FlushResult> WriteByChunkAsync(this PipeWriter writer, ReadOnlyMemory<byte> buffer,
            CancellationToken token = default) =>
            buffer.Length < _chunkSize ? writer.WriteAsync(buffer, token) : writer.WriteBigBufferAsync(buffer, token);

        private static async ValueTask<FlushResult> WriteBigBufferAsync(this PipeWriter writer, ReadOnlySequence<byte> sequence,
            CancellationToken token = default)
        {
            var enumerator = sequence.GetEnumerator();
            
            FlushResult lastResult = default;
            while (enumerator.MoveNext() && !token.IsCancellationRequested)
                lastResult = await writer.WriteBigBufferAsync(enumerator.Current, token)
                    .ConfigureAwait(false);
            
            return lastResult;
        }

        private static async ValueTask<FlushResult> WriteBigBufferAsync(this PipeWriter writer, ReadOnlyMemory<byte> buffer,
            CancellationToken token = default)
        {
            var i = 0;
            FlushResult lastResult = default;
            for (var c = buffer.Length / _chunkSize; i < c; i++) // write by blocks of size 8192
                lastResult = await writer.WriteAsync(buffer.Slice(i * _chunkSize, _chunkSize), token)
                    .ConfigureAwait(false);

            if (buffer.Length % _chunkSize != 0) // more data remaining
                lastResult = await writer.WriteAsync(buffer.Slice(i * _chunkSize), token)
                    .ConfigureAwait(false);
            
            return lastResult;
        }

        private static async PooledValueTask<ReadResult> TryReadAsync(this PipeReader r, CancellationToken token = default) {
            try { return await r.ReadAsync(token).ConfigureAwait(false); }
            catch (OperationCanceledException) { return CreateEmptyResult(token); }
            catch (InvalidOperationException) { return CreateEmptyResult(token); }
        }

        internal static bool TryAdvanceTo(this PipeReader r, SequencePosition consumed,
            SequencePosition examined)
        {
            try
            {
                if (r == default) return false;
                r.AdvanceTo(consumed, examined); 
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (InvalidOperationException) { return false; }
        }
        
        private static ReadResult CreateEmptyResult(CancellationToken t) =>
                new ReadResult(ReadOnlySequence<byte>.Empty, t.IsCancellationRequested, true);


        private const int _chunkSize = 1024 * 8;
    }
}
