using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Andromeda.Framing.Behaviors;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Protocol;
using Microsoft.Extensions.DependencyInjection;
using PooledAwait;

namespace Andromeda.Framing.Extensions.Behaviors
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
            if (!(frame.Metadata is MessageMetadataWithId metadata))
                return new ValueTask<DispatchResult>(DispatchResult.InvalidFrame);

            return !_handlers.TryGetValue(metadata.MessageId, out var handler)
                ? new ValueTask<DispatchResult>(DispatchResult.HandlerNotMapped)
                : handler(frame, context);
        }

        public void Map<TMessage>(int withId) where TMessage : new()
        {
            _handlers[withId] = onFrame;
            async PooledValueTask<DispatchResult> onFrame(Frame frame, HandlerContext context)
            {
                var payload = frame.Payload;
                var message = new TMessage();
                if (!_decoder.TryParse(in payload, message))
                    return DispatchResult.InvalidFrame;

                using var scope = _provider.CreateScope();
                var handler = scope.ServiceProvider.GetService<MessageHandler<TMessage>>();
                if (handler == null) return DispatchResult.HandlerNotRegistered;
                handler.Setup(context, message);

                if (!await handler.CanProcessAsync().ConfigureAwait(false)) return DispatchResult.PredicateFailed;
                await foreach (var response in handler.OnMessageReceivedAsync(context.Connection.ConnectionClosed).ConfigureAwait(false))
                    await response.ExecuteAsync(context).ConfigureAwait(false);

                return DispatchResult.Success;
            }
        }
    }
}
