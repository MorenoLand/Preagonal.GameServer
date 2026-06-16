using System.Net;
using System.Net.Sockets;
using GServ.Network;
using Xunit;

namespace GServ.Network.Tests;

public sealed class ProductionTcpServerTests
{
    [Fact]
    public async Task AcceptOneAsyncAssignsPlayerIdFromCppInitialValueAndDispatchesBufferedFrames()
    {
        var handler = new RecordingProductionFrameHandler(expectedFrames: 2);
        using var server = new ProductionTcpServer(IPAddress.Loopback, port: 0, handler);
        server.Start();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var acceptTask = server.AcceptOneAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, server.Port, timeout.Token);
        await using var stream = client.GetStream();

        await stream.WriteAsync(new byte[] { 0, 3, 65 }, timeout.Token);
        await stream.WriteAsync(new byte[] { 66, 67, 0, 1, 68 }, timeout.Token);
        await handler.WaitForExpectedFrames(timeout.Token);
        client.Close();

        var result = await acceptTask;

        Assert.Equal(2, result.PlayerId);
        Assert.Equal(2, handler.Sessions.Single());
        Assert.Equal([new byte[] { 65, 66, 67 }, new byte[] { 68 }], handler.Frames);
        Assert.Equal(ProductionTcpSessionStopReason.ClientDisconnected, result.StopReason);
    }

    [Fact]
    public async Task AcceptOneAsyncWritesHandlerOutboundBytesAfterFrameDispatch()
    {
        var handler = new EchoProductionFrameHandler();
        using var server = new ProductionTcpServer(IPAddress.Loopback, port: 0, handler);
        server.Start();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var acceptTask = server.AcceptOneAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, server.Port, timeout.Token);
        await using var stream = client.GetStream();
        await stream.WriteAsync(new byte[] { 0, 1, 70 }, timeout.Token);

        var response = new byte[1];
        var read = await stream.ReadAsync(response, timeout.Token);
        client.Close();
        var result = await acceptTask;

        Assert.Equal(1, read);
        Assert.Equal(71, response[0]);
        Assert.Equal(ProductionTcpSessionStopReason.ClientDisconnected, result.StopReason);
    }

    private sealed class RecordingProductionFrameHandler(int expectedFrames) : IProductionSocketFrameHandler
    {
        private readonly TaskCompletionSource _expectedFramesSeen = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public List<ushort> Sessions { get; } = [];
        public List<byte[]> Frames { get; } = [];

        public ValueTask<ProductionSocketFrameResult> HandleFrameAsync(
            ProductionSocketSessionContext session,
            ReadOnlyMemory<byte> frame,
            CancellationToken cancellationToken)
        {
            if (!Sessions.Contains(session.PlayerId))
                Sessions.Add(session.PlayerId);

            Frames.Add(frame.ToArray());
            if (Frames.Count == expectedFrames)
                _expectedFramesSeen.TrySetResult();

            return ValueTask.FromResult(ProductionSocketFrameResult.Continue());
        }

        public async Task WaitForExpectedFrames(CancellationToken cancellationToken)
        {
            await using var registration = cancellationToken.Register(() => _expectedFramesSeen.TrySetCanceled(cancellationToken));
            await _expectedFramesSeen.Task;
        }
    }

    private sealed class EchoProductionFrameHandler : IProductionSocketFrameHandler
    {
        public ValueTask<ProductionSocketFrameResult> HandleFrameAsync(
            ProductionSocketSessionContext session,
            ReadOnlyMemory<byte> frame,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(ProductionSocketFrameResult.Continue([unchecked((byte)(frame.Span[0] + 1))]));
        }
    }
}
