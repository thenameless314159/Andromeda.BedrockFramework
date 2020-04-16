using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Connections;

namespace Andromeda.Bedrock.Framework
{
    public abstract class ServerBinding
    {
        public virtual ConnectionDelegate Application { get; }

        public abstract IAsyncEnumerable<IConnectionListener> BindAsync(CancellationToken cancellationToken = default);
    }
}
