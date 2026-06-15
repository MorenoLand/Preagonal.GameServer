using GServ.Core.Compatibility;

namespace GServ.Protocol;

/// <summary>
/// Reads the first byte of the original login packet.
/// C++ source: PlayerLogin::msgLoginPacket computes m_type = (1 &lt;&lt; pPacket.readGChar()).
/// </summary>
public static class LoginPacketPrelude
{
    public static int DecodeSessionTypeBitMask(byte encodedTypeByte)
    {
        var typeIndex = encodedTypeByte - CompatibilityConstants.GraalAsciiOffset;
        if (typeIndex < 0 || typeIndex >= 31)
        {
            throw new ArgumentOutOfRangeException(nameof(encodedTypeByte), "Login session type index must fit in a signed 32-bit bit mask.");
        }

        return 1 << typeIndex;
    }
}
