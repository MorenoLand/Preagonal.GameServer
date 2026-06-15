namespace GServ.Protocol.Packets;

/// <summary>
/// Packet direction in the original protocol. Numeric IDs remain blocked on recovered C++ IEnums.h.
/// </summary>
public enum PacketDirection
{
    ClientToServer,
    ServerToClient,
    ServerList
}

public readonly record struct PacketId(byte Value)
{
    public override string ToString() => Value.ToString();
}

public interface IPacket
{
    PacketDirection Direction { get; }
    PacketId Id { get; }
    ReadOnlyMemory<byte> Payload { get; }
}

public sealed record RawPacket(PacketDirection Direction, PacketId Id, ReadOnlyMemory<byte> Payload) : IPacket;
