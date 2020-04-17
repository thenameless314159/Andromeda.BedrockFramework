using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace Andromeda.Framing.Behaviors
{
    internal sealed class AbortAction : IHandlerAction
    {
        public ValueTask ExecuteAsync(HandlerContext context)
        {
            context.Connection.Abort(new ConnectionAbortedException("Aborted by a message handler !"));
            return default;
        }
    }
}
