using Preagonal.GServer.Protocol;

namespace Preagonal.GServer.Network;

public sealed record ClientLoginAuthResult(
    bool Accepted,
    SessionLifecycle Lifecycle,
    byte[] OutboundBytes);

public sealed record ServerListLoginResponseResult(
    ServerListAuthResponseStatus Status,
    ushort PlayerId,
    PlayerSessionType Type,
    byte[] OutboundBytes);

public sealed class LoginAuthBridge(
    IServerListGateway serverList,
    PreWorldAuthOptions options,
    LoginWorldEntryOptions? worldEntryOptions = null)
{
    private readonly Dictionary<(ushort PlayerId, PlayerSessionType Type), ClientSessionSkeleton> _pendingSessions = [];
    private readonly Dictionary<(ushort PlayerId, PlayerSessionType Type), string> _remoteAddresses = [];

    public ClientLoginAuthResult BeginClientLogin(
        ClientSocketSessionContext context,
        ReadOnlySpan<byte> loginFrame)
    {
        var session = new ClientSessionSkeleton(context.PlayerId);
        if (!session.ReceiveLoginPacket(loginFrame))
            return Finish(session, accepted: false);

        var auth = new ServerListAuthBoundary(serverList, options);
        var result = auth.Begin(session);
        if (!result.Accepted)
            return Finish(session, accepted: false);

        var key = (session.Id, session.Type);
        _pendingSessions[key] = session;
        _remoteAddresses[key] = context.RemoteAddress;
        return Finish(session, accepted: true);
    }

    public ServerListLoginResponseResult HandleVerifyAccount2(ReadOnlySpan<byte> payloadWithoutPacketId)
    {
        var handler = new ServerListAuthResponseHandler(FindSession);
        var result = handler.HandleVerifyAccount2(payloadWithoutPacketId);
        var response = result.Response;
        var key = (response.PlayerId, response.Type);
        var session = FindSession(response.PlayerId, response.Type);
        if (result.Status == ServerListAuthResponseStatus.AcceptedPreWorld &&
            session is not null &&
            worldEntryOptions is not null &&
            LoginWorldEntry.Complete(session, worldEntryOptions with
            {
                AccountLoginOptions = worldEntryOptions.AccountLoginOptions with
                {
                    RemoteIp = _remoteAddresses.GetValueOrDefault(key, worldEntryOptions.AccountLoginOptions.RemoteIp)
                }
            }, out var playerAdd))
        {
            serverList.SendPlayerAdd(playerAdd);
        }

        var outbound = session is null ? [] : FlushOutboundBytes(session);

        if (result.Status != ServerListAuthResponseStatus.AcceptedPreWorld)
        {
            _pendingSessions.Remove(key);
            _remoteAddresses.Remove(key);
        }

        return new ServerListLoginResponseResult(
            result.Status,
            response.PlayerId,
            response.Type,
            outbound);
    }

    private ClientSessionSkeleton? FindSession(ushort id, PlayerSessionType type) =>
        _pendingSessions.TryGetValue((id, type), out var session) ? session : null;

    private static ClientLoginAuthResult Finish(ClientSessionSkeleton session, bool accepted) =>
        new(accepted, session.Lifecycle, FlushOutboundBytes(session));

    private static byte[] FlushOutboundBytes(ClientSessionSkeleton session)
    {
        var raw = session.TakeOutboundBytes();
        if (raw.Length == 0)
            return [];

        var queue = new GraalFileQueue();
        if (session.LoginPacket?.Type is PlayerSessionType.Client3 or PlayerSessionType.RemoteControl2 &&
            session.LoginPacket.EncryptionKey is { } key)
        {
            queue.SetCodec(EncryptionGeneration.Gen5, key);
        }
        else if (session.LoginPacket?.Type == PlayerSessionType.Web)
        {
            queue.SetCodec(EncryptionGeneration.Gen1, key: 0);
        }

        queue.AddPacket(raw);
        return queue.FlushSocket(forceSendFiles: true);
    }
}
