using System;
using System.Collections.Generic;
using System.Threading;

namespace Andromeda.Framing
{
    public interface IFrameDecoder : IDisposable
    {
        IAsyncEnumerable<Frame> ReadFramesAsync(CancellationToken token = default);
    }
}
