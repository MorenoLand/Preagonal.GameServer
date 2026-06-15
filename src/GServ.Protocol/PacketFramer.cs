namespace GServ.Protocol;

/// <summary>
/// Applies the outbound packet newline rule from Player::sendPacket.
/// Source: ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp.
/// </summary>
public static class PacketFramer
{
    public const byte NewLine = 10;

    public static byte[] FrameForSend(ReadOnlySpan<byte> packet, bool appendNewline = true)
    {
        if (packet.IsEmpty)
        {
            return [];
        }

        if (!appendNewline || packet[^1] == NewLine)
        {
            return packet.ToArray();
        }

        var framed = new byte[packet.Length + 1];
        packet.CopyTo(framed);
        framed[^1] = NewLine;
        return framed;
    }
}
