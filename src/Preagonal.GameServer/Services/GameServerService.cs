using System.Net;
using Microsoft.Extensions.Options;
using Preagonal.Common.Core;
using Preagonal.GameServer.Configuration;
using Preagonal.GameServer.Connections.ListServer;
using Preagonal.GameServer.Game;
using Preagonal.GameServer.Network;
using Preagonal.GameServer.Persistence;
using Preagonal.Scripting.GS2Engine.GS2.Script;
using GS2LSP = Preagonal.Common.Models.Connections.Packets.GameServerToListServer;

namespace Preagonal.GameServer.Services;

public class GameServerService(ILogger<GameServerService> logger, IOptions<ServerOptions> serverOptions, IOptions<AdminConfig> adminConfig, IScriptManager scriptManager, IListServerConnection listServerConnection, ICommandLineArguments args) : IGameServerService
{
	private CancellationTokenSource?    _cts;
	private Task?                       _maintenanceLoop;
	private TcpClientConnectionRegistry clientConnections = new();

	private async Task MaintenanceLoop(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			/*
			foreach (var clientConnection in clientConnections)
				clientConnection.Value.DoMaintenance();
			*/
			await Task.Delay(500, ct).ConfigureAwait(false);
		}
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_cts              = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		listServerConnection.SetGameServerService(this);

		//await StartListServerSocket().ConfigureAwait(false);
		//await StartPlayerSocket().ConfigureAwait(false);

