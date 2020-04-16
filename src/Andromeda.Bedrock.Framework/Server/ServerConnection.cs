using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;

namespace Andromeda.Bedrock.Framework
{
    internal sealed class ServerConnection : IConnectionEndPointFeature,
                                                IConnectionLifetimeNotificationFeature,
                                                IReadOnlyList<KeyValuePair<string, object>>
    {
        private readonly CancellationTokenSource _connectionClosingCts = new CancellationTokenSource();

        public ServerConnection(long id, ConnectionContext connectionContext)
        {
            Id = id;
            TransportConnection = connectionContext;

            connectionContext.Features.Set<IConnectionLifetimeNotificationFeature>(this);
            var endpointFeature = connectionContext.Features.Get<IConnectionEndPointFeature>();
            if (endpointFeature == null) connectionContext.Features.Set<IConnectionEndPointFeature>(this);

            ConnectionClosedRequested = _connectionClosingCts.Token;
        }

        public long Id { get; }
        public ConnectionContext TransportConnection { get; }
        public CancellationToken ConnectionClosedRequested { get; set; }
        public void RequestClose() => _connectionClosingCts.Cancel();

        // For logging to get the connection data
        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
                return new KeyValuePair<string, object>("ConnectionId", TransportConnection.ConnectionId);
            }
        }

        public int Count => 1;

        public EndPoint LocalEndPoint
        {
            get => TransportConnection.LocalEndPoint;
            set => TransportConnection.LocalEndPoint = value;
        }

        public EndPoint RemoteEndPoint
        {
            get => TransportConnection.RemoteEndPoint;
            set => TransportConnection.RemoteEndPoint = value;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() { for (var i = 0; i < Count; ++i) yield return this[i]; }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => TransportConnection.ConnectionId;
    }
}
