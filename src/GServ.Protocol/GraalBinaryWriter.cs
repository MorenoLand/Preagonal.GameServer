using System.Text;
using GServ.Core.Compatibility;

namespace GServ.Protocol;

/// <summary>
/// Writes Graal protocol primitive values.
/// Source mapping: C++ CString writeG* calls. Exact non-GChar behavior must be reverified
/// when CString.h is recovered.
/// </summary>
public sealed class GraalBinaryWriter
{
    private static readonly Encoding WireEncoding = Encoding.Latin1;
    private readonly List<byte> _buffer = [];

    public int Length => _buffer.Count;

    public void WriteByte(byte value) => _buffer.Add(value);

    public void WriteBytes(ReadOnlySpan<byte> value)
    {
        foreach (var b in value)
        {
            _buffer.Add(b);
        }
    }

    public void WriteGChar(int value)
    {
        _buffer.Add(unchecked((byte)(value + CompatibilityConstants.GraalAsciiOffset)));
    }

    public void WriteGShort(int value)
    {
        var clamped = Math.Clamp(value, 0, 28_767);
        var high = Math.Min(clamped >> 7, 223);
        var low = clamped - (high << 7);

        WriteGChar(high);
        WriteGChar(low);
    }

    public void WriteGInt(int value)
    {
        var clamped = Math.Clamp(value, 0, 3_682_303);
        var b0 = Math.Min(clamped >> 14, 223);
        clamped -= b0 << 14;
        var b1 = Math.Min(clamped >> 7, 223);
        clamped -= b1 << 7;

        WriteGChar(b0);
        WriteGChar(b1);
        WriteGChar(clamped);
    }

    public void WriteGInt4(int value)
    {
        var clamped = Math.Clamp(value, 0, 471_347_295);
        var b0 = Math.Min(clamped >> 21, 223);
        clamped -= b0 << 21;
        var b1 = Math.Min(clamped >> 14, 223);
        clamped -= b1 << 14;
        var b2 = Math.Min(clamped >> 7, 223);
        clamped -= b2 << 7;

        WriteGChar(b0);
        WriteGChar(b1);
        WriteGChar(b2);
        WriteGChar(clamped);
    }

    public void WriteGUInt5(uint value)
    {
        var b0 = Math.Min((int)(value >> 28), 15);
        value -= (uint)b0 << 28;
        var b1 = Math.Min((int)(value >> 21), 223);
        value -= (uint)b1 << 21;
        var b2 = Math.Min((int)(value >> 14), 223);
        value -= (uint)b2 << 14;
        var b3 = Math.Min((int)(value >> 7), 223);
        value -= (uint)b3 << 7;

        WriteGChar(b0);
        WriteGChar(b1);
        WriteGChar(b2);
        WriteGChar(b3);
        WriteGChar((int)value);
    }

    public void WriteLengthPrefixedString(string value)
    {
        var bytes = WireEncoding.GetBytes(value);
        if (bytes.Length > 191)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "GChar length-prefixed strings cannot exceed 191 bytes.");
        }

        WriteGChar(bytes.Length);
        WriteBytes(bytes);
    }

    public void WriteFixedString(string value) => WriteBytes(WireEncoding.GetBytes(value));

    public byte[] ToArray() => [.. _buffer];
}
