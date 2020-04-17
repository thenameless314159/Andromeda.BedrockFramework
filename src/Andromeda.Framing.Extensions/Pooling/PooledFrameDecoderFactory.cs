using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.ObjectPool;

namespace Andromeda.Framing.Extensions.Pooling
{
    internal class PooledFrameDecoderFactory : IFrameDecoderFactory
    {
        public PooledFrameDecoderFactory(ObjectPool<PooledFrameDecoder> pool) => _pool = pool;
        private readonly ObjectPool<PooledFrameDecoder> _pool;

        public IFrameDecoder Create(ConnectionContext fromContext)
        {
            var reader = fromContext.Transport.Input;
            var encoder = _pool.Get();

            if (encoder._pool == null) encoder._pool = _pool;

            return encoder.TryReset(reader)
                ? encoder
                : default;
        }
    }
}
