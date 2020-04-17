using System;
using System.Collections.Generic;
using System.Reflection;
using Andromeda.Framing.Behaviors;
using Andromeda.Framing.Extensions.Behaviors;
using Andromeda.Framing.Extensions.Infrastructure;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Protocol;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace Andromeda.Framing
{
    public class ServerFramingBuilder
    {
        public ICollection<(int, Type)> Mappings { get; } = new List<(int, Type)>();
        public IFrameDecoderFactory DecoderFactory { get; set; }
        public IFrameEncoderFactory EncoderFactory { get; set; }
        public IServiceProvider ApplicationServices { get; }
        public IMessageReader MessageReader { get; set; }
        public IMessageWriter MessageWriter { get; set; }
        public IMetadataParser Parser { get; set; }

        public ServerFramingBuilder() : this(EmptyServiceProvider.Instance) { }

        public ServerFramingBuilder(IServiceProvider serviceProvider)
        {
            ApplicationServices = serviceProvider ?? throw new InvalidOperationException($"{nameof(IServiceProvider)} cannot be null !");
            MessageReader = serviceProvider.GetService<IMessageReader>(); // attempt to get already registered instances from the service provider
            MessageWriter = serviceProvider.GetService<IMessageWriter>();
            Parser = serviceProvider.GetService<IMetadataParser>();
        }

        public ServerFramingBuilder(IServiceProvider serviceProvider, BehaviorsBuilder behaviors)
        {
            ApplicationServices = serviceProvider ?? throw new InvalidOperationException($"{nameof(IServiceProvider)} cannot be null !");
            MessageReader = serviceProvider.GetService<IMessageReader>(); // attempt to get already registered instances from the service provider
            MessageWriter = serviceProvider.GetService<IMessageWriter>();
            Parser = serviceProvider.GetService<IMetadataParser>();

            foreach (var (id, handler) in behaviors._messageHandlers)
                Mappings.Add((id, handler.ServiceType.GetGenericArguments()[0]));
        }

        private static readonly MethodInfo _mapMi = typeof(FrameDispatcher).GetMethod(nameof(FrameDispatcher.Map));
        public ServerFraming Build()
        {
            if (MessageReader == default) throw new InvalidOperationException($"{nameof(IMessageReader)} must be setup !");
            if (MessageWriter == default) throw new InvalidOperationException($"{nameof(IMessageWriter)} must be setup !");
            if (Parser == default) throw new InvalidOperationException($"{nameof(IMetadataParser)} must be setup !");
            if(EncoderFactory == default) EncoderFactory = new DefaultEncoderFactory(Parser);
            if(DecoderFactory == default) DecoderFactory = new DefaultDecoderFactory(Parser);

            var dispatcher = new FrameDispatcher(ApplicationServices, MessageReader);
            foreach (var (id, message) in Mappings) _mapMi.MakeGenericMethod(message)
                .Invoke(dispatcher, new object[] { id });

            return new ServerFraming(DecoderFactory, EncoderFactory, dispatcher);
        }

        private class DefaultEncoderFactory : IFrameEncoderFactory
        {
            private readonly IMetadataEncoder _encoder;
            public DefaultEncoderFactory(IMetadataEncoder encoder) => _encoder = encoder;
            public IFrameEncoder Create(ConnectionContext fromContext) =>
                _encoder.AsFrameEncoder(fromContext.Transport.Output, fromContext.ConnectionClosed);
        }
        private class DefaultDecoderFactory : IFrameDecoderFactory
        {
            private readonly IMetadataDecoder _decoder;
            public DefaultDecoderFactory(IMetadataDecoder decoder) => _decoder = decoder;
            public IFrameDecoder Create(ConnectionContext fromContext) =>
                _decoder.AsFrameDecoder(fromContext.Transport.Input);
        }
    }
}
