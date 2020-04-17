using System;
using System.Collections.Generic;
using System.Text;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Protocol;
using Microsoft.Extensions.ObjectPool;

namespace Andromeda.Framing.Extensions.Pooling
{
    internal class PooledMessageEncoderPolicy : PooledObjectPolicy<PooledMessageEncoder>
    {
        private readonly IMetadataEncoder _metaEncoder;
        private readonly IMessageWriter _writer;

        public PooledMessageEncoderPolicy(IMetadataEncoder metaEncoder, IMessageWriter writer)
        {
            _metaEncoder = metaEncoder;
            _writer = writer;
        }

        public override PooledMessageEncoder Create() => new PooledMessageEncoder(_writer, _metaEncoder);
        public override bool Return(PooledMessageEncoder _) => true;
    }
}
