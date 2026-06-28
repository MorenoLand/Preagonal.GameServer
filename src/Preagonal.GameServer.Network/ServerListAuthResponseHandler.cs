using Preagonal.GameServer.Network.Protocol;

namespace Preagonal.GameServer.Network;

public enum ServerListAuthResponseStatus
{
    SessionNotFound,
    AcceptedPreWorld,
    Rejected
}

public sealed record ServerListAuthResponseResult(
    ServerListAuthResponseStatus Status,
    ServerListVerifyAccount2Response Response)
{
    public bool SessionFound => Status != ServerListAuthResponseStatus.SessionNotFound;
}

public sealed class ServerListAuthResponseHandler(
    Func<ushort, PlayerSessionType, ClientSessionSkeleton?> findSession)
{
    public ServerListAuthResponseResult HandleVerifyAccount2(ReadOnlySpan<byte> payloadWithoutPacketId)
    {
        var response = ServerListAuthPackets.ParseVerifyAccount2Response(payloadWithoutPacketId);
        var session = findSession(response.PlayerId, response.Type);

        if (session is null)
            return new(
                ServerListAuthResponseStatus.SessionNotFound,
                response);

        var accepted = session.ReceiveServerListAuthResponse(response);
        return new(
            accepted
                ? ServerListAuthResponseStatus.AcceptedPreWorld
                : ServerListAuthResponseStatus.Rejected,
            response);
    }
}
