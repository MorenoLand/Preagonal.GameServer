using Preagonal.GameServer.Network.Protocol;

namespace Preagonal.GameServer.Network;

public sealed record PreWorldAuthOptions(
    int MaxPlayers,
    int CurrentPlayerCount,
    bool IsIpBanned,
    bool IsServerListConnected,
    IReadOnlyList<string> AllowedVersions,
    string AllowedVersionText);

public sealed record PreWorldAuthResult(bool Accepted, ClientSessionSkeleton? ServerListRequest);

public sealed class PreWorldAuthBoundary(PreWorldAuthOptions options)
{
	public PreWorldAuthResult Begin(ClientSessionSkeleton session)
    {
        if (session.LoginPacket is null)
            throw new InvalidOperationException("Login packet must be parsed before pre-world authentication.");

        if (options.CurrentPlayerCount >= options.MaxPlayers)
            return Reject(session, "This server has reached its player limit.");

        if (options.IsIpBanned)
            return Reject(session, "You have been banned from this server.");

        if (IsClient(session.Type) &&
            !AllowedVersionPolicy.IsAllowed(session.LoginPacket.VersionId, options.AllowedVersions))
        {
            return Reject(session, $"Your client version is not allowed on this server.\rAllowed: {options.AllowedVersionText}");
        }

        if (!options.IsServerListConnected)
            return Reject(session, "The login server is offline.  Try again later.");

        session.MarkWaitingForServerListAuth();

        return new(true, session);
    }

    private static PreWorldAuthResult Reject(ClientSessionSkeleton session, string message)
    {
        session.QueueDisconnect(message);
        return new(false, null);
    }

    private static bool IsClient(PlayerSessionType type) =>
        (type & PlayerSessionType.AnyClient) != 0;
}