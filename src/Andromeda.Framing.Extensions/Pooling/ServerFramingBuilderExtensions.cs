using Andromeda.Framing.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Andromeda.Framing.Extensions.Pooling
{
    public static class ServerFramingBuilderExtensions
    {
        public static ServerFramingBuilder UseMessageEncoderPoolingWith<T>(this ServerFramingBuilder builder)
            where T : MessageAsFrameWriter
        {
            var poolProvider = builder.ApplicationServices.GetRequiredService<ObjectPoolProvider>();
            builder.MessageWriter = builder.ApplicationServices.GetRequiredService<T>();
            var pool = poolProvider.Create(new PooledMessageEncoderPolicy(builder.Parser, builder.MessageWriter));
            builder.EncoderFactory = new PooledMessageEncoderFactory(pool);
            return builder;
        }

        public static ServerFramingBuilder UseMessageEncoderPooling(this ServerFramingBuilder builder)
        {
            var poolProvider = builder.ApplicationServices.GetRequiredService<ObjectPoolProvider>();
            var pool = poolProvider.Create(new PooledMessageEncoderPolicy(builder.Parser, builder.MessageWriter));
            builder.EncoderFactory = new PooledMessageEncoderFactory(pool);
            return builder;
        }

        public static ServerFramingBuilder UseEncoderPooling(this ServerFramingBuilder builder)
        {
            var poolProvider = builder.ApplicationServices.GetRequiredService<ObjectPoolProvider>();
            var pool = poolProvider.Create(new PooledFrameEncoderPolicy(builder.Parser));
            builder.EncoderFactory = new PooledFrameEncoderFactory(pool);
            return builder;
        }

        public static ServerFramingBuilder UseDecoderPooling(this ServerFramingBuilder builder)
        {
            var poolProvider = builder.ApplicationServices.GetRequiredService<ObjectPoolProvider>();
            var pool = poolProvider.Create(new PooledFrameDecoderPolicy(builder.Parser));
            builder.DecoderFactory = new PooledFrameDecoderFactory(pool);
            return builder;
        }
    }
}
