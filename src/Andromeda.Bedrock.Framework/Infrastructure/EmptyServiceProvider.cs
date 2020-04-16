using System;

namespace Andromeda.Bedrock.Framework.Infrastructure
{
    internal class EmptyServiceProvider : IServiceProvider
    {
        public static IServiceProvider Instance { get; } = new EmptyServiceProvider();

        public object GetService(Type serviceType) => null;
    }
}
