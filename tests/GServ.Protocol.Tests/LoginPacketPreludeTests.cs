using GServ.Protocol;

namespace GServ.Protocol.Tests;

public sealed class LoginPacketPreludeTests
{
    [Theory]
    [InlineData(32, 1)]
    [InlineData(33, 2)]
    [InlineData(34, 4)]
    [InlineData(37, 32)]
    public void DecodeSessionTypeBitMaskMatchesCppShiftRule(byte encodedTypeByte, int expectedMask)
    {
        Assert.Equal(expectedMask, LoginPacketPrelude.DecodeSessionTypeBitMask(encodedTypeByte));
    }

    [Fact]
    public void DecodeSessionTypeBitMaskRejectsBytesBelowGraalOffset()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoginPacketPrelude.DecodeSessionTypeBitMask(31));
    }
}
