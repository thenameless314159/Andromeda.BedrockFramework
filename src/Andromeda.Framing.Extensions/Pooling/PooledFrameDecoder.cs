using System.IO.Pipelines;
using System.Threading;
using Andromeda.Framing.Metadata;
using Microsoft.Extensions.ObjectPool;

namespace Andromeda.Framing.Extensions.Pooling
{
    internal sealed class PooledFrameDecoder : PipeFrameDecoder
    {
        internal ObjectPool<PooledFrameDecoder> _pool;
        public PooledFrameDecoder(IMetadataDecoder decoder)
            : base(decoder, default)
        {
        }

        public bool TryReset(PipeReader reader)
        {
            if (Interlocked.CompareExchange(ref _reader, reader, null) != null)
                return false;

            FramesRead = 0;
            return true;
        }

        public override void Dispose()
        {
            if (!Close()) return;
            _pool?.Return(this);
        }
    }
}
