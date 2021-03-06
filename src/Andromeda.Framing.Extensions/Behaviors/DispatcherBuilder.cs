﻿using System;
using System.Linq;
using System.Reflection;
using Andromeda.Framing.Metadata;
using Andromeda.Framing.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace Andromeda.Framing.Behaviors
{
    public class DispatcherBuilder
    { 
        public IMessageReader Decoder { get; set; }

        public IServiceProvider ApplicationServices { get; }
        private readonly BehaviorsBuilder _behaviors;

        public DispatcherBuilder(IServiceProvider serviceProvider, BehaviorsBuilder behaviors)
        {
            ApplicationServices = serviceProvider ?? throw new InvalidOperationException($"{nameof(IServiceProvider)} cannot be null !");
            _behaviors = behaviors ?? throw new InvalidOperationException($"{nameof(BehaviorsBuilder)} cannot be null !");
            Decoder = serviceProvider.GetService<IMessageReader>();
        }

        private static readonly MethodInfo _mapMi = typeof(FrameDispatcher).GetMethod(nameof(FrameDispatcher.Map));
        public IFrameDispatcher Build()
        {
            if (!_behaviors._messageHandlers.Any()) throw new InvalidOperationException($"At least one {nameof(_behaviors._messageHandlers)} must be setup !");
            if (Decoder == default) throw new InvalidOperationException($"{nameof(IMessageEncoder)} must be setup !");

            var dispatcher = new FrameDispatcher(ApplicationServices, Decoder);
            foreach (var (id, handler) in _behaviors._messageHandlers)
                _mapMi.MakeGenericMethod(handler.ServiceType.GetGenericArguments()[0])
                    .Invoke(dispatcher, new object[] { id });

            return dispatcher;
        }
    }
}
