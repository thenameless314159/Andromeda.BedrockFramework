using Andromeda.Framing;
using Andromeda.Framing.Behaviors;
using Microsoft.AspNetCore.Connections;

namespace Protocol
{
    public class SenderContext : HandlerContext
    {
        public override ConnectionContext Connection { get; }
        public IMessageEncoder Encoder { get; }

        public SenderContext(ConnectionContext connection, IMessageEncoder encoder)
        {
            Connection = connection;
            Encoder = encoder;
        }
    }
}
