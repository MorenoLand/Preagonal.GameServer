using Preagonal.GameServer.Services;

namespace Preagonal.GameServer.Network;

public sealed class ServerListAuthBoundary(
    IGameServerService gameServerService,
    PreWorldAuthOptions options)
{
    public PreWorldAuthResult Begin(ClientSessionSkeleton session)
    {
        var effectiveOptions = options with { IsServerListConnected = gameServerService.ListServerConnected };
        var result = new PreWorldAuthBoundary(effectiveOptions).Begin(session);
        if (result.Accepted)
            gameServerService.SendLoginPacketForPlayer(result.ServerListRequest);

        return result;
    }
}