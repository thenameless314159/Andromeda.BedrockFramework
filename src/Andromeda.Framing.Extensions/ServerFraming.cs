using Andromeda.Framing.Behaviors;
using Microsoft.AspNetCore.Connections;

namespace Andromeda.Framing
{
    public sealed class ServerFraming
    {
        private readonly IFrameDecoderFactory _decoderFactory;
        private readonly IFrameEncoderFactory _encoderFactory;

        public IFrameDispatcher Dispatcher { get; }

        internal ServerFraming(IFrameDecoderFactory decoderFactory, IFrameEncoderFactory encoderFactory, IFrameDispatcher dispatcher)
        {
            _decoderFactory = decoderFactory;
            _encoderFactory = encoderFactory;
            Dispatcher = dispatcher;
        }

        public (IFrameDecoder Decoder, IFrameEncoder Encoder) CreatePair(ConnectionContext fromContext) => (
            _decoderFactory.Create(fromContext),
            _encoderFactory.Create(fromContext));
    }
}
