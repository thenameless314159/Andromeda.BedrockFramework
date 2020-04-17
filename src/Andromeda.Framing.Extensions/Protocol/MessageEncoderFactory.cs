using Andromeda.Framing.Metadata;
using Microsoft.AspNetCore.Connections;

namespace Andromeda.Framing.Protocol
{
    public class MessageEncoderFactory : IFrameEncoderFactory
    {
        private readonly IMetadataEncoder _metaEncoder;
        private readonly IMessageWriter _msgWriter;

        public MessageEncoderFactory(IMetadataEncoder metaEncoder, IMessageWriter msgWriter)
        {
            _metaEncoder = metaEncoder;
            _msgWriter = msgWriter;
        }

        public IFrameEncoder Create(ConnectionContext fromContext) => new PipeMessageEncoder(_msgWriter, _metaEncoder, 
            fromContext.Transport.Output, fromContext.ConnectionClosed);
    }
}
