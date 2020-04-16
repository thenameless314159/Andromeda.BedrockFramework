using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Andromeda.Framing.Extensions;
using Andromeda.Framing.Metadata;
using PooledAwait;

namespace Andromeda.Framing
{
    internal class PipeFrameEncoder : IFrameEncoder
    {
        private readonly IMetadataEncoder _metadataEncoder;
        protected readonly SemaphoreSlim _singleWriter;
        protected CancellationToken _token;
        protected PipeWriter _pipeWriter;
        private long _framesWritten;

        public long FramesWritten
        {
            get => Interlocked.Read(ref _framesWritten);
            protected set => Interlocked.Exchange(ref _framesWritten, value);
        }

        public PipeFrameEncoder(IMetadataEncoder metadataEncoder, PipeWriter writer, CancellationToken token = default)
        {
            _token = token == default ? CancellationToken.None : token;
            _singleWriter = new SemaphoreSlim(1,1);
            _metadataEncoder = metadataEncoder;
            _pipeWriter = writer;
        }

        public ValueTask WriteAsync(in Frame frame) // fast path from M. Gravell, cf. https://github.com/mgravell/simplsockets/blob/master/SimplPipelines/SimplPipeline.cs
        {
            var writer = _pipeWriter ?? throw new ObjectDisposedException(nameof(IFrameEncoder));

            // try to get the conch; if not, switch to async
            if (!_singleWriter.Wait(0)) return sendAsyncSlowPath(frame);
            var release = true;
            try
            {
                var write = _metadataEncoder.WriteFrameAsync(writer, frame, _token); // includes a flush
                if (write.IsCompletedSuccessfully) return default;// sync fast path
                release = false;
                return awaitFlushAndRelease(write);
            }
            finally { if (release) { Release(); } }

            async PooledValueTask awaitFlushAndRelease(ValueTask<FlushResult> flush) {
                try { await flush; } finally { Release(); }
            }
            async PooledValueTask sendAsyncSlowPath(Frame frm) {
                await _singleWriter.WaitAsync(_token).ConfigureAwait(false);
                try { await _metadataEncoder.WriteFrameAsync(writer, frm, _token).ConfigureAwait(false); }
                finally { Release(); }
            }
        }

        public ValueTask WriteAsync(IEnumerable<Frame> frames)
        {
            var writer = _pipeWriter ?? throw new ObjectDisposedException(nameof(IFrameEncoder));
            return !_singleWriter.Wait(0) 
                ? sendAllSlow()
                : sendAll();

            async PooledValueTask sendAll() {
                try {
                    foreach (var frame in frames) 
                        await _metadataEncoder.WriteFrameAsync(writer, frame, _token)
                            .ConfigureAwait(false);
                } 
                finally { Release(); }
            }
            async PooledValueTask sendAllSlow() {
                await _singleWriter.WaitAsync(_token).ConfigureAwait(false);
                await sendAll().ConfigureAwait(false);
            }
        }

        // maybe should iterate async enumerable first, and wait to write only on each frame received from the async enumerable
        // since the async enumerable can be from the IFrameDecoder consumer, it'd lock any external write from this class
        public ValueTask WriteAsync(IAsyncEnumerable<Frame> frames)
        {
            var writer = _pipeWriter ?? throw new ObjectDisposedException(nameof(IFrameEncoder));
            return !_singleWriter.Wait(0)
                ? sendAllSlow()
                : sendAll();

            async PooledValueTask sendAll() {
                var writtenCount = 0;
                try {
                    await foreach (var frame in frames.WithCancellation(_token)) {
                        await _metadataEncoder.WriteFrameAsync(writer, frame, _token)
                            .ConfigureAwait(false);
                        writtenCount++;
                    }
                }
                finally { Release(writtenCount); }
            }
            async PooledValueTask sendAllSlow() {
                await _singleWriter.WaitAsync(_token).ConfigureAwait(false);
                await sendAll().ConfigureAwait(false);
            }
        }

        protected virtual void Release(int framesWritten = 1) {
            _framesWritten += framesWritten;
            _singleWriter.Release();
        }

        protected bool Close(Exception ex = null)
        {
            var writer = Interlocked.Exchange(ref _pipeWriter, null);
            if (writer == null) return false;

            try { writer.Complete(ex); } catch { /* discarded */ }
            try { writer.CancelPendingFlush(); } catch { /* discarded */}
            return true;
        }

        public virtual void Dispose()
        {
            Close();
            _singleWriter.Dispose();
        }
    }
}
