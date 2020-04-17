using System;

namespace Andromeda.Framing.Extensions.Infrastructure
{
    internal class EmptyServiceProvider : IServiceProvider
    {
        public static IServiceProvider Instance { get; } = new EmptyServiceProvider();

        public object GetService(Type serviceType) => null;
    }
}
