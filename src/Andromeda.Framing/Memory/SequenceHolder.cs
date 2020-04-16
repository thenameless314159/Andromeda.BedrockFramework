using System.Buffers;

namespace Andromeda.Framing.Memory
{
    // no ref parameters in async method, we need this class
    public class SequenceHolder
    {
        public ReadOnlySequence<byte> Buffer { get; set; }

        public SequenceHolder(ReadOnlySequence<byte> buffer) => Buffer = buffer;

        public SequenceHolder()
        {
        }
    }
}
