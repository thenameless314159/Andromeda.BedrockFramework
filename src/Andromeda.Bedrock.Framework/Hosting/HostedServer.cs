using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Andromeda.Bedrock.Framework.Hosting
{
    internal sealed class HostedServer : IHostedService
    {
        private readonly Server _server;
        public HostedServer(IOptions<HostedServerOptions> options) => _server = options
            .Value.ServerBuilder.Build(); 
        
        public Task StartAsync(CancellationToken cancellationToken) => 
            _server.StartAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) =>
            _server.StopAsync(cancellationToken);
    }
}
