using GServ.Protocol;

namespace GServ.Network;

public enum LevelMapType
{
    BigMap = 0,
    Gmap = 1
}

public sealed record LevelMapSnapshot(LevelMapType Type, string MapName);

public sealed record LevelEntrySnapshot(
    string LevelName,
    LevelMapSnapshot? Map = null,
    byte MapX = 0,
    byte MapY = 0);

public sealed record LevelWarpRequest(
    string LevelName,
    float X,
    float Y,
    float Z,
    ClientVersionId ClientVersion,
    long ModTime);

public interface ILevelLookup
{
    LevelEntrySnapshot? FindLevel(string levelName);
}

public enum LevelEntryStopPoint
{
    MissingLevel,
    BeforeSendLevelRuntime
}

public sealed record LevelEntryBoundaryResult(
    bool Accepted,
    LevelEntryStopPoint StopPoint,
    LevelEntrySnapshot? Level);

public static class WarpWorldEntryBoundary
{
    public static LevelEntryBoundaryResult BeginSetLevel(
        ClientSessionSkeleton session,
        ILevelLookup levelLookup,
        LevelWarpRequest request)
    {
        if (session.Lifecycle != SessionLifecycle.ReadyForLevelWarp)
            throw new InvalidOperationException("setLevel boundary requires ReadyForLevelWarp.");

        var level = levelLookup.FindLevel(request.LevelName);
        if (level is null)
        {
            session.QueuePacket(AppendNewline(WarpPackets.BuildWarpFailed(request.LevelName)));
            return new LevelEntryBoundaryResult(false, LevelEntryStopPoint.MissingLevel, null);
        }

        if (request.ModTime == 0 || request.ClientVersion < ClientVersionId.Client21)
        {
            var packet = level.Map is { Type: LevelMapType.Gmap } &&
                         request.ClientVersion >= ClientVersionId.Client21
                ? WarpPackets.BuildPlayerWarp2(request.X, request.Y, request.Z, level.MapX, level.MapY, level.Map.MapName)
                : WarpPackets.BuildPlayerWarp(request.X, request.Y, level.LevelName);
            session.QueuePacket(AppendNewline(packet));
        }

        session.MarkReadyForLevelRuntime();
        return new LevelEntryBoundaryResult(true, LevelEntryStopPoint.BeforeSendLevelRuntime, level);
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
