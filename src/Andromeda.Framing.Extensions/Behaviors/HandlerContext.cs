using Microsoft.AspNetCore.Connections;

namespace Andromeda.Framing.Behaviors
{
    public abstract class HandlerContext
    {
        public abstract ConnectionContext Connection { get; }
    }
}
