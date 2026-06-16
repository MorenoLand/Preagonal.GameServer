using System.Text;
using GServ.Protocol;
using Xunit;

namespace GServ.Protocol.Tests;

public sealed class InboundPacketDecoderTests
{
    [Fact]
    public void Gen5UncompressedFramePayloadDecodesConfirmedFixture()
    {
        var decoder = new InboundPacketDecoder(EncryptionGeneration.Gen5, key: 0);

        var result = decoder.DecodeSocketFramePayload([0x02, 0x79, 0x7A, 0xB2, 0xDC]);

        var packet = Assert.Single(result.Packets);
        Assert.Equal("abc", Encoding.ASCII.GetString(packet));
    }

    [Fact]
    public void Gen5ZlibFramePayloadDecodesConfirmedFixture()
    {
        var decoder = new InboundPacketDecoder(EncryptionGeneration.Gen5, key: 0);

        var result = decoder.DecodeSocketFramePayload([
            0x04, 0x60, 0x84, 0x9A, 0x9A, 0x5C, 0xD3, 0x31,
            0x82, 0x58, 0x46, 0x1C, 0x13, 0x5A
        ]);

        var packet = Assert.Single(result.Packets);
        Assert.Equal(new string('a', 55), Encoding.ASCII.GetString(packet));
    }

    [Fact]
    public void Gen2ZlibFramePayloadDecodesConfirmedFixture()
    {
        var decoder = new InboundPacketDecoder(EncryptionGeneration.Gen2, key: 0);

        var result = decoder.DecodeSocketFramePayload([
            0x78, 0x9C, 0x4B, 0x4C, 0x4A, 0xE6, 0x02, 0x00, 0x03, 0x7E, 0x01, 0x31
        ]);

        var packet = Assert.Single(result.Packets);
        Assert.Equal("abc", Encoding.ASCII.GetString(packet));
    }
}
