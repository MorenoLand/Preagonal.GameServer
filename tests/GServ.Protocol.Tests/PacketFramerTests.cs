using GServ.Protocol;

namespace GServ.Protocol.Tests;

public sealed class PacketFramerTests
{
    [Fact]
    public void FrameForSendAppendsNewlineWhenMissing()
    {
        byte[] packet = [105, 1, 2, 3];

        var framed = PacketFramer.FrameForSend(packet);

        Assert.Equal([105, 1, 2, 3, 10], framed);
    }

    [Fact]
    public void FrameForSendDoesNotAppendSecondNewline()
    {
        byte[] packet = [105, 10];

        var framed = PacketFramer.FrameForSend(packet);

        Assert.Equal([105, 10], framed);
    }

    [Fact]
    public void FrameForSendLeavesRawPacketsUnterminated()
    {
        byte[] packet = [132, 0, 1, 2];

        var framed = PacketFramer.FrameForSend(packet, appendNewline: false);

        Assert.Equal(packet, framed);
    }
}
