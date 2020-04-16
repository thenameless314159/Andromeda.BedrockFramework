﻿using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using Andromeda.Framing.Extensions;
using Andromeda.Framing.Memory;
using Andromeda.Framing.Metadata;

namespace Andromeda.Framing
{
    internal class PipeFrameDecoder : IFrameDecoder
    {
        private readonly IMetadataDecoder _decoder;
        private PipeReader _reader;

        public PipeFrameDecoder(IMetadataDecoder decoder, PipeReader reader)
        {
            _decoder = decoder;
            _reader = reader;
        }

        public async IAsyncEnumerable<Frame> ReadFramesAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            var reader = _reader ?? throw new ObjectDisposedException(nameof(PipeReader));
            var holder = new SequenceHolder();
             
            await foreach (var buffer in reader.AsAsyncEnumerable(token))
            {
                holder.Buffer = buffer;
                foreach (var frame in _decoder.GetFramesOf(holder, token)) {
                    yield return frame;
                }

                if (token.IsCancellationRequested) break;
                if (!_reader.TryAdvanceTo(holder.Buffer.Start, holder.Buffer.End))
                    break;
            }
        }

        public bool Close(Exception ex = null)
        {
            var reader = Interlocked.Exchange(ref _reader, null);
            if (reader == null) return false;

            try { reader.Complete(ex); } catch { /* discarded */ }
            try { reader.CancelPendingRead(); } catch { /* discarded */}
            return true;
        }

        public virtual void Dispose() => Close();
    }
}
