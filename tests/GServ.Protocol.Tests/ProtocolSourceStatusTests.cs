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

    [Fact]
    public void ProtocolCriticalCppDependencyHeadersRemainUnrecovered()
    {
        Assert.Equal("gs2lib", ProtocolDependencySourceStatus.ExpectedSourceDependency);
        Assert.Equal("gs2lib_SOURCE_DIR/include", ProtocolDependencySourceStatus.ExpectedSourceIncludePath);
        Assert.False(ProtocolDependencySourceStatus.IEnumsHeaderRecovered);
        Assert.False(ProtocolDependencySourceStatus.CStringHeaderRecovered);
        Assert.False(ProtocolDependencySourceStatus.CEncryptionHeaderRecovered);
        Assert.False(ProtocolDependencySourceStatus.CFileQueueHeaderRecovered);
        Assert.False(ProtocolDependencySourceStatus.CSocketHeaderRecovered);
    }
}
