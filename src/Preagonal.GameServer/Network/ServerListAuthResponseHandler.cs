using Preagonal.Common.Models.Connections.Packets.ListServerToGameServer;
using Preagonal.GameServer.Network.Protocol;

namespace Preagonal.GameServer.Network;

public enum ServerListAuthResponseStatus
{
    SessionNotFound,
    AcceptedPreWorld,
    Rejected,
}

public sealed record ServerListAuthResponseResult(
    ServerListAuthResponseStatus Status,
    VerifyAccountV2Packet Response)
{
    public bool SessionFound => Status != ServerListAuthResponseStatus.SessionNotFound;
}

public sealed class ServerListAuthResponseHandler(
    Func<ushort, PlayerSessionType, ClientSessionSkeleton?> findSession)
{
    public ServerListAuthResponseResult HandleVerifyAccount2(VerifyAccountV2Packet payloadWithoutPacketId)
    {
	    var session  = findSession(payloadWithoutPacketId.Id, (PlayerSessionType)payloadWithoutPacketId.Type);

        if (session is null)
            return new(
                ServerListAuthResponseStatus.SessionNotFound,
                payloadWithoutPacketId);

        var accepted = session.ReceiveServerListAuthResponse(payloadWithoutPacketId);
        return new(
            accepted
                ? ServerListAuthResponseStatus.AcceptedPreWorld
                : ServerListAuthResponseStatus.Rejected,
            payloadWithoutPacketId);
    }
}