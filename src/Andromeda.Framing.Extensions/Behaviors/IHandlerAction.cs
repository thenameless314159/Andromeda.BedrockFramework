using System.Threading.Tasks;

namespace Andromeda.Framing.Behaviors
{
    public interface IHandlerAction
    {
        ValueTask ExecuteAsync(HandlerContext context);
    }
}
