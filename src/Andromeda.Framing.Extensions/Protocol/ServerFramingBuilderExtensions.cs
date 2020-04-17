using Andromeda.Framing.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace Andromeda.Framing
{
    public static class ServerFramingBuilderExtensions
    {
        public static ServerFramingBuilder UseMessageEncoderWith<T>(this ServerFramingBuilder builder)
            where T : MessageAsFrameWriter
        {
            builder.MessageWriter = builder.ApplicationServices.GetRequiredService<T>();
            builder.EncoderFactory = new MessageEncoderFactory(builder.Parser, builder.MessageWriter);
            return builder;
        }

        public static ServerFramingBuilder UseMessageEncoder(this ServerFramingBuilder builder)
        {
            builder.EncoderFactory = new MessageEncoderFactory(builder.Parser, builder.MessageWriter);
            return builder;
        }
    }
}
