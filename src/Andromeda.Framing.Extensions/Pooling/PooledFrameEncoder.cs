using System.IO.Pipelines;
using System.Threading;
using Andromeda.Framing.Metadata;
using Microsoft.Extensions.ObjectPool;

namespace Andromeda.Framing.Extensions.Pooling
{
    internal sealed class PooledFrameEncoder : PipeFrameEncoder
    {
        internal ObjectPool<PooledFrameEncoder> _pool;
        public PooledFrameEncoder(IMetadataEncoder metadataEncoder)
            : base(metadataEncoder, default)
        {
        }

        public bool TryReset(PipeWriter writer, CancellationToken token = default)
        {
            if (Interlocked.CompareExchange(ref _pipeWriter, writer, null) != null)
                return false;

            _token = token == default ? CancellationToken.None : token;
            FramesWritten = 0;
            return true;
        }

        public override void Dispose()
        {
            if (!Close()) return;
            _pool.Return(this);
        }
    }
}
