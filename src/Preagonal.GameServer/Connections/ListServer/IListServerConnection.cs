using System.Net;
using Preagonal.Common.Core;
using Preagonal.Common.Models.Connections.Packets;
using Preagonal.GameServer.Network;
using Preagonal.GameServer.Services;

namespace Preagonal.GameServer.Connections.ListServer;

public interface IListServerConnection
{
	bool Connected     { get; }
	Task ConnectAsync(IPAddress addr, int port, CancellationToken ct = default );
	Task ConnectAsync(string hostname, int port, CancellationToken ct = default );
	Task SendLogin();
	void SetOptions(ServerListConnectOptions serverListOptions);
	void SendPacket(GByteBuffer packet);

	void SendPacket(GenericPacket packet, bool sendNow = true);

	void SendPacket<T>(bool sendNow = true, params object[] parameters) where T : GenericPacket;
	void SetAuthBridge(LoginAuthBridge authBridge);
	void SetGameServerService(IGameServerService gameServerService);
}