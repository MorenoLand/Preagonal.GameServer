using GServ.Protocol;

namespace GServ.Game;

public static class NwLevelPacketBuilder
{
    public static byte[] BuildBoardPacket(NwLevelSnapshot level)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.BoardPacket);
        WriteRawTileLayer(writer, level, 0);
        writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }

    public static byte[] BuildLayerPacket(NwLevelSnapshot level, int layer)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.BoardLayer);
        writer.WriteByte((byte)layer);
        writer.WriteByte(0);
        writer.WriteByte(0);
        writer.WriteByte(64);
        writer.WriteByte(64);
        WriteRawTileLayer(writer, level, layer);
        writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }

    private static void WriteRawTileLayer(GraalBinaryWriter writer, NwLevelSnapshot level, int layer)
    {
        level.Layers.TryGetValue(layer, out var tiles);
        tiles ??= new ushort[64 * 64];

        foreach (var tile in tiles)
        {
            writer.WriteByte((byte)(tile & 0xFF));
            writer.WriteByte((byte)(tile >> 8));
        }
    }
}
