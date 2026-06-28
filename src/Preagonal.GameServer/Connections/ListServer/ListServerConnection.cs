using System.Net.Sockets;
using Newtonsoft.Json;
using Preagonal.Common.Core;
using Preagonal.Common.Extensions;
using Preagonal.Common.Models.Connections.Packets.GameServerToListServer;
using Preagonal.Common.Models.Servers.Enums;
using Preagonal.Common.Registries;
using Preagonal.Common.Serializers;
using Preagonal.GameServer.Network;
using Preagonal.GameServer.Network.Protocol;
using Preagonal.GameServer.Services;
using LS2GS = Preagonal.Common.Models.Connections.Enums.Packets.ListServerToGameServer;
using LS2GSP = Preagonal.Common.Models.Connections.Packets.ListServerToGameServer;

namespace Preagonal.GameServer.Connections.ListServer;

public class ListServerConnection(ILogger<ListServerConnection> logger) : AsyncSocket, IListServerConnection
{
	private ServerListConnectOptions? _serverListOptions;
	private bool                      _newServerProtocol = false;
	private DateTime                  _lastData;
	private bool                      _stopParsingData = false;
	private LoginAuthBridge?          _authBridge;
	private IGameServerService?       _gameServerService;

	public async Task SendLogin()
	{
		if (_serverListOptions == null)
			return;

		var localIp = ResolveLocalIp(_serverListOptions.LocalIp, GetIp().ToString());

		Codec.Reset(Encrypt.Generation.GEN1, 0);
		SendPacket(new RegisterV3Packet(_serverListOptions.Version), sendNow: true);
		Codec.Reset(Encrypt.Generation.GEN2, key: 0);
		_newServerProtocol = true;
		SendPacket(new ServerPasswordPacket(_serverListOptions.HqPassword));
		SendPacket(
			new NewServerPacket(
				_serverListOptions.Name,
				_serverListOptions.Description,
				_serverListOptions.Language,
				_serverListOptions.Version,
				_serverListOptions.Url,
				_serverListOptions.ServerIp,
				_serverListOptions.ServerPort,
				localIp
			)
		);
		SendPacket(new SetServerLevelPacket(_serverListOptions.OnlyStaff?ServerLevel.Hidden:(ServerLevel)_serverListOptions.HqLevel), sendNow: true);
		SendPacket(AllowedVersionsText(_serverListOptions.AllowedVersions), sendNow: true);
		SendPacket(new ResetPlayersPacket(), sendNow: true);

		StartLoops();

		await Task.CompletedTask.ConfigureAwait(false);
	}

	private static SendTextPacket AllowedVersionsText(IReadOnlyList<string> allowedVersions) => new($"Listserver,settings,allowedversions,{allowedVersions.Tokenize()}");


	public void SetOptions(ServerListConnectOptions serverListOptions)     => _serverListOptions = serverListOptions;
	public void SetAuthBridge(LoginAuthBridge authBridge)                  => _authBridge = authBridge;
	public void SetGameServerService(IGameServerService gameServerService) => _gameServerService = gameServerService;

	private static string ResolveLocalIp(string configuredLocalIp, string socketLocalIp)
	{
		var localIp = string.IsNullOrEmpty(configuredLocalIp) || configuredLocalIp == "AUTO"
			? socketLocalIp
			: configuredLocalIp;

		return localIp is "127.0.1.1" or "127.0.0.1" ? string.Empty : localIp;
	}

	public async Task VerifyAccountV2PacketHandler(LS2GSP.VerifyAccountV2Packet packet)
	{
		if (_authBridge == null) return;

		var result = _authBridge.HandleVerifyAccount2(packet);
		logger.LogInformation(
			"VerifyAccount2 result: status={ResultStatus}; player={ResultPlayerId}; type={ResultType}; outbound={OutboundBytesLength}; broadcasts={BroadcastsCount}",
			result.Status,
			result.PlayerId,
			result.Type,
			result.OutboundBytes.Length,
			result.Broadcasts.Count
		);

		if (result.OutboundBytes.Length != 0)
			if (_gameServerService != null)
				await _gameServerService.SendAsync(result.PlayerId, result.OutboundBytes).ConfigureAwait(false);
		foreach (var broadcast in result.Broadcasts)
			if (broadcast.OutboundBytes.Length != 0)
			{
				var sent = _gameServerService != null && await _gameServerService.SendAsync(
						broadcast.PlayerId,
						broadcast.OutboundBytes
					)
					.ConfigureAwait(false);
				Console.WriteLine(
					sent
						? $"Sent login broadcast to client session {broadcast.PlayerId}: {broadcast.OutboundBytes.Length} bytes."
						: $"Missed login broadcast to client session {broadcast.PlayerId}: stream not registered."
				);
			}
	}

	public Task SendTextPacketHandler(LS2GSP.SendTextPacket packet)
	{
		logger.LogInformation("Received sendtext from listserver: {Text}", packet.Data.Tokenize());
		return Task.CompletedTask;
	}

