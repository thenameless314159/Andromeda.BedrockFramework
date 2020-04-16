using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andromeda.Framing
{
    public interface IFrameEncoder : IDisposable
    {
        long FramesWritten { get; }
        ValueTask WriteAsync(in Frame frame);
        ValueTask WriteAsync(IEnumerable<Frame> frames);
        ValueTask WriteAsync(IAsyncEnumerable<Frame> frames);
    }
}
