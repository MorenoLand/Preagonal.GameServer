using System.Text;

namespace GServ.Protocol;

public static class EntityPackets
{
    public static byte[] ItemAdd(byte encodedX, byte encodedY, byte itemType)
    {
        var writer = NewPacket(ServerToPlayerPacketId.ItemAdd);
        writer.WriteGChar(encodedX);
        writer.WriteGChar(encodedY);
        writer.WriteGChar(itemType);
        return WithNewline(writer);
    }

    public static byte[] ItemDelete(byte encodedX, byte encodedY)
    {
        var writer = NewPacket(ServerToPlayerPacketId.ItemDelete);
        writer.WriteGChar(encodedX);
        writer.WriteGChar(encodedY);
        return WithNewline(writer);
    }

    public static byte[] ItemDeleteFromLevelCoordinates(float x, float y) =>
        ItemDelete((byte)(x * 2), (byte)(y * 2));

    public static byte[] HorseAdd(float x, float y, byte direction, byte bushes, string image)
    {
        var writer = NewPacket(ServerToPlayerPacketId.HorseAdd);
        writer.WriteByte((byte)(x * 2));
        writer.WriteGChar((byte)(y * 2));
        writer.WriteGChar((byte)((bushes << 2) | (direction & 0x03)));
        writer.WriteBytes(Encoding.ASCII.GetBytes(image));
        return WithNewline(writer);
    }

    public static byte[] HorseDelete(float x, float y)
    {
        var writer = NewPacket(ServerToPlayerPacketId.HorseDelete);
        writer.WriteGChar((byte)(x * 2));
        writer.WriteGChar((byte)(y * 2));
        return WithNewline(writer);
    }

    public static byte[] DefaultWeapon(byte itemType)
    {
        var writer = NewPacket(ServerToPlayerPacketId.DefaultWeapon);
        writer.WriteGChar(itemType);
        return WithNewline(writer);
    }

    public static byte[] NpcWeaponAdd(string name, string image, string formattedClientGs1)
    {
        var writer = NewPacket(ServerToPlayerPacketId.NpcWeaponAdd);
        WriteGCharString(writer, name);
        writer.WriteGChar(0);
        WriteGCharString(writer, image);
        writer.WriteGChar(1);
        writer.WriteGShort((ushort)Encoding.Latin1.GetByteCount(formattedClientGs1));
        writer.WriteBytes(Encoding.Latin1.GetBytes(formattedClientGs1));
        return WithNewline(writer);
    }

    public static byte[] NpcWeaponDelete(string name)
    {
        var writer = NewPacket(ServerToPlayerPacketId.NpcWeaponDelete);
        writer.WriteBytes(Encoding.ASCII.GetBytes(name));
        return WithNewline(writer);
    }

    public static byte[] NpcWeaponScriptRawData(ReadOnlySpan<byte> bytecode)
    {
        var writer = NewPacket(ServerToPlayerPacketId.RawData);
        writer.WriteGInt((uint)bytecode.Length);
        writer.WriteByte((byte)'\n');
        writer.WriteGChar((byte)ServerToPlayerPacketId.NpcWeaponScript);
        writer.WriteBytes(bytecode);
        return writer.ToArray();
    }

    public static byte[] NpcDelete(uint npcId)
    {
        var writer = NewPacket(ServerToPlayerPacketId.NpcDelete);
        writer.WriteGInt(npcId);
        return WithNewline(writer);
    }

    public static byte[] NpcProps(uint npcId, ReadOnlySpan<byte> props)
    {
        var writer = NewPacket(ServerToPlayerPacketId.NpcProps);
        writer.WriteGInt(npcId);
        writer.WriteBytes(props);
        return WithNewline(writer);
    }

    public static byte[] NpcDelete2(string levelName, uint npcId)
    {
        var writer = NewPacket(ServerToPlayerPacketId.NpcDelete2);
        writer.WriteBytes(Encoding.ASCII.GetBytes(levelName));
        writer.WriteGInt(npcId);
        return WithNewline(writer);
    }

    private static GraalBinaryWriter NewPacket(ServerToPlayerPacketId packetId)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)packetId);
        return writer;
    }

    private static void WriteGCharString(GraalBinaryWriter writer, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        writer.WriteGChar((byte)bytes.Length);
        writer.WriteBytes(bytes);
    }

    private static byte[] WithNewline(GraalBinaryWriter writer)
    {
        writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }
}
