using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Andromeda.Bedrock.Framework.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureServer(this IHostBuilder builder, Action<ServerBuilder> configure)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddHostedService<HostedServer>();

                services.AddOptions<HostedServerOptions>()
                    .Configure<IServiceProvider>((options, sp) =>
                    {
                        options.ServerBuilder = new ServerBuilder(sp);
                        configure(options.ServerBuilder);
                    });
            });
        }
    }
}
