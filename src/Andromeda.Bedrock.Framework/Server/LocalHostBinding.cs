using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Connections;

namespace Andromeda.Bedrock.Framework
{
    public sealed class LocalHostBinding : ServerBinding
    {
        private readonly IConnectionListenerFactory _listenerFactory;
        private readonly int _port;

        public LocalHostBinding(int port, ConnectionDelegate application, IConnectionListenerFactory connectionListenerFactory)
        {
            _listenerFactory = connectionListenerFactory;
            Application = application;
            _port = port;
        }

        public override ConnectionDelegate Application { get; }

        public override async IAsyncEnumerable<IConnectionListener> BindAsync([EnumeratorCancellation]CancellationToken token = default)
        {
            IConnectionListener ipv6Listener = null;
            IConnectionListener ipv4Listener = null;
            var exceptions = new List<Exception>();

            try { ipv6Listener = await _listenerFactory.BindAsync(new IPEndPoint(IPAddress.IPv6Loopback, _port), token); }
            catch (Exception ex) when (!(ex is IOException)) { exceptions.Add(ex); }

            if (ipv6Listener != null) yield return ipv6Listener;

            try { ipv4Listener = await _listenerFactory.BindAsync(new IPEndPoint(IPAddress.Loopback, _port), token); }
            catch (Exception ex) when (!(ex is IOException)) { exceptions.Add(ex); }

            if (exceptions.Count == 2) throw new IOException($"Failed to bind to {this}", new AggregateException(exceptions));
            if (ipv4Listener != null) yield return ipv4Listener;
        }

        public override string ToString() => $"localhost:{_port}";
    }
}
