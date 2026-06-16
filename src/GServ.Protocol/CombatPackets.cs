namespace GServ.Protocol;

public static class CombatPackets
{
    public static byte[] BombAdd(ushort playerId, ReadOnlySpan<byte> clientPacket, bool appendNewline = false)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.BombAdd);
        writer.WriteGShort(playerId);
        WriteAfterClientOpcode(writer, clientPacket);
        AppendNewline(writer, appendNewline);
        return writer.ToArray();
    }

    public static byte[] BombDelete(ReadOnlySpan<byte> clientPacket, bool appendNewline = false)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.BombDelete);
        WriteAfterClientOpcode(writer, clientPacket);
        AppendNewline(writer, appendNewline);
        return writer.ToArray();
    }

    public static byte[] ArrowAdd(ushort playerId, ReadOnlySpan<byte> clientPacket, bool appendNewline = false)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.ArrowAdd);
        writer.WriteGShort(playerId);
        WriteAfterClientOpcode(writer, clientPacket);
        AppendNewline(writer, appendNewline);
        return writer.ToArray();
    }

    public static byte[] HurtPlayer(
        ushort attackerId,
        byte hurtDx,
        byte hurtDy,
        byte power,
        uint npcId,
        bool appendNewline = false)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.HurtPlayer);
        writer.WriteGShort(attackerId);
        writer.WriteGChar(hurtDx);
        writer.WriteGChar(hurtDy);
        writer.WriteGChar(power);
        writer.WriteGInt(npcId);
        AppendNewline(writer, appendNewline);
        return writer.ToArray();
    }

    public static byte[] Explosion(
        ushort playerId,
        byte radius,
        byte encodedX,
        byte encodedY,
        byte power,
        bool appendNewline = false)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.Explosion);
        writer.WriteGShort(playerId);
        writer.WriteGChar(radius);
        writer.WriteGChar(encodedX);
        writer.WriteGChar(encodedY);
        writer.WriteGChar(power);
        AppendNewline(writer, appendNewline);
        return writer.ToArray();
    }

    public static byte[] HitObjectsFromPlayer(
        ushort playerId,
        byte encodedPower,
        byte encodedX,
        byte encodedY,
        bool appendNewline = false)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.HitObjects);
        writer.WriteGShort(playerId);
        writer.WriteGChar(encodedPower);
        writer.WriteGChar(encodedX);
        writer.WriteGChar(encodedY);
        AppendNewline(writer, appendNewline);
        return writer.ToArray();
    }

    public static byte[] HitObjectsFromNpc(
        uint npcId,
        byte encodedPower,
        byte encodedX,
        byte encodedY,
        bool appendNewline = false)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.HitObjects);
        writer.WriteGShort(0);
        writer.WriteGChar(encodedPower);
        writer.WriteGChar(encodedX);
        writer.WriteGChar(encodedY);
        writer.WriteGInt(npcId);
        AppendNewline(writer, appendNewline);
        return writer.ToArray();
    }

    private static void WriteAfterClientOpcode(GraalBinaryWriter writer, ReadOnlySpan<byte> clientPacket)
    {
        if (clientPacket.Length > 1)
            writer.WriteBytes(clientPacket[1..]);
    }

    private static void AppendNewline(GraalBinaryWriter writer, bool appendNewline)
    {
        if (appendNewline)
            writer.WriteByte((byte)'\n');
    }
}
