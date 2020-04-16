using System;
using System.Collections.Generic;
using Andromeda.Bedrock.Framework.Infrastructure;

namespace Andromeda.Bedrock.Framework
{
    public class ServerBuilder
    {
        public ServerBuilder() : this(EmptyServiceProvider.Instance) { }
        public ServerBuilder(IServiceProvider serviceProvider) => ApplicationServices = serviceProvider;

        public IList<ServerBinding> Bindings { get; } = new List<ServerBinding>();
        public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(3);
        public IServiceProvider ApplicationServices { get; }

        public Server Build() => new Server(this);
    }
}
