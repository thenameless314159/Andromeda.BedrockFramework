using System;
using Andromeda.Framing.Behaviors;
using Andromeda.Framing.Extensions.Tests.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Andromeda.Framing.Extensions.Tests.Infrastructure
{
    internal static class DispatcherProvider
    {
        public static TestFrameDispatcher Empty  => new TestFrameDispatcher(new ServiceCollection().BuildServiceProvider(), new TestMessageReader());
        public static IFrameDispatcher Dispatcher { get; }
        public static IServiceProvider Services { get; }
        
        static DispatcherProvider()
        {
            Services = new ServiceCollection().AddFrameDispatcher(
                b => b.Configure<HandshakeMessageHandler, HandshakeMessage>(1),
                d => d.Decoder = new TestMessageReader())
                .BuildServiceProvider();

            Dispatcher = Services.GetRequiredService<IFrameDispatcher>();
        }
    }
}
