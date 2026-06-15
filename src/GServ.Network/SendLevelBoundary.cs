using GServ.Protocol;

namespace GServ.Network;

public sealed record LevelLayerPayload(int LayerIndex, byte[] Packet);

public sealed record ModernLevelPayload(
    string LevelName,
    long LevelModTime,
    byte[] BoardPacket,
    IReadOnlyList<LevelLayerPayload> Layers,
    byte[] LinksPacket,
    byte[] SignsPacket);

public sealed record SendLevelRequest(
    long RequestedModTime,
    long CachedLevelModTime,
    bool FromAdjacent);

public enum SendLevelStopPoint
{
    BeforeDynamicLevelRuntime
}

public sealed record SendLevelBoundaryResult(
    bool Accepted,
    SendLevelStopPoint StopPoint);

public static class SendLevelBoundary
{
    private const int BoardRawDataLength = 1 + (64 * 64 * 2) + 1;

    public static SendLevelBoundaryResult BeginModern(
        ClientSessionSkeleton session,
        ModernLevelPayload level,
        SendLevelRequest request)
    {
        if (session.Lifecycle != SessionLifecycle.ReadyForLevelRuntime)
            throw new InvalidOperationException("sendLevel boundary requires ReadyForLevelRuntime.");

        QueuePacket(session, WarpPackets.BuildLevelName(level.LevelName));

        var cachedLevelModTime = request.CachedLevelModTime;
        var requestedModTime = request.RequestedModTime == -1
            ? level.LevelModTime
            : request.RequestedModTime;

        if (cachedLevelModTime == 0)
        {
            if (requestedModTime != level.LevelModTime)
            {
                QueuePacket(session, RawDataHeader(BoardRawDataLength));
                QueuePacket(session, level.BoardPacket);

                foreach (var layer in level.Layers)
                {
                    if (layer.LayerIndex == 0)
                        continue;

                    QueuePacket(session, RawDataHeader(layer.Packet.Length));
                    QueuePacket(session, layer.Packet);
                }
            }

            QueuePacket(session, LevelModTime(level.LevelModTime));
            QueuePacket(session, level.LinksPacket);
            QueuePacket(session, level.SignsPacket);
        }

        session.MarkLevelPayloadSent();
        return new SendLevelBoundaryResult(true, SendLevelStopPoint.BeforeDynamicLevelRuntime);
    }

    private static byte[] RawDataHeader(int length)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.RawData);
        writer.WriteGInt(unchecked((uint)length));
        return writer.ToArray();
    }

    private static byte[] LevelModTime(long modTime)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.LevelModTime);
        writer.WriteGInt5(unchecked((uint)modTime));
        return writer.ToArray();
    }

    private static void QueuePacket(ClientSessionSkeleton session, byte[] packet)
    {
        if (packet.Length == 0)
            return;

        session.QueuePacket(AppendNewline(packet));
    }

    private static byte[] AppendNewline(byte[] packet)
    {
        if (packet.Length > 0 && packet[^1] == (byte)'\n')
            return packet;

        var output = new byte[packet.Length + 1];
        packet.CopyTo(output, 0);
        output[^1] = (byte)'\n';
        return output;
    }
}
