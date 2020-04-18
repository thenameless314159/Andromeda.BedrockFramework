using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Moq;

namespace Andromeda.Framing.Extensions.Tests.Infrastructure
{
    public class TestConnectionContext : ConnectionContext
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly Mock<ConnectionContext> _mock = new Mock<ConnectionContext>();

        public override string ConnectionId { get; set; } = new Guid().ToString();
        public override IFeatureCollection Features { get; } = new FeatureCollection();
        public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
        public override CancellationToken ConnectionClosed
        {
            get => _tokenSource.Token; 
            set => throw new InvalidOperationException();
        }

        public override IDuplexPipe Transport { get; set; }

        public override void Abort() => _mock.Object.Abort();

        public void Close() => _tokenSource.Cancel();
        public void VerifyAbort() => _mock.Verify(c => c.Abort());
        public void VerifyNotAborted() => _mock.Verify(c => c.Abort(), Times.Never);
    }
}
