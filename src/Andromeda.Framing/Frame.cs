using System.Buffers;
using Andromeda.Framing.Metadata;

namespace Andromeda.Framing
{
    public sealed class Frame
    {
        public ReadOnlySequence<byte> Payload { get; }
        public IMessageMetadata Metadata { get; }

        public Frame(ReadOnlySequence<byte> payload, IMessageMetadata metadata)
        {
            Payload = payload;
            Metadata = metadata;
        }
    }

    public static class FrameExtensions
    {
        public static Frame Copy(this Frame f) => new Frame(new ReadOnlySequence<byte>(f.Payload.ToArray()), f.Metadata);
    }
}
