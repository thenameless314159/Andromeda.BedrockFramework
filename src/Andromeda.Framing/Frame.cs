using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Andromeda.Framing.Metadata;

namespace Andromeda.Framing
{
    public sealed class Frame
    {
        public static Frame Empty = new Frame(ReadOnlySequence<byte>.Empty, new EmptyMetadata());
        public static IEnumerable<Frame> EmptyFrames = Enumerable.Empty<Frame>();
        
        public ReadOnlySequence<byte> Payload { get; }
        public IMessageMetadata Metadata { get; }

        public Frame(ReadOnlySequence<byte> payload, IMessageMetadata metadata)
        {
            Payload = payload;
            Metadata = metadata;
        }

        private class EmptyMetadata : IMessageMetadata { public int Length => 0; }
    }

    public static class FrameExtensions
    {
        public static Frame Copy(this Frame f) => new Frame(new ReadOnlySequence<byte>(f.Payload.ToArray()), f.Metadata);
    }
}
