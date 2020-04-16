using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Andromeda.Bedrock.Framework.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Andromeda.Bedrock.Framework
{
    public class Server
    {
        private readonly List<ServerListener> _runningListeners = new List<ServerListener>();
        private readonly TaskCompletionSource<object> _shutdownTcs;
        private readonly ILogger<Server> _logger;
        private readonly ServerBuilder _builder;

        public IEnumerable<EndPoint> EndPoints => _runningListeners.Select(l => l.Listener.EndPoint);

        internal Server(ServerBuilder builder)
        {
            _shutdownTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _logger = builder.ApplicationServices.GetLoggerFactory().CreateLogger<Server>();
            _builder = builder;
        }

        public async Task StartAsync(CancellationToken token = default)
        {
            try
            {
                foreach (var binding in _builder.Bindings)
                {
                    await foreach (var l in binding.BindAsync(token).ConfigureAwait(false))
                    {
                        var listener = new ServerListener(l, binding.Application, _shutdownTcs.Task,
                            _builder.ShutdownTimeout, _logger);
                        _runningListeners.Add(listener);
                        listener.Start();
                    }
                }
            }
            catch
            {
                await StopAsync(default).ConfigureAwait(false);
                throw; // rethrow unexpected exception
            }
        }

        public async Task StopAsync(CancellationToken token = default)
        {
            var tasks = new Task[_runningListeners.Count];
            for (var i = 0; i < _runningListeners.Count; i++)
                tasks[i] = _runningListeners[i].Listener.UnbindAsync(token)
                    .AsTask();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            // Signal to all of the listeners that it's time to start the shutdown process
            // We call this after unbind so that we're not touching the listener anymore (each loop will dispose the listener)
            _shutdownTcs.TrySetResult(null);
            for (var i = 0; i < _runningListeners.Count; i++)
                tasks[i] = _runningListeners[i].ExecutionTask;

            var shutdownTask = Task.WhenAll(tasks);
            if (token.CanBeCanceled)
                await shutdownTask.WithCancellation(token).ConfigureAwait(false);
            else
                await shutdownTask.ConfigureAwait(false);
        }
    }
}
