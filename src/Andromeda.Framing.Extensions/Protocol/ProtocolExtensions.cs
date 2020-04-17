using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Andromeda.Framing.Protocol
{
    public static class ProtocolExtensions
    {
        public static ValueTask<FlushResult> EncodeAsFrame<T>(this IMessageWriter e, PipeWriter writer, in T message,
            CancellationToken token = default)
        {
            e.Encode(writer, message);
            return writer.FlushAsync(token);
        }

        public static bool TryParse<T>(this IMessageReader r, in Frame frame, T message)
        {
            var payload = frame.Payload;
            return r.TryParse(in payload, message);
        }
    }
}
