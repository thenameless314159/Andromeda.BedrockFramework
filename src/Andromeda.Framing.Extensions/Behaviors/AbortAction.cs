using System.Threading.Tasks;

namespace Andromeda.Framing.Behaviors
{
    internal sealed class AbortAction : IHandlerAction
    {
         public ValueTask ExecuteAsync(HandlerContext context)
        {
            context.Connection.Abort();
            return default;
        }
    }
}
