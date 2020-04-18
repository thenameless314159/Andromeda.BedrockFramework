using System.Buffers;
using System.Text;

namespace Protocol
{
    public static class SequenceExtensions
    {
        public static string AsString(this in ReadOnlySequence<byte> buffer, Encoding useEncoding = default)
        {
            if (buffer.IsEmpty) return string.Empty;
            var encoding = useEncoding ?? Encoding.UTF8;
            if (buffer.IsSingleSegment) return encoding.GetString(buffer.First.Span);

            return string.Create((int)buffer.Length, buffer, (span, sequence) => {
                foreach (var segment in sequence)
                {
                    encoding.GetChars(segment.Span, span);
                    span = span.Slice(segment.Length);
                }
            });
        }

        public static ReadOnlySequence<byte> GetRemainingSequence(this ref SequenceReader<byte> r) =>
            r.Sequence.Slice(r.Position, r.Sequence.End);
    }
}
