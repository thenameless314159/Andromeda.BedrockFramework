using Andromeda.Framing.Metadata;
using Microsoft.Extensions.ObjectPool;

namespace Andromeda.Framing.Extensions.Pooling
{
    internal class PooledFrameDecoderPolicy : PooledObjectPolicy<PooledFrameDecoder>
    {
        public PooledFrameDecoderPolicy(IMetadataDecoder metaDecoder) => _metaDecoder = metaDecoder;
        private readonly IMetadataDecoder _metaDecoder;

        public override PooledFrameDecoder Create() => new PooledFrameDecoder(_metaDecoder);
        public override bool Return(PooledFrameDecoder _) => true;
    }
}
