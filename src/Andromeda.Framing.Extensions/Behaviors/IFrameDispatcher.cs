using System.Threading.Tasks;

namespace Andromeda.Framing.Behaviors
{
    public interface IFrameDispatcher
    {
        ValueTask<DispatchResult> OnFrameReceivedAsync(in Frame frame, HandlerContext context);
    }
}
