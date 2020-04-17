using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Andromeda.Framing.Extensions.Infrastructure;

namespace Andromeda.Framing.Behaviors
{
    /// <summary>
    /// Base class for any message handler, only the public virtual methods
    /// are used for the message handling process. You can implement either
    /// the asynchronous or synchronous <see cref="CanProcess"/> message
    /// handling predicate. You can do the same about the <see cref="OnMessageReceivedAsync"/>
    /// handling method, implement either single or no response handling
    /// method.
    ///
    /// A <see cref="MessageHandler{TMessage}"/> is a state-less scoped dependency,
    /// it shouldn't store any different logic than the handling behavior of the message.
    /// </summary>
    /// <typeparam name="TMessage">The message to handle.</typeparam>
    public abstract class MessageHandler<TMessage>
    {
        private readonly SetOnce<HandlerContext> _context = new SetOnce<HandlerContext>(true, nameof(HandlerContext));
        private readonly WriteOnce<TMessage> _message = new WriteOnce<TMessage>(true, typeof(TMessage).GetTypeName());

        internal void Setup(HandlerContext context, TMessage message) {
            HandlerContext = context;
            Message = message;
        }

        protected internal HandlerContext HandlerContext
        {
            get => _context.Value;
            private set => _context.Set(in value);
        }

        public TMessage Message
        {
            get => _message.Value;
            private set => _message.Set(in value);
        }

        public virtual ValueTask<bool> CanProcessAsync() => new ValueTask<bool>(CanProcess());
        protected virtual bool CanProcess() => true;

        public virtual async IAsyncEnumerable<IHandlerAction> OnMessageReceivedAsync([EnumeratorCancellation]CancellationToken token = default)
        {
            var action = await ExecuteActionOnReceivedAsync(token);
            if (action == default) yield break;
            yield return action;
        }

        protected virtual async ValueTask<IHandlerAction> ExecuteActionOnReceivedAsync(CancellationToken token = default)
        {
            await ExecuteOnReceivedAsync(token);
            return default;
        }

        protected virtual ValueTask ExecuteOnReceivedAsync(CancellationToken token = default) => default;

        protected IHandlerAction Abort() => new AbortAction();
    }
}