	public Task RequestTextPacketHandler(LS2GSP.RequestTextPacket packet)
	{
		logger.LogInformation("Received requesttext from listserver: {Text}", packet.Data.Tokenize());
		return Task.CompletedTask;
	}

	public Task PingPacketHandler(LS2GSP.PingPacket packet)
	{
		logger.LogInformation("Received ping from listserver");
		SendPacket(ServerListAuthPackets.Ping());
		return Task.CompletedTask;
	}

	public Task PingPacketHandler(LS2GSP.ErrorMessagePacket packet)
	{
		logger.LogError("Received error from listserver: {Text}", packet.Message);
		return Task.CompletedTask;
	}

	public async Task ServerInfoPacketHandler(LS2GSP.ServerInfoPacket packet)
	{
		logger.LogInformation("Listserver server info: {SerializeObject}", JsonConvert.SerializeObject(packet.ServerData.Identifier.Detokenize()));
		var warp = _authBridge?.HandleServerInfo(packet);
		if (warp != null && warp.OutboundBytes.Length != 0)
		{
			if (_gameServerService != null)
				await _gameServerService.SendAsync(warp.PlayerId, warp.OutboundBytes).ConfigureAwait(false);
		}
		else if (!string.IsNullOrEmpty(warp?.Diagnostic))
			Console.WriteLine($"Listserver server info ignored: {warp.Diagnostic}");
	}

	#region Packet handling

	private static readonly IReadOnlyDictionary<LS2GS, Type> PacketHandlers = PacketTypeRegistry.BuildForEnum<LS2GS>(typeof(PacketSerializer).Assembly);

	protected override void SendCompress()
	{
		if (_mDataOut.Length < 1) return;

		try
		{
			if (!_newServerProtocol || _newClientProtocol)
			{
				var payload = _mDataOut.Buffer; // GByteBuffer exposes an owned array
				_mDataOut.Clear();

				QueueRaw(payload);
			}
			else
			{
				base.SendCompress();
			}
		}
		catch (ObjectDisposedException) { }
		catch (SocketException) { Disconnect(); }
	}

	protected override async Task ParseData()
	{
		while (_mDataIn.BytesLeft > 0)
		{
			_lastData = DateTime.UtcNow;

			if (_stopParsingData) return;

			if (_newServerProtocol)
			{
				_mDataIn.ReadCount = 0;
				// Parse Data
				while (_mDataIn.BytesLeft >= 2)
				{
					// packet length
					var len = _mDataIn.ReadShort();
					if (len > _mDataIn.Length - 2) break;

					var packet = _mDataIn.Read(len);
					_mDataIn.Remove(0, len + 2);

					switch (MCodec.Gen)
					{
						// Gen 1 is not encrypted or compressed.
						case Encrypt.Generation.GEN1:
							break;

						// Gen 2 and 3 are zlib compressed.  Gen 3 encrypts individual packets
						// Uncompress so we can properly decrypt later on.
						case Encrypt.Generation.GEN2:
						case Encrypt.Generation.GEN3:
							packet.ZDecompress();
							break;

						// Gen 4 and up encrypt the whole combined and compressed packet.
						// Decrypt and decompress.
						default:
							DecryptPacket(packet);
							break;
					}

					await HandleData(packet).ConfigureAwait(false);
				}
			}
			else
			{
				do
				{
					int lineEnd;
					if ((lineEnd = _mDataIn.IndexOf('\n')) == -1) break;
					var line = _mDataIn.ReadChars2((uint)(lineEnd + 1));

					_mDataIn.Remove(0, line.Length);
					await HandleData(line).ConfigureAwait(false);
				} while (_mDataIn.BytesLeft > 0 && !_newServerProtocol);
			}
		}
	}

	protected override async Task HandleData(GByteBuffer packet)
	{
		LS2GS      prevPacket = 0;
		const uint readCount  = 0;

		while (packet.BytesLeft > 0)
		{
			var curPacket = prevPacket == LS2GS.RawData ? packet.ReadChars2(readCount) : packet.ReadString('\n');

			if (!curPacket.TryParse<LS2GS>(out var packetId))
			{
				packet.ReadCount = 0;
				logger.LogError(
					"Received unknown packet from {Ip} {PacketId}: {PacketData}",
					GetIp(),
					packetId,
					packet.ToString()
				);

				Disconnect();
				return;
			}

			if (PacketHandlers.TryGetValue(packetId, out var packetClass) && GetType().GetMethod($"{packetClass.Name}Handler"??"") is {} method)
			{
				method.Invoke(this, [curPacket.Deserialize(packetClass)]);
			}
			else
			{
				logger.LogWarning("Received unimplemented packet: {PacketId}", packetId);
			}

			_lastData = DateTime.UtcNow;
			prevPacket = packetId;
		}
	}

	#endregion

}