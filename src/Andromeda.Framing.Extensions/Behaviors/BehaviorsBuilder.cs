using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Andromeda.Framing.Behaviors
{
    public class BehaviorsBuilder
    {
        protected internal readonly ICollection<(int, ServiceDescriptor)> _messageHandlers = new List<(int, ServiceDescriptor)>();

        public BehaviorsBuilder Configure<THandler, TMsg>(int messageId)
            where THandler : MessageHandler<TMsg>
            where TMsg : new()
        {
            _messageHandlers.Add((messageId, ServiceDescriptor.Scoped<MessageHandler<TMsg>, THandler>()));
            return this;
        }
    }
}
