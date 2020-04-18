using Andromeda.Framing.Behaviors;
using Microsoft.AspNetCore.Connections;

namespace Andromeda.Framing.Extensions.Tests.Infrastructure
{
    public class TestHandlerContext : HandlerContext
    {
        public override ConnectionContext Connection { get; } = new TestConnectionContext();

        public void Close() => ((TestConnectionContext) Connection).Close();
        public void VerifyAbort() => ((TestConnectionContext) Connection).VerifyAbort();
        public void VerifyNotAborted() => ((TestConnectionContext) Connection).VerifyNotAborted();
    }
}
