using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Preagonal.Common.Core;
using Preagonal.Common.Extensions;
using Preagonal.Common.Models.Connections.Packets.GameServerToListServer;
using Preagonal.Common.Models.Servers.Enums;
using Preagonal.Common.PacketHandling;
using Preagonal.Common.Registries;
using Preagonal.Common.Serializers;
using Preagonal.GameServer.Configuration;
using Preagonal.GameServer.Network;
using Preagonal.GameServer.Network.Protocol;
using Preagonal.GameServer.Persistence;
using Preagonal.GameServer.Services;
using LS2GS = Preagonal.Common.Models.Connections.Enums.Packets.ListServerToGameServer;
using LS2GSP = Preagonal.Common.Models.Connections.Packets.ListServerToGameServer;

namespace Preagonal.GameServer.Connections.ListServer;

[GeneratePacketHandlerInterface(typeof(LS2GS), nameof(IHandleListServerMessages))]
public class ListServerConnection(ILogger<ListServerConnection> logger, IOptions<ServerOptions> serverOptions, IOptions<AdminConfig> adminConfig) :
	AsyncSocket,
	IListServerConnection,
	IHandleListServerMessages
{
	private ServerOptions       _serverListOptions = serverOptions.Value;
	private AdminConfig          _adminConfig        = adminConfig.Value;
	private bool                _newServerProtocol = false;
	private DateTime            _lastData          = DateTime.UtcNow;
	private bool                _stopParsingData   = false;
	private LoginAuthBridge?    _authBridge;
	private IGameServerService? _gameServerService;

	public async Task SendLogin()
	{
		var localIp = ResolveLocalIp(_serverListOptions.LocalIp, GetIp().ToString());

		Codec.Reset(Encrypt.Generation.GEN1, 0);
		SendPacket(new RegisterV3Packet(Program.BuildVersion!), sendNow: true);
		Codec.Reset(Encrypt.Generation.GEN2, key: 0);
		_newServerProtocol = true;
		SendPacket(new ServerPasswordPacket(_adminConfig.HQPassword));
		SendPacket(
			new NewServerPacket(
				_serverListOptions.Name,
				_serverListOptions.Description,
				_serverListOptions.Language,
				Program.BuildVersion!,
				_serverListOptions.Url,
				_serverListOptions.ServerIp,
				_serverListOptions.ServerPort.ToString(),
				localIp
			)
		);
		SendPacket(new SetServerLevelPacket(_serverListOptions.OnlyStaff?ServerLevel.Hidden:(ServerLevel)_adminConfig.HQLevel), sendNow: true);
		SendPacket(AllowedVersionsText([""]/*TODO:FIX*/), sendNow: true);
		SendPacket(new ResetPlayersPacket(), sendNow: true);
		SendCompress();
		StartLoops();

		await Task.CompletedTask.ConfigureAwait(false);
	}

	private static SendTextPacket AllowedVersionsText(IReadOnlyList<string> allowedVersions) => new($"Listserver,settings,allowedversions,{allowedVersions.Tokenize()}");


	public void SetAuthBridge(LoginAuthBridge authBridge)                  => _authBridge = authBridge;
	public void SetGameServerService(IGameServerService gameServerService) => _gameServerService = gameServerService;

	private static string ResolveLocalIp(string configuredLocalIp, string socketLocalIp)
	{
		var localIp = string.IsNullOrEmpty(configuredLocalIp) || configuredLocalIp == "AUTO"
			? socketLocalIp
			: configuredLocalIp;

		return localIp is "127.0.1.1" or "127.0.0.1" ? string.Empty : localIp;
	}

	public Task Handle(LS2GSP.AssignPCIdPacket packet)
	{
		logger.LogInformation(
			"Received listserver PC id assignment: id={Id}; type={Type}; pcId={PCId}",
			packet.Id,
			packet.Type,
			packet.PCId
		);
		return Task.CompletedTask;
	}

	public async Task Handle(LS2GSP.VerifyAccountV2Packet packet)
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

	public Task Handle(LS2GSP.SendTextPacket packet)
	{
		logger.LogInformation("Received sendtext from listserver: {Text}", packet.Data.Tokenize());
		return Task.CompletedTask;
	}

	public Task Handle(LS2GSP.RequestTextPacket packet)
	{
		logger.LogInformation("Received requesttext from listserver: {Text}", packet.Data.Tokenize());
		return Task.CompletedTask;
	}

	public Task Handle(LS2GSP.PingPacket packet)
	{
		logger.LogInformation("Received ping from listserver");
		SendPacket(ServerListAuthPackets.Ping());
		return Task.CompletedTask;
	}

	public Task Handle(LS2GSP.ErrorMessagePacket packet)
	{
		logger.LogError("Received error from listserver: {Text}", packet.Message);
		return Task.CompletedTask;
	}

	public async Task Handle(LS2GSP.ServerInfoPacket packet)
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

	public Task Handle(LS2GSP.ProfilePacket packet)
	{
		logger.LogWarning("Received unimplemented listserver profile packet for account {Account}", packet.Account);
		return Task.CompletedTask;
	}

#pragma warning disable CS0618
	public Task Handle(LS2GSP.VerifyAccountV1Packet packet)
	{
		logger.LogWarning("Received deprecated listserver VerifyAccountV1 packet for account {Account}", packet.Account);
		return Task.CompletedTask;
	}
#pragma warning restore CS0618

	public Task Handle(LS2GSP.VerifyGuildPacket packet)
	{
		logger.LogWarning("Received unimplemented listserver VerifyGuild packet for player {Id}", packet.Id);
		return Task.CompletedTask;
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

			if (PacketHandlers.TryGetValue(packetId, out var packetClass))
				await DispatchPacket(packetId, packetClass, curPacket).ConfigureAwait(false);
			else
				logger.LogWarning("Received unregistered packet: {PacketId}", packetId);

			_lastData = DateTime.UtcNow;
			prevPacket = packetId;
		}
	}

	private async Task DispatchPacket(LS2GS packetId, Type packetClass, GByteBuffer packet)
	{
		var handlerInterface = typeof(IHandleMessage<>).MakeGenericType(packetClass);
		if (!handlerInterface.IsInstanceOfType(this))
		{
			logger.LogWarning("Received unimplemented packet: {PacketId}", packetId);
			return;
		}

		var handler = handlerInterface.GetMethod(nameof(IHandleMessage<>.Handle));
		if (handler?.Invoke(this, [packet.Deserialize(packetClass)]) is Task task)
			await task.ConfigureAwait(false);
	}

	#endregion
}