using Preagonal.GameServer.Network.Protocol;

namespace Preagonal.GameServer.Network;

public sealed record ServerListConnectOptions(
    string ListIp,
    string ListPort,
    string Name,
    string Description,
    string Language,
    string Version,
    string Url,
    string ServerIp,
    string ServerPort,
    string LocalIp,
    string HqPassword,
    int HqLevel,
    bool OnlyStaff,
    IReadOnlyList<string> AllowedVersions);

public sealed record ServerListConnectResult(bool Connected);

public interface IServerListSocket
{
    bool IsConnected { get; }
    string LocalIp { get; }
    bool Initialize(string host, string port);
    bool Connect();
    void Register();
    void ClearOutgoingBuffers();
    void SetCodec(EncryptionGeneration generation, byte key);
    void SendPacket(byte[] packetBody, bool sendNow = false);
}

public sealed class ServerListLifecycle(IServerListSocket socket)
{
	public ServerListConnectResult ConnectServer(ServerListConnectOptions options)
    {
        if (socket.IsConnected)
            return new(true);

        if (!socket.Initialize(options.ListIp, options.ListPort))
            return new(false);

        if (!socket.Connect())
            return new(false);

        socket.Register();

        var localIp = ResolveLocalIp(options.LocalIp, socket.LocalIp);

        socket.ClearOutgoingBuffers();
        socket.SetCodec(EncryptionGeneration.Gen1, key: 0);
        socket.SendPacket(ServerListAuthPackets.RegisterV3(options.Version), sendNow: true);
        socket.SetCodec(EncryptionGeneration.Gen2, key: 0);
        socket.SendPacket(ServerListAuthPackets.ServerHqPass(options.HqPassword));
        socket.SendPacket(ServerListAuthPackets.NewServer(
            options.Name,
            options.Description,
            options.Language,
            options.Version,
            options.Url,
            options.ServerIp,
            options.ServerPort,
            localIp));
        socket.SendPacket(ServerListAuthPackets.ServerHqLevel(options.OnlyStaff, options.HqLevel), sendNow: true);
        socket.SendPacket(ServerListAuthPackets.AllowedVersionsText(options.AllowedVersions), sendNow: true);
        socket.SendPacket(ServerListAuthPackets.SetPlayers(), sendNow: true);

        return new(socket.IsConnected);
    }

    private static string ResolveLocalIp(string configuredLocalIp, string socketLocalIp)
    {
        var localIp = string.IsNullOrEmpty(configuredLocalIp) || configuredLocalIp == "AUTO"
            ? socketLocalIp
            : configuredLocalIp;

        return localIp is "127.0.1.1" or "127.0.0.1" ? string.Empty : localIp;
    }
}
