using System.Threading;
using System.Threading.Tasks;
using Andromeda.Framing.Behaviors;
using Andromeda.Framing.Extensions.Tests.Infrastructure.Models;

namespace Andromeda.Framing.Extensions.Tests.Infrastructure
{
    public class HandshakeMessageHandler : MessageHandler<HandshakeMessage>
    {
        protected override ValueTask<IHandlerAction> ExecuteActionOnReceivedAsync(CancellationToken token = default) =>
            new ValueTask<IHandlerAction>(Abort());
    }
}
