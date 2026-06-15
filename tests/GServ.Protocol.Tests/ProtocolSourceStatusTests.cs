using GServ.Protocol;

namespace GServ.Protocol.Tests;

public sealed class ProtocolSourceStatusTests
{
    [Fact]
    public void NumericPacketIdsRemainUnrecoveredUntilAuthoritativeEnumHeaderExists()
    {
        Assert.Equal("IEnums.h", PacketIdSourceStatus.AuthoritativeEnumHeader);
        Assert.False(PacketIdSourceStatus.NumericPacketIdsRecovered);
        Assert.Contains("does not include it", PacketIdSourceStatus.MissingNumericPacketIdsReason);
    }

    [Fact]
    public void WireConstantsMatchCppPacketHandlerShape()
    {
        Assert.Equal(2, ProtocolWireConstants.PacketBundleLengthPrefixBytes);
        Assert.Equal(10, ProtocolWireConstants.PacketTerminator);
        Assert.Equal(256, ProtocolWireConstants.HandlerTableSize);
    }
}
