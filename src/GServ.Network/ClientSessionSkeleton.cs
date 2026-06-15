using GServ.Protocol;

namespace GServ.Network;

public sealed class ClientSessionSkeleton
{
    private readonly MemoryStream _outbound = new();

    public ClientSessionSkeleton(ushort id)
    {
        Id = id;
    }

    public ushort Id { get; }
    public PlayerSessionType Type { get; private set; } = PlayerSessionType.Await;
    public EncryptionGeneration InboundEncryptionGeneration { get; private set; } = EncryptionGeneration.Gen3;
    public SessionLifecycle Lifecycle { get; private set; } = SessionLifecycle.AwaitingLoginPrelude;
    public LoginPacket? LoginPacket { get; private set; }

    public void ReceiveLoginPrelude(ReadOnlySpan<byte> payload)
    {
        var prelude = LoginPreludeParser.Parse(payload);
        Type = prelude.Type;
        InboundEncryptionGeneration = prelude.InboundGeneration;
        Lifecycle = SessionLifecycle.LoginPreludeParsed;
    }

    public bool ReceiveLoginPacket(ReadOnlySpan<byte> payload)
    {
        var login = LoginPacketParser.Parse(payload);
        Type = login.Type;
        InboundEncryptionGeneration = login.InboundGeneration;
        LoginPacket = login;

        if (!LoginPacketParser.IsKnownSessionType(login.Type))
        {
            var message = $"Your client type is unknown.  Please inform the OpenGraal Team.  Type: {(int)login.Type}.";
            _outbound.Write(OutboundLoginPackets.DisconnectMessage(message, appendNewline: true));
            Lifecycle = SessionLifecycle.Rejected;
            return false;
        }

        Lifecycle = SessionLifecycle.LoginPreludeParsed;
        return true;
    }

    public byte[] TakeOutboundBytes()
    {
        var bytes = _outbound.ToArray();
        _outbound.SetLength(0);
        return bytes;
    }
}
