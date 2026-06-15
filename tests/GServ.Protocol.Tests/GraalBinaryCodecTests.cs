using GServ.Protocol;

namespace GServ.Protocol.Tests;

public sealed class GraalBinaryCodecTests
{
    [Fact]
    public void GCharEncodingAddsAsciiSpaceOffset()
    {
        var writer = new GraalBinaryWriter();

        writer.WriteGChar(0);
        writer.WriteGChar(73);
        writer.WriteGChar(-32);

        Assert.Equal([32, 105, 0], writer.ToArray());
    }

    [Fact]
    public void GCharRoundTripsSignedValues()
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar(-32);
        writer.WriteGChar(-1);
        writer.WriteGChar(0);
        writer.WriteGChar(95);

        var reader = new GraalBinaryReader(writer.ToArray());

        Assert.Equal(-32, reader.ReadGChar());
        Assert.Equal(-1, reader.ReadGChar());
        Assert.Equal(0, reader.ReadGChar());
        Assert.Equal(95, reader.ReadGChar());
        Assert.True(reader.IsEmpty);
    }

    [Fact]
    public void FixedLengthStringUsesRawBytesWithoutTerminator()
    {
        var writer = new GraalBinaryWriter();

        writer.WriteFixedString("G3D0311C");

        Assert.Equal("G3D0311C"u8.ToArray(), writer.ToArray());
    }

    [Fact]
    public void LengthPrefixedStringUsesGCharLength()
    {
        var writer = new GraalBinaryWriter();

        writer.WriteLengthPrefixedString("account");

        Assert.Equal([39, 97, 99, 99, 111, 117, 110, 116], writer.ToArray());
        Assert.Equal("account", new GraalBinaryReader(writer.ToArray()).ReadLengthPrefixedString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(28_767)]
    public void GShortRoundTripsSupportedRange(int value)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGShort(value);

        var reader = new GraalBinaryReader(writer.ToArray());

        Assert.Equal(value, reader.ReadGUShort());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10_000)]
    [InlineData(3_682_303)]
    public void GIntRoundTripsSupportedRange(int value)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGInt(value);

        var reader = new GraalBinaryReader(writer.ToArray());

        Assert.Equal(value, reader.ReadGInt());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10_000)]
    [InlineData(471_347_295)]
    public void GInt4RoundTripsSupportedRange(int value)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGInt4(value);

        var reader = new GraalBinaryReader(writer.ToArray());

        Assert.Equal(value, reader.ReadGInt4());
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(10_000u)]
    [InlineData(uint.MaxValue)]
    public void GUInt5RoundTripsSupportedRange(uint value)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGUInt5(value);

        var reader = new GraalBinaryReader(writer.ToArray());

        Assert.Equal(value, reader.ReadGUInt5());
    }
}
