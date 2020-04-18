# <p align="center"> <img src="https://gamepedia.cursecdn.com/minecraft_fr_gamepedia/e/ee/Bedrock.png" width="32" height="32" style="margin-bottom:-5"> Andromeda.Bedrock.Framework - [![Build Status](https://travis-ci.com/thenameless314159/Andromeda.BedrockFramework.svg?branch=master)](https://travis-ci.com/thenameless314159/Andromeda.BedrockFramework) </p>

<div style="text-align:center"><p align="center"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/andromeda_icon2.png?token=AFMTCCLAUUAALOP5UR4TWWC6JQ6Y6" width="140" height="158"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/ASP.NET-Core-Logo_2colors_Square_RGB.png?token=AFMTCCNPNVM6MBG7AF6E75K6JQTHI" width="180" height="168"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/NET-Core-Logo_2colors_Square_RGB.png?token=AFMTCCNORD45RRHKSS456HK6JQTJU" width="180" height="168"></p></div>

This project is a fork of the [*Project Bedrock*](https://github.com/aspnet/AspNetCore/issues/4772) which provides a **different protocol/messaging** logic and a **new framing layer** decoupled from the protocol logic. At this point, the project was only made to suits my needs, therefore the **BedrockFramework** layer has some differences with the original :

- No other protocol implementation, only the base socket layers with the same APIs
- No more `IConnectionHeartbeatFeature` and `IConnectionCompleteFeature` in the server `ConnectionContext`

For further infos of the original project, see the presentation [here](https://speakerdeck.com/davidfowl/project-bedrock)

# WIP
