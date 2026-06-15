using GServ.Game;

namespace GServ.Game.Tests;

public sealed class NwLevelPacketBuilderTests
{
    [Fact]
    public void BuildBoardPacketWritesPloBoardPacketRawLittleEndianTilesAndNewline()
    {
        var parsed = NwLevelParser.Parse("""
            GLEVNW01
            BOARD 0 0 2 0 AB+/
            """);

        var packet = NwLevelPacketBuilder.BuildBoardPacket(parsed.Level);

        Assert.Equal(1 + 64 * 64 * 2 + 1, packet.Length);
        Assert.Equal(133, packet[0]);
        Assert.Equal([1, 0], packet[1..3]);
        Assert.Equal([191, 15], packet[3..5]);
        Assert.Equal(10, packet[^1]);
    }

    [Fact]
    public void BuildLayerPacketWritesLayerHeaderRawLittleEndianTilesAndNewline()
    {
        var parsed = NwLevelParser.Parse("""
            GLEVNW01
            BOARD 0 0 1 1 +/
            """);

        var packet = NwLevelPacketBuilder.BuildLayerPacket(parsed.Level, 1);

        Assert.Equal(1 + 5 + 64 * 64 * 2 + 1, packet.Length);
        Assert.Equal(139, packet[0]);
        Assert.Equal([1, 0, 0, 64, 64], packet[1..6]);
        Assert.Equal([191, 15], packet[6..8]);
        Assert.Equal(10, packet[^1]);
    }
}