		_maintenanceLoop = Task.Run(() => MaintenanceLoop(_cts.Token), _cts.Token);
		var config = LocalDebugCommandLine.Parse(args);
		if (!config.Enabled)
		{
			logger.LogInformation("Preagonal GameServer v{BuildVersion} ({BuildDateTime}) runtime initialized", Program.BuildVersion, Program.BuildDateTime);
			var productionArgs = ServerStartupCommandLine.Parse(args, Environment.GetEnvironmentVariable);
			if (productionArgs.ShowHelp)
			{
				logger.LogInformation(
					"Confirmed C++ options: -h, --help, -s/--server, -p/--port, --localip, --serverip, --interface, --staff, --name"
				);
				logger.LogInformation(
					"Local debug shell: --local-debug --dev-root <path> --dev-level <level.nw> [--port <port>]"
				);
				return;
			}

			var snapshot = ServerStartupLoader.Load(Path.GetDirectoryName(AppContext.BaseDirectory) ?? Environment.CurrentDirectory, productionArgs, serverOptions, adminConfig);

			if (snapshot.Resolution.Success)
			{
				logger.LogInformation(
					"Server startup resolved server \'{ResolutionServerName}\' from {ResolutionSource}",
					snapshot.Resolution.ServerName,
					snapshot.Resolution.Source
				);
				logger.LogInformation("Server path: {ResolutionServerPath}", snapshot.Resolution.ServerPath);
				logger.LogInformation("config/serveroptions.txt loaded: {ServerOptionsIsOpened}", serverOptions.Value.IsLoaded);
				logger.LogInformation("config/adminconfig.txt loaded: {AdminConfigIsOpened}", serverOptions.Value.IsLoaded);
			}
			else
			{
				logger.LogError("Error: {Diagnostic}",snapshot.Resolution.Diagnostic);
				return;
			}

			var runtimeServer = new RuntimeServer();
			var runtimeLevelCache = new RuntimeLevelCache();
			var runtime = new ServerHostRuntime(runtimeServer, serverOptions.Value.ServerSide);
			await listServerConnection.ConnectAsync(serverOptions.Value.ListIp, serverOptions.Value.ListPort, cancellationToken).ConfigureAwait(false);

			if (listServerConnection.Connected)
			{
				await listServerConnection.SendLogin().ConfigureAwait(false);
				logger.LogInformation(
					"Registered \'{Name}\' with list server {ListIp}:{ListPort}",
					serverOptions.Value.Name,
					serverOptions.Value.ListIp,
					serverOptions.Value.ListPort
				);
			}
			else
				logger.LogInformation(
					"Could not connect/register with list server {ListIp}:{ListPort}",
					serverOptions.Value.ListIp,
					serverOptions.Value.ListPort
				);

			var serverRoot          = snapshot.Resolution.ServerPath!;
			var resourceFileSystems = LoadResourceFileSystems(serverRoot, serverOptions.Value);
			var levelLoader         = new NwLevelFileLoader(resourceFileSystems.Get(ServerFileSystemKind.All));
			var staffAccounts       = serverOptions.Value.Staff;
			var authBridge = new LoginAuthBridge(
				this,
				scriptManager,
				new(
					serverOptions.Value.MaxPlayers,
					0,
					false,
					listServerConnection.Connected,
					[],//serverListOptions.AllowedVersions,
					string.Join(", ", [""]/*serverListOptions.AllowedVersions*/)
				),
				new(
					new DiskAccountFileSystem(serverRoot),
					serverOptions.Value,
					levelLoader,
					new FileLevelLookup(levelLoader),
					new(
						serverOptions.Value.OnlyStaff,
						serverOptions.Value.Name,
						[],
						staffAccounts,
						"",
						new RandomGuestIdentitySelector(Random.Shared)
					)
				),
				runtimeServer
			);
			listServerConnection.SetAuthBridge(authBridge);

			runtime.LevelTimedEventsHandler = () =>
			{
				foreach (var broadcast in authBridge.TickLevelTimedEvents())
					if (broadcast.OutboundBytes.Length != 0)
						clientConnections.SendAsync(broadcast.PlayerId, broadcast.OutboundBytes, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
			};
			using var clientServer = new ClientTcpServer(
				IPAddress.Any,
				serverOptions.Value.ServerPort,
				new LoginSocketFrameHandler(authBridge, clientConnections),
				clientConnections,
				session => logger.LogInformation(
					"Accepted client session {SessionPlayerId} from {SessionRemoteAddress}",
					session.PlayerId,
					session.RemoteAddress
				)
			);

			var nextServerListKeepalive = DateTimeOffset.UtcNow.AddMinutes(1);
			runtime.ServerListTimedEventsHandler = () =>
			{
				if (!listServerConnection.Connected || DateTimeOffset.UtcNow < nextServerListKeepalive)
					return;

				nextServerListKeepalive = DateTimeOffset.UtcNow.AddMinutes(1);
				var ip = serverOptions.Value.ServerIp;
				if (string.IsNullOrEmpty(ip))
					ip = "AUTO";

				listServerConnection.SendPacket(new GS2LSP.SetIPPacket(ip));
				logger.LogInformation("Sent listserver set-ip keepalive: {Ip}", ip);
			};

			runtime.CleanupHandler = () =>
			{
				runtimeServer.CleanupForShutdown(player => authBridge.EndClientSession(player.Id));
				runtimeLevelCache.Clear();
			};
			var hostLoop = new ServerHostLoop(runtime, ServerHostLoop.StaticTime, TimeSpan.Zero);

			using var productionCts = new CancellationTokenSource();
			Console.CancelKeyPress += (_, e) =>
			{
				e.Cancel = true;
				productionCts.Cancel();
			};

			clientServer.Start();
			var acceptTask = clientServer.RunAsync(
				productionCts.Token,
				result =>
				{
					var endResult = authBridge.EndClientSession(result.PlayerId);
					foreach (var broadcast in endResult.Broadcasts)
						if (broadcast.OutboundBytes.Length != 0)
							clientConnections.SendAsync(
								                 broadcast.PlayerId,
								                 broadcast.OutboundBytes,
								                 productionCts.Token
							                 )
							                 .ConfigureAwait(false)
							                 .GetAwaiter()
							                 .GetResult();

					var saveResult = endResult.SaveResult;
					if (saveResult is { WriteAttempted: true })
						logger.LogInformation(
							"Saved account for client session {ResultPlayerId}: writeSucceeded={SaveResultWriteSucceeded}; path={SaveResultPath}",
							result.PlayerId,
							saveResult.WriteSucceeded,
							saveResult.Path
						);

					if (string.IsNullOrEmpty(result.Diagnostic))
						logger.LogInformation("Client session {ResultPlayerId} stopped: {ResultStopReason}", result.PlayerId, result.StopReason);
					else
						logger.LogInformation(
							"Client session {ResultPlayerId} stopped: {ResultStopReason}; {ResultDiagnostic}",
							result.PlayerId,
							result.StopReason,
							result.Diagnostic
						);
				}
			);


			logger.LogInformation(
				"Server startup resolved. Listening for clients on port {ServerPort}. Press Ctrl+C to stop",
				serverOptions.Value.ServerPort
			);
			hostLoop.Run(TimeSpan.FromMilliseconds(5), productionCts.Token);
			await Task.WhenAll(acceptTask).ConfigureAwait(false);
			return;
		}

		Console.WriteLine("WARNING: running LOCAL DEBUG local shell.");
		Console.WriteLine(
			"This is not production-compatible auth, server-list, movement, NPC, script, or file-transfer behavior."
		);
		Console.WriteLine($"Root: {config.RootPath}");
		Console.WriteLine($"Level: {config.LevelName}");
		Console.WriteLine($"Port: {config.Port}");

		var fileSystems = ServerResourceFileSystems.LoadAllFolders(config.RootPath, string.Empty);
		var fileSystem  = fileSystems.Get(ServerFileSystemKind.All);
		var pipeline    = new LocalDebugSessionPipeline(new(true, config.LevelName), new(fileSystem));

		using var server = new LocalDebugTcpServer(IPAddress.Any, config.Port, pipeline);
		Console.CancelKeyPress += (_, e) =>
		{
			e.Cancel = true;
			_cts.Cancel();
		};

		server.Start();
		logger.LogInformation("Listening. Press Ctrl+C to stop");
		while (!_cts.IsCancellationRequested)
			try
			{
				var result = await server.AcceptOneAsync(_cts.Token).ConfigureAwait(false);
				foreach (var line in result.Log)
					logger.LogInformation(line);
				logger.LogInformation(
					"Connection stopped at {ResultStopPoint}; lifecycle={ResultLifecycle}; accepted={ResultAccepted}",
					result.StopPoint,
					result.Lifecycle,
					result.Accepted
				);
			}
			catch (OperationCanceledException) when (_cts.IsCancellationRequested)
			{
				break;
			}

		return;



		static ServerResourceFileSystems LoadResourceFileSystems(string serverRoot, ServerOptions options)
		{
			var foldersConfig = Path.Combine(serverRoot, "config", "foldersconfig.txt");
			if (!options.GetBool("nofoldersconfig", false) && File.Exists(foldersConfig))
				return ServerResourceFileSystems.LoadFolderConfig(serverRoot, File.ReadAllText(foldersConfig));

			return ServerResourceFileSystems.LoadAllFolders(serverRoot, options.GetString("sharefolder", "")!);
		}
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("{ServiceName} has stopped", nameof(GameServerService));

		if (_maintenanceLoop == null)
			return;

		if (_cts != null)
			await _cts.CancelAsync().ConfigureAwait(false);

		await Task.WhenAny(_maintenanceLoop, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
	}

	public bool ListServerConnected                             => listServerConnection.Connected;
	public void SendPlayerAdd(PostLoginPlayerSnapshot snapshot, GByteBuffer properties) =>
		listServerConnection.SendPacket(
			new GS2LSP.AddPlayerPacket(
				snapshot.PlayerId,
				(byte)snapshot.Type,
				properties
			)
		);

	public void SendPlayerRemove(ushort playerId) =>
		listServerConnection.SendPacket(
			new GS2LSP.DeletePlayerPacketV2(playerId)
		);

	public void SendLoginPacketForPlayer(ClientSessionSkeleton? sessionSkeleton)
	{
		if (sessionSkeleton?.LoginPacket == null) return;
		listServerConnection.SendPacket(
			new GS2LSP.VerifyAccountV2Packet(sessionSkeleton.LoginPacket.AccountName, sessionSkeleton.LoginPacket.Password, sessionSkeleton.Id, (byte)sessionSkeleton.Type, sessionSkeleton.LoginPacket.Identity)
		);
	}

	public void SendServerInfoForPlayer(ushort playerId, string serverName) =>
		listServerConnection.SendPacket(
			new GS2LSP.GetServerInfoPacket(playerId, serverName)
		);

	public Task<bool> SendAsync(ushort resultPlayerId, byte[] resultOutboundBytes) => clientConnections.SendAsync(resultPlayerId, resultOutboundBytes, CancellationToken.None);
}