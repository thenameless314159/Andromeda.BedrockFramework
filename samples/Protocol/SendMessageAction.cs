using System;
using System.Threading.Tasks;
using Andromeda.Framing.Behaviors;

namespace Protocol
{
    internal class SendMessageAction<T> : IHandlerAction
    {
        public SendMessageAction(T message) => _message = message;
        private readonly T _message;

        public ValueTask ExecuteAsync(HandlerContext context)
        {
            if (!(context is SenderContext ctx))
                throw new InvalidOperationException();

            return ctx.Encoder.WriteAsync(in _message);
        }
    }
}
