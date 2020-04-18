using Andromeda.Framing.Behaviors;

namespace Protocol
{
    public abstract class SendMessageHandler<TMsg> : MessageHandler<TMsg>
    {
        protected IHandlerAction Send<T>(T message) => new SendMessageAction<T>(message);
    }
}
