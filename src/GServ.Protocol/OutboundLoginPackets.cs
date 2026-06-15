using System.Text;

namespace GServ.Protocol;

public static class OutboundLoginPackets
{
    public static byte[] Signature()
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.Signature);
        writer.WriteGChar(73);
        return writer.ToArray();
    }

    public static byte[] DisconnectMessage(string message, bool appendNewline = false)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.DisconnectMessage);
        writer.WriteBytes(Encoding.ASCII.GetBytes(message));
        if (appendNewline)
            writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }
}
