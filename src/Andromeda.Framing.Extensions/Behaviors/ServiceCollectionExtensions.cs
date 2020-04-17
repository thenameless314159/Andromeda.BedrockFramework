using System;
using System.Linq;
using System.Reflection;
using Andromeda.Framing.Extensions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Andromeda.Framing.Behaviors
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register all the message handlers found in the entry assembly into the
        /// <see cref="IServiceCollection"/> provided.
        /// </summary>
        /// <param name="services">The services.</param>
        public static IServiceCollection AddMessageHandlers(this IServiceCollection services) =>
            services.AddMessageHandlers(Assembly.GetEntryAssembly());

        /// <summary>
        /// Register all the message handlers found in the provided assemblies into the
        /// <see cref="IServiceCollection"/> provided.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="fromAssemblies">The assemblies.</param>
        public static IServiceCollection AddMessageHandlers(this IServiceCollection services,
            params Assembly[] fromAssemblies)
        {
            var handlers = from assembly in fromAssemblies
                from type in assembly.GetExportedTypes()
                where !type.IsAbstract
                      && type.IsClass
                      && type.IsAssignableToGenericType(typeof(MessageHandler<>))
                select type;

            foreach (var handler in handlers)
            {
                var message = handler.GetBaseTypes().First(t => t.Namespace == typeof(MessageHandler<>).Namespace);
                services.TryAddScoped(typeof(MessageHandler<>).MakeGenericType(message),
                    handler);
            }

            return services;
        }

        public static IServiceCollection AddFrameDispatcher(this IServiceCollection services, Action<BehaviorsBuilder> configure, 
            Action<DispatcherBuilder> configureProtocol = default)
        {
            var behaviors = new BehaviorsBuilder();
            configure(behaviors);

            services.TryAdd(behaviors._messageHandlers.Select(p => p.Item2));
            services.TryAddSingleton(sp =>
            {
                var dispatcher = new DispatcherBuilder(sp, behaviors);
                configureProtocol?.Invoke(dispatcher);
                return dispatcher.Build();
            });

            return services;
        }
    }
}
