using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Andromeda.Framing.Metadata;
using PooledAwait;

namespace Andromeda.Framing.Protocol
{
    internal class PipeMessageEncoder : PipeFrameEncoder, IMessageEncoder
    {
        private readonly IMessageWriter _messageWriter;

        public PipeMessageEncoder(IMessageWriter messageWriter, IMetadataEncoder metadataEncoder, PipeWriter writer, 
            CancellationToken token = default) : base(metadataEncoder, writer, token) =>
            _messageWriter = messageWriter;

        public ValueTask WriteAsync<T>(in T message)
        {
            var writer = _pipeWriter ?? throw new ObjectDisposedException(nameof(IMessageEncoder));

            // try to get the conch; if not, switch to async
            if (!_singleWriter.Wait(0)) return sendAsyncSlowPath(message);
            var release = true;
            try
            {
                var write = _messageWriter.EncodeAsFrame(writer, in message, _token); // includes a flush
                if (write.IsCompletedSuccessfully) return default; // sync fast path
                release = false;
                return awaitFlushAndRelease(write);
            }
            finally { if (release) { Release(); } }

            async PooledValueTask awaitFlushAndRelease(ValueTask<FlushResult> flush) {
                try { await flush; } finally { Release(); }
            }
            async PooledValueTask sendAsyncSlowPath(T msg) {
                await _singleWriter.WaitAsync(_token).ConfigureAwait(false);
                try { await _messageWriter.EncodeAsFrame(writer, in msg, _token); }
                finally { Release(); }
            }
        }
    }
}
