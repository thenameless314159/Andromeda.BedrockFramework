using System;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
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

namespace ClientApplication
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Manual wire up of the client
            var services = new ServiceCollection().AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            });

            services.AddScoped<MessageHandler<HandshakeMessage>, HandshakeMessageHandler>();
            services.Add<MessageMetadataParser, MessageMetadata>();
            services.AddSingleton<ClientConnectionHandler>();
            services.AddSingleton(sp =>
            {
                var behaviors = new BehaviorsBuilder();
                behaviors.Configure<HandshakeMessageHandler, HandshakeMessage>(HandshakeMessage.Id);

                var builder = new ServerFramingBuilder(sp, behaviors);
                builder.MessageWriter = new MessageWriter(builder.Parser);
                builder.MessageReader = new MessageReader();
                builder.UseMessageEncoder();

                return builder.Build();
            });
            
            var serviceProvider = services.BuildServiceProvider();
            var connectionHandler = serviceProvider
                .GetRequiredService<ClientConnectionHandler>();
            var clientFactory = new SocketConnectionFactory();

            Console.WriteLine("Press any key to start connection.");
            Console.ReadKey();

            await using var connection = await clientFactory.ConnectAsync(
                new IPEndPoint(IPAddress.Loopback, 5000));

            await connectionHandler.OnConnectedAsync(connection).ConfigureAwait(false);
        }
    }

    public class HandshakeMessageHandler : SendMessageHandler<HandshakeMessage>
    {
        public HandshakeMessageHandler(ILogger<HandshakeMessageHandler> logger) => _logger = logger;
        private readonly ILogger<HandshakeMessageHandler> _logger;

        protected override ValueTask<IHandlerAction> ExecuteActionOnReceivedAsync(CancellationToken token = default)
        {
            _logger.LogInformation($"Received handshake with message : {Message.Message}");
            return new ValueTask<IHandlerAction>(Send(new HelloServerMessage { Response = "General Kenobi !"}));
        }
    }

    internal class ClientConnectionHandler : ConnectionHandler
    {
        private readonly ILogger<ClientConnectionHandler> _logger;
        private readonly ServerFraming _framing;

        public ClientConnectionHandler(ILogger<ClientConnectionHandler> logger, ServerFraming framing)
        {
            _framing = framing;
            _logger = logger;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        { 
            var (decoder, encoder) = _framing.CreatePair(connection);
            var context = new SenderContext(connection, encoder as IMessageEncoder);
            _logger.LogInformation($"Successfully connected to remote server at {connection.RemoteEndPoint} !");

            try {
                await foreach (var frame in decoder.ReadFramesAsync(connection.ConnectionClosed))
                {
                    _logger.LogDebug($"Received a frame with : {frame.Metadata}");
                    var dispatchResult = await _framing.Dispatcher.OnFrameReceivedAsync(in frame, context)
                        .ConfigureAwait(false);

                    if (dispatchResult != DispatchResult.Success) break;
                }
            }
            finally
            {
                _logger.LogInformation($"Connection with remote server at {connection.RemoteEndPoint} ended with framesRead: {decoder.FramesRead}, framesSent: {encoder.FramesWritten} !");
                decoder.Dispose();
                encoder.Dispose();
            }
        }
    }
}
