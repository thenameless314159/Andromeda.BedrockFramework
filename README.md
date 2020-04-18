# <p align="center"> <img src="https://gamepedia.cursecdn.com/minecraft_fr_gamepedia/e/ee/Bedrock.png" width="48" height="48" style="margin-bottom:-15"> Andromeda.Bedrock.Framework - [![Build Status](https://travis-ci.com/thenameless314159/Andromeda.BedrockFramework.svg?branch=master)](https://travis-ci.com/thenameless314159/Andromeda.BedrockFramework) </p>

<div style="text-align:center"><p align="center"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/andromeda_icon2.png?token=AFMTCCLAUUAALOP5UR4TWWC6JQ6Y6" width="140" height="158"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/ASP.NET-Core-Logo_2colors_Square_RGB.png?token=AFMTCCNPNVM6MBG7AF6E75K6JQTHI" width="180" height="168"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/NET-Core-Logo_2colors_Square_RGB.png?token=AFMTCCNORD45RRHKSS456HK6JQTJU" width="180" height="168"></p></div>

This project is a fork of the [*Project Bedrock*](https://github.com/aspnet/AspNetCore/issues/4772) which provides a **different protocol/messaging** logic and a **new framing layer** decoupled from the protocol logic. At this point, the project was only made to suits my needs, therefore the **BedrockFramework** layer has some differences with the original :

- No other protocol implementation, only the base socket layers with the same APIs
- No more `IConnectionHeartbeatFeature` and `IConnectionCompleteFeature` in the server `ConnectionContext`

For further infos about the original project, see the presentation [here](https://speakerdeck.com/davidfowl/project-bedrock)

## Andromeda.Framing

Standalone assembly that contains generic and extandable APIs to handle the frame encoding/decoding logic. In networking, a frame is a unit of data that helps to identify data packets, here they are represented by
[**a sealed class**](https://github.com/thenameless314159/Andromeda.BedrockFramework/blob/master/src/Andromeda.Framing/Frame.cs)  which contains 2 read only properties : [`IMessageMetadata`](https://github.com/thenameless314159/Andromeda.BedrockFramework/blob/master/src/Andromeda.Framing/Metadata/IMessageMetadata.cs) and a `ReadOnlySequence<byte>` representing the payload of the current frame and delimited by the length parsed from the `IMessageMetadata`.

The main features of this project are provided within the [`IFrameDecoder`](https://github.com/thenameless314159/Andromeda.BedrockFramework/blob/master/src/Andromeda.Framing/IFrameDecoder.cs) and [`IFrameEncoder`](https://github.com/thenameless314159/Andromeda.BedrockFramework/blob/master/src/Andromeda.Framing/IFrameEncoder.cs) interfaces which are available once you have **implemented your metadata parsing logic**  using the [`MetadataParser<T>`](https://github.com/thenameless314159/Andromeda.BedrockFramework/blob/master/src/Andromeda.Framing/Metadata/MetadataParser.cs) abstract class. Here is an usage example on a hypothetic [`ConnectionHandler`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.connections.connectionhandler?view=aspnetcore-3.1) :


```csharp
using Andromeda.Framing;
using Andromeda.Framing.Metadata;

public class ServerConnectionHandler : ConnectionHandler
{
    public ServerConnectionHandler(IMetadataParser parser) => _parser = parser;
    private readonly IMetadataParser _parser;

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
	var token = connection.ConnectionClosed;
        var decoder = _parser.AsFrameDecoder(connection.Transport.Input);
        var encoder = _parser.AsFrameEncoder(connection.Transport.Output, token);

        await foreach (var frame in decoder.ReadFramesAsync(token))
        {
            var metadata = frame.Metadata as MyMessageMetadata ?? throw new ArgumentException();
            if(metadata.MessageId == 1)
                encoder.WriteAsync(new Frame(ReadOnlySequence<byte>.Empty, 
                    new MyMessageMetadata(messageId: 2, length: 0)))
        }
    }
}
```

## Andromeda.Framing.Extensions

Provides a base protocol/messaging layer with implementable dispatching behaviors. Also provides pooled encoder/decoder implementations and registration/build logic. You can look at the samples and unit tests for more infos until I wrote this part of the doc.
