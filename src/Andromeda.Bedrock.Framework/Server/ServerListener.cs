using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PooledAwait;

namespace Andromeda.Bedrock.Framework
{
    internal sealed class ServerListener
    {
        private readonly ConcurrentDictionary<long, (ServerConnection Connection, Task ExecutionTask)> _connections;
        private readonly ConnectionDelegate _application;
        private readonly TimeSpan _shutdownTimeout;
        private readonly Task _shutdownTask;
        private readonly ILogger _logger;

        public IConnectionListener Listener { get; }
        public Task ExecutionTask { get; private set; }

        public ServerListener(IConnectionListener listener, ConnectionDelegate application, Task shutdownTask = default,
            TimeSpan shutdownTimeout = default, ILogger<Server> logger = default)
        {
            _connections = new ConcurrentDictionary<long, (ServerConnection Context, Task ExecutionTask)>();
            _shutdownTimeout = shutdownTimeout == default ? TimeSpan.FromSeconds(3) : shutdownTimeout;
            _shutdownTask = shutdownTask ?? Task.CompletedTask;
            _logger = logger ?? NullLogger<Server>.Instance;
            _application = application;
            Listener = listener;
        }

        public void Start() => ExecutionTask = RunListenerAsync();

        private async Task RunListenerAsync()
        {
            var executePipeline = _application;
            await using var listener = Listener;
            long id = 0;

            for(;;)
            {
                id++;
                try
                {
                    var connection = await listener.AcceptAsync().ConfigureAwait(false);
                    // Null means we don't have anymore connections
                    if (connection == null) break;

                    var networkConnection = new ServerConnection(id, connection);
                    _connections[id] = (networkConnection, ExecuteConnectionAsync(networkConnection, executePipeline));
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex) {
                    _logger.LogCritical(ex, "Stopped accepting connections on {endpoint}", listener.EndPoint);
                    break;
                }
            }

            // Don't shut down connections until entire server is shutting down
            await _shutdownTask.ConfigureAwait(false);

            // Give connections a chance to close gracefully
            var tasks = new List<Task>(_connections.Count);
            foreach (var (_, (connection, task)) in _connections)
            {
                connection.RequestClose();
                tasks.Add(task);
            }

            if (await Task.WhenAll(tasks).TimeoutAfter(_shutdownTimeout).ConfigureAwait(false))
                return;

            foreach (var (_, (connection, _)) in _connections)
                connection.TransportConnection.Abort();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async PooledTask ExecuteConnectionAsync(ServerConnection connection, ConnectionDelegate execute)
        {
            await Task.Yield();
            var endpoint = Listener.EndPoint;
            var transport = connection.TransportConnection;
            _logger.LogTrace(NewConnection, connection, endpoint);

            try
            {
                using var scope = BeginConnectionScope(connection);
                await execute(transport).ConfigureAwait(false);
            }
            catch (ConnectionAbortedException) { /* Don't let connection aborted exceptions out */ }
            catch (ConnectionResetException) { /* Don't let connection aborted exceptions out */ }
            catch (Exception ex) { _logger.LogError(ex, Unexpected, connection); }
            finally
            {
                await transport.DisposeAsync().ConfigureAwait(false);

                // Remove the connection from tracking
                _connections.TryRemove(connection.Id, out _);
                _logger.LogTrace(Disconnection, connection, endpoint);
            }
        }

        private IDisposable BeginConnectionScope(ServerConnection connection) => _logger.IsEnabled(LogLevel.Critical)
            ? _logger.BeginScope(connection)
            : null;

        private const string NewConnection = "A new connection with id='{Connection}' has successfully been accepted on server at {EndPoint} !";
        private const string Disconnection = "Connection with id='{Connection}' has succesffuly been disconnected on server at {EndPoint}!";
        private const string Unexpected = "An unexpected exception has been caught from connection with id='{Connection}'";
    }
}
