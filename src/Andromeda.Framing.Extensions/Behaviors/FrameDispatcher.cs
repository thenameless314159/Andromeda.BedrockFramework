using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Protocol;
using Microsoft.Extensions.DependencyInjection;
using PooledAwait;

namespace Andromeda.Framing.Behaviors
{
    internal class FrameDispatcher : IFrameDispatcher
    {
        protected delegate PooledValueTask<DispatchResult> Handler(Frame frame, HandlerContext context);
        protected readonly IDictionary<int, Handler> _handlers = new Dictionary<int, Handler>();
        private readonly IServiceProvider _provider;
        private readonly IMessageReader _decoder;

        public FrameDispatcher(IServiceProvider sp, IMessageReader decoder)
        {
            _decoder = decoder;
            _provider = sp;
        }

        public ValueTask<DispatchResult> OnFrameReceivedAsync(in Frame frame, HandlerContext context)
        {
            if (context == default) throw new ArgumentNullException(nameof(context));
            if (!(frame.Metadata is MessageMetadataWithId metadata))
                throw new InvalidOperationException($"{nameof(frame.Metadata)} must be of type {nameof(MessageMetadataWithId)} !");

            return !_handlers.TryGetValue(metadata.MessageId, out var handler)
                ? new ValueTask<DispatchResult>(DispatchResult.HandlerNotMapped)
                : handler(frame, context);
        }

        public void Map<TMessage>(int withId) where TMessage : new()
        {
            _handlers[withId] = onFrame;

            // The handler should be to able to execute once if the message is valid even if the
            // connection closed since a message handler can also perform db update, logging etc...
            async PooledValueTask<DispatchResult> onFrame(Frame frame, HandlerContext context)
            {
                var payload = frame.Payload;
                var message = new TMessage();
                if (!_decoder.TryParse(in payload, message))
                    return DispatchResult.InvalidFrame;

                using var scope = _provider.CreateScope();
                var handler = scope.ServiceProvider.GetService<MessageHandler<TMessage>>();
                if (handler == default) return DispatchResult.HandlerNotRegistered;
                handler.Setup(context, message);

                if (!await handler.CanProcessAsync().ConfigureAwait(false)) return DispatchResult.PredicateFailed;

                var token = context.Connection.ConnectionClosed;
                await foreach (var response in handler.OnMessageReceivedAsync(token).ConfigureAwait(false)) {
                    if (token.IsCancellationRequested) return DispatchResult.Cancelled; // if OnMessageReceivedAsync is overriden the first action will be out so this check is still needed
                    await response.ExecuteAsync(context).ConfigureAwait(false);
                }

                return token.IsCancellationRequested 
                    ? DispatchResult.Cancelled 
                    : DispatchResult.Success;
            }
        }
    }
}
