using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Linq;

namespace Andromeda.Framing.Tests.Helpers
{
    internal static class FrameProvider
    {
        public static Memory<byte> GetMultiplesRandomAsBuffer(int messageId, params int[] framesLength)
        {
            var random = new Random();
            Memory<byte> buffer = new byte[6 * framesLength.Length + framesLength.Sum()];
            var offset = 0;
            foreach (var length in framesLength)
            {
                GetRandomAsBuffer((short)messageId, length, random)
                    .CopyTo(buffer.Slice(offset));

                offset += 6 + length;
            }

            return buffer;
        }

        public static ReadOnlySequence<byte> GetMultiplesRandom(int messageId, params int[] framesLength) =>
            new ReadOnlySequence<byte>(GetMultiplesRandomAsBuffer(messageId, framesLength));

        public static ReadOnlySequence<byte> GetRandom(short id, int length) => 
            new ReadOnlySequence<byte>(GetRandomAsBuffer(id, length));

        public static Memory<byte> GetRandomAsBuffer(short id, int length, Random random = default)
        {
            if(random == default) random = new Random();
            Memory<byte> buffer = new byte[length + 6];
            BinaryPrimitives.WriteInt16BigEndian(buffer.Span, id);
            BinaryPrimitives.WriteInt32BigEndian(buffer.Span.Slice(2), length);
            if (length > 0) random.NextBytes(buffer.Span.Slice(6));
            return buffer;
        }
    }
}
