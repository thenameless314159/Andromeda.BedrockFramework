using System;
using System.Collections.Generic;
using System.Text;
using Andromeda.Framing.Metadata;
using Microsoft.Extensions.ObjectPool;

namespace Andromeda.Framing.Extensions.Pooling
{
    internal class PooledFrameEncoderPolicy : PooledObjectPolicy<PooledFrameEncoder>
    {
        public PooledFrameEncoderPolicy(IMetadataEncoder metaEncoder) => _metaEncoder = metaEncoder;
        private readonly IMetadataEncoder _metaEncoder;

        public override PooledFrameEncoder Create() => new PooledFrameEncoder(_metaEncoder);
        public override bool Return(PooledFrameEncoder _) => true;
    }
}
