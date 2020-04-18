using System;
using System.Buffers;
using System.Threading.Tasks;
using Andromeda.Framing.Behaviors;
using Andromeda.Framing.Extensions.Tests.Infrastructure;
using Andromeda.Framing.Extensions.Tests.Infrastructure.Models;
using Xunit;

namespace Andromeda.Framing.Extensions.Tests.Behaviors
{
    public class FrameDispatcherTests
    {
        private static readonly IFrameDispatcher _dispatcher = DispatcherProvider.Dispatcher;

        [Fact]
        public void Map_ShouldRegisterHandlerMapping()
        {
            var dispatcher = DispatcherProvider.Empty;
            Assert.Empty(dispatcher.Mappings);

            dispatcher.Map<HandshakeMessage>(1);
            Assert.NotEmpty(dispatcher.Mappings);
        }

        [Fact]
        public async Task OnFrameReceivedAsync_ShouldNotExecuteHandlerAction_OnConnectionClosed()
        {
            var frame = new Frame(ReadOnlySequence<byte>.Empty, new TestMessageMetadata(1, 0));
            var context = new TestHandlerContext();
            context.Close();

            Assert.Equal(DispatchResult.Cancelled, await _dispatcher.OnFrameReceivedAsync(in frame, context));
            context.VerifyNotAborted();
        }

        [Fact]
        public async Task OnFrameReceivedAsync_ShouldExecuteHandlerAction()
        {
            var context = new TestHandlerContext();
            var frame = new Frame(ReadOnlySequence<byte>.Empty, new TestMessageMetadata(1, 0));
            await _dispatcher.OnFrameReceivedAsync(in frame, context);
            context.VerifyAbort();
        }

        [Fact]
        public async Task OnFrameReceivedAsync_ShouldThrow_OnInvalidMessageMetadata() => await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            { // since IMessageMetadata is default and therefore not of type MessageMetadataWithId it should throws
                await DispatcherProvider.Empty.OnFrameReceivedAsync(in Frame.Empty, new TestHandlerContext());
            });

        [Fact]
        public async Task OnFrameReceivedAsync_ShouldThrow_OnNullContext() => await Assert.ThrowsAsync<ArgumentNullException>(
            async () => { await DispatcherProvider.Empty.OnFrameReceivedAsync(in Frame.Empty, default); });

        [Fact]
        public async Task OnFrameReceivedAsync_ShouldReturnInvalidFrameDispatchResult_WhenCouldNotDecodeMessage()
        {
            var frame = new Frame(ReadOnlySequence<byte>.Empty, new TestMessageMetadata(1, 0));
            var dispatcher = DispatcherProvider.Empty;
            dispatcher.Map<UnknownMessage>(1);

            Assert.Equal(DispatchResult.InvalidFrame,
                await dispatcher.OnFrameReceivedAsync(in frame, new TestHandlerContext()));
        }

        [Fact]
        public async Task OnFrameReceivedAsync_ShouldReturnNotMappedDispatchResult_WhenNotMapped()
        {
            var frame = new Frame(ReadOnlySequence<byte>.Empty, new TestMessageMetadata(5, 0));
            var dispatcher = DispatcherProvider.Empty;

            Assert.Equal(DispatchResult.HandlerNotMapped,
                await dispatcher.OnFrameReceivedAsync(in frame, new TestHandlerContext()));
        }

        [Fact]
        public async Task OnFrameReceivedAsync_ShouldReturnNotRegisteredDispatchResult_WhenMappedButNotRegistered()
        {
            var frame = new Frame(ReadOnlySequence<byte>.Empty, new TestMessageMetadata(1, 0));
            var dispatcher = DispatcherProvider.Empty;
            dispatcher.Map<EmptyMessage>(1);
            
            Assert.Equal(DispatchResult.HandlerNotRegistered,
                await dispatcher.OnFrameReceivedAsync(in frame, new TestHandlerContext()));
        }

        private class UnknownMessage { }
    }
}
