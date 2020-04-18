using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Andromeda.Bedrock.Framework;
using Andromeda.Framing;
using Andromeda.Framing.Behaviors;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Protocol;
using Protocol.Models;

namespace ServerApplication
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // Manual wire up of the server
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            });

            services.AddScoped<MessageHandler<HelloServerMessage>, HelloServerMessageHandler>();
            services.Add<MessageMetadataParser, MessageMetadata>();
            services.AddSingleton(sp =>
            {
                var builder = new ServerFramingBuilder(sp);
                builder.Mappings.Add((HelloServerMessage.Id, typeof(HelloServerMessage)));
                builder.MessageWriter = new MessageWriter(builder.Parser);
                builder.MessageReader = new MessageReader();
                builder.UseMessageEncoder();

                return builder.Build();
            });

            var serviceProvider = services.BuildServiceProvider();

            var server = new ServerBuilder(serviceProvider)
                        .UseSockets(sockets => sockets.Listen(IPAddress.Loopback, 5000,
                            builder => builder.UseConnectionHandler<ServerConnectionHandler>()))
                        .Build();

            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
            await server.StartAsync();

            foreach (var ep in server.EndPoints)
                logger.LogInformation("Listening on {EndPoint}", ep);

            var tcs = new TaskCompletionSource<object>();
            Console.CancelKeyPress += (sender, e) => tcs.TrySetResult(null);

            await tcs.Task;
            await server.StopAsync();
        }
    }

    public class HelloServerMessageHandler : MessageHandler<HelloServerMessage>
    {
        public HelloServerMessageHandler(ILogger<HelloServerMessageHandler> logger) => _logger = logger;
        private readonly ILogger<HelloServerMessageHandler> _logger;

        protected override ValueTask<IHandlerAction> ExecuteActionOnReceivedAsync(CancellationToken token = default)
        {
            _logger.LogInformation($"Connection with id='{HandlerContext.Connection.ConnectionId}' sent response : {Message.Response}");
            return new ValueTask<IHandlerAction>(Abort());
        }
    }

    internal class ServerConnectionHandler : ConnectionHandler
    {
        private static readonly HandshakeMessage _handshakeMessage = new HandshakeMessage { Message = "Hello there !"};
        private readonly ILogger<ServerConnectionHandler> _logger;
        private readonly ServerFraming _framing;

        public ServerConnectionHandler(ILogger<ServerConnectionHandler> logger, ServerFraming framing)
        {
            _framing = framing;
            _logger = logger;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            _logger.LogInformation($"New connection with id='{connection.ConnectionId}' !");
            var (decoder, encoder) = _framing.CreatePair(connection); 
            var proxy = encoder as IMessageEncoder ?? throw new InvalidOperationException();
            var context = new SenderContext(connection, proxy);

            try {
                await proxy.WriteAsync(_handshakeMessage).ConfigureAwait(false);
                await foreach (var frame in decoder.ReadFramesAsync(connection.ConnectionClosed))
                {
                    _logger.LogDebug($"Connection with id = '{connection.ConnectionId}' sent a frame with : {frame.Metadata}");
                    var dispatchResult = await _framing.Dispatcher.OnFrameReceivedAsync(in frame, context)
                        .ConfigureAwait(false);

                    if (dispatchResult != DispatchResult.Success) break;
                }
            }
            finally {
                _logger.LogInformation($"Connection with id='{connection.ConnectionId}' ended with framesRead: {decoder.FramesRead}, framesSent: {encoder.FramesWritten} !");
                decoder.Dispose();
                encoder.Dispose();
            }
        }
    }
}
