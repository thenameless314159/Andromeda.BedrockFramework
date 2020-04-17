using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.ObjectPool;

namespace Andromeda.Framing.Extensions.Pooling
{
    internal class PooledFrameEncoderFactory : IFrameEncoderFactory
    {
        public PooledFrameEncoderFactory(ObjectPool<PooledFrameEncoder> pool) => _pool = pool;
        private readonly ObjectPool<PooledFrameEncoder> _pool;

        public IFrameEncoder Create(ConnectionContext fromContext)
        {
            var writer = fromContext.Transport.Output;
            var token = fromContext.ConnectionClosed;
            var encoder = _pool.Get();

            if (encoder._pool == null) encoder._pool = _pool;

            return encoder.TryReset(writer, token) 
                ? encoder
                : default;
        }
    }
}
