using System;
using System.Linq;
using Andromeda.Framing.Behaviors;
using Andromeda.Framing.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Andromeda.Framing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Add<TParser, TMeta>(this IServiceCollection services)
            where TParser : MetadataParserWithId<TMeta>, new()
            where TMeta : MessageMetadataWithId
        {
            var parser = new TParser();
            return services.AddMetadataParser<TParser, TMeta>(parser);
        }
        
        public static IServiceCollection AddMetadataParser<TParser, TMeta>(this IServiceCollection services, TParser parser)
            where TParser : MetadataParserWithId<TMeta>
            where TMeta : MessageMetadataWithId
        {
            services.TryAddSingleton<IMetadataParser>(parser);
            services.TryAddSingleton<IMetadataDecoder>(parser);
            services.TryAddSingleton<IMetadataEncoder>(parser);
            return services;
        }

        public static IServiceCollection ConfigureFraming(this IServiceCollection services,
            Action<ServerFramingBuilder> configure)
        {
            services.AddOptions<ServerFramingOptions>()
                .Configure<IServiceProvider>((options, sp) =>
                {
                    options.FramingBuilder = new ServerFramingBuilder(sp);
                    configure(options.FramingBuilder);
                });

            return services;
        }

        public static IServiceCollection ConfigureFraming(this IServiceCollection services,
            Action<BehaviorsBuilder> configureBehaviors,
            Action<ServerFramingBuilder> configure)
        {
            var behaviors = new BehaviorsBuilder();
            configureBehaviors(behaviors);

            services.TryAdd(behaviors._messageHandlers.Select(p => p.Item2));
            services.AddOptions<ServerFramingOptions>()
                .Configure<IServiceProvider>((options, sp) =>
                {
                    options.FramingBuilder = new ServerFramingBuilder(sp, behaviors);
                    configure(options.FramingBuilder);
                });

            return services;
        }
    }
}
