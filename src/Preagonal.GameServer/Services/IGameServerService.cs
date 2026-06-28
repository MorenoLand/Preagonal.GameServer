using Preagonal.Common.Core;
using Preagonal.GameServer.Network;

namespace Preagonal.GameServer.Services;

public interface IGameServerService : IHostedService
{
	bool ListServerConnected { get; }
	void SendPlayerAdd(PostLoginPlayerSnapshot snapshot, GByteBuffer properties);
	void SendPlayerRemove(ushort playerId);
	void SendLoginPacketForPlayer(ClientSessionSkeleton? sessionSkeleton);
	void SendServerInfoForPlayer(ushort playerId, string serverName);
	Task<bool> SendAsync(ushort resultPlayerId, byte[] resultOutboundBytes);
}