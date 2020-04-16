using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.AspNetCore.Connections;

namespace Andromeda.Bedrock.Framework
{
    public class EndPointBinding : ServerBinding
    {
        private readonly IConnectionListenerFactory _listenerFactory;
        private readonly EndPoint _endPoint;

        public EndPointBinding(EndPoint endPoint, ConnectionDelegate application, IConnectionListenerFactory connectionListenerFactory)
        {
            _listenerFactory = connectionListenerFactory;
            Application = application;
            _endPoint = endPoint;
        }

        public override ConnectionDelegate Application { get; }

        public override async IAsyncEnumerable<IConnectionListener> BindAsync([EnumeratorCancellation]CancellationToken token = default)
        {
            yield return await _listenerFactory.BindAsync(_endPoint, token);
        }

        public override string ToString() => _endPoint?.ToString();
    }
}
