using GServ.Network;
using GServ.Protocol;
using Xunit;

namespace GServ.Network.Tests;

public sealed class SendLevelBoundaryTests
{
    [Fact]
    public void BeginModernQueuesLevelNameModTimeLinksAndSignsWhenCacheEmptyAndBoardIsCurrent()
    {
        var session = ReadyForLevelRuntimeSession();
        var level = new ModernLevelPayload(
            LevelName: "start.nw",
            LevelModTime: 1,
            BoardPacket: EmptyBoardPacket(),
            Layers: [],
            LinksPacket: "links\n"u8.ToArray(),
            SignsPacket: "signs\n"u8.ToArray());

        var result = SendLevelBoundary.BeginModern(
            session,
            level,
            new SendLevelRequest(RequestedModTime: 1, CachedLevelModTime: 0, FromAdjacent: false));

        Assert.True(result.Accepted);
        Assert.Equal(SendLevelStopPoint.BeforeDynamicLevelRuntime, result.StopPoint);
        Assert.Equal(SessionLifecycle.LevelPayloadSent, session.Lifecycle);
        Assert.Equal(
            new byte[]
            {
                38, (byte)'s', (byte)'t', (byte)'a', (byte)'r', (byte)'t', (byte)'.', (byte)'n', (byte)'w', 10,
                71, 32, 32, 32, 32, 33, 10,
                (byte)'l', (byte)'i', (byte)'n', (byte)'k', (byte)'s', 10,
                (byte)'s', (byte)'i', (byte)'g', (byte)'n', (byte)'s', 10
            },
            session.TakeOutboundBytes());
    }

    [Fact]
    public void BeginModernQueuesRawBoardAndLayersBeforeModTimeWhenClientNeedsLevelPayload()
    {
        var session = ReadyForLevelRuntimeSession();
        var board = EmptyBoardPacket();
        var layer = new byte[] { 35, 2, 0, 0, 64, 64, 10 };
        var level = new ModernLevelPayload(
            LevelName: "start.nw",
            LevelModTime: 1,
            BoardPacket: board,
            Layers: [new LevelLayerPayload(1, layer)],
            LinksPacket: [],
            SignsPacket: []);

        var result = SendLevelBoundary.BeginModern(
            session,
            level,
            new SendLevelRequest(RequestedModTime: 0, CachedLevelModTime: 0, FromAdjacent: false));

        Assert.True(result.Accepted);
        var bytes = session.TakeOutboundBytes();
        Assert.Equal(
            new byte[] { 38, (byte)'s', (byte)'t', (byte)'a', (byte)'r', (byte)'t', (byte)'.', (byte)'n', (byte)'w', 10, 132, 32, 96, 34, 10 },
            bytes[..15]);
        Assert.Equal(board, bytes[15..(15 + board.Length)]);
        var layerHeaderStart = 15 + board.Length;
        Assert.Equal(new byte[] { 132, 32, 32, 39, 10 }, bytes[layerHeaderStart..(layerHeaderStart + 5)]);
        Assert.Equal(layer, bytes[(layerHeaderStart + 5)..(layerHeaderStart + 5 + layer.Length)]);
        Assert.Equal(new byte[] { 71, 32, 32, 32, 32, 33, 10 }, bytes[^7..]);
    }

    [Fact]
    public void BeginModernUsesLevelModTimeWhenRequestedModTimeIsMinusOne()
    {
        var session = ReadyForLevelRuntimeSession();
        var level = new ModernLevelPayload(
            LevelName: "start.nw",
            LevelModTime: 1,
            BoardPacket: EmptyBoardPacket(),
            Layers: [],
            LinksPacket: [],
            SignsPacket: []);

        SendLevelBoundary.BeginModern(
            session,
            level,
            new SendLevelRequest(RequestedModTime: -1, CachedLevelModTime: 0, FromAdjacent: false));

        Assert.Equal(
            new byte[]
            {
                38, (byte)'s', (byte)'t', (byte)'a', (byte)'r', (byte)'t', (byte)'.', (byte)'n', (byte)'w', 10,
                71, 32, 32, 32, 32, 33, 10
            },
            session.TakeOutboundBytes());
    }

    [Fact]
    public void BeginModernSkipsStaticLevelPayloadWhenCachedModTimeIsKnown()
    {
        var session = ReadyForLevelRuntimeSession();
        var level = new ModernLevelPayload(
            LevelName: "start.nw",
            LevelModTime: 1,
            BoardPacket: EmptyBoardPacket(),
            Layers: [],
            LinksPacket: "links\n"u8.ToArray(),
            SignsPacket: "signs\n"u8.ToArray());

        SendLevelBoundary.BeginModern(
            session,
            level,
            new SendLevelRequest(RequestedModTime: 0, CachedLevelModTime: 1, FromAdjacent: false));

        Assert.Equal(
            new byte[] { 38, (byte)'s', (byte)'t', (byte)'a', (byte)'r', (byte)'t', (byte)'.', (byte)'n', (byte)'w', 10 },
            session.TakeOutboundBytes());
    }

    private static ClientSessionSkeleton ReadyForLevelRuntimeSession()
    {
        var session = new ClientSessionSkeleton(7);
        var packet = new GraalBinaryWriter();
        packet.WriteGChar(5);
        packet.WriteGChar(42);
        packet.WriteBytes("G3D0311C"u8);
        packet.WriteGChar(4);
        packet.WriteBytes("Ruan"u8);
        packet.WriteGChar(2);
        packet.WriteBytes("pw"u8);
        packet.WriteBytes("win"u8);
        Assert.True(session.ReceiveLoginPacket(packet.ToArray()));
        Assert.True(session.ReceiveServerListAuthResponse(
            new ServerListVerifyAccount2Response("pc:Ruan", 7, PlayerSessionType.Client3, "SUCCESS")));
        Assert.True(PlayerSendLoginContinuation.Begin(
            session,
            new PlayerSendLoginAccount("pc:Ruan", false, "", false, false, true, ["0.0.0.0"], false),
            new PlayerSendLoginOptions(false, "Graal Reborn", [])).Accepted);
        _ = session.TakeOutboundBytes();

        PostLoginWorldEntryBoundary.BeginClient(session, BaseSnapshot());
        _ = session.TakeOutboundBytes();

        var levels = new MemoryLevelLookup();
        levels.Add(new LevelEntrySnapshot("start.nw"));
        var result = WarpWorldEntryBoundary.BeginSetLevel(
            session,
            levels,
            new LevelWarpRequest("start.nw", 30, 30, 0, ClientVersionId.Client21, 123));
        Assert.True(result.Accepted);
        _ = session.TakeOutboundBytes();
        return session;
    }

    private static byte[] EmptyBoardPacket()
    {
        var board = new byte[1 + 64 * 64 * 2 + 1];
        board[0] = 133;
        board[^1] = 10;
        return board;
    }

    private static PostLoginPlayerSnapshot BaseSnapshot()
    {
        var prop = new GraalBinaryWriter();
        prop.WriteGChar(0);

        return new PostLoginPlayerSnapshot(
            PlayerId: 7,
            Type: PlayerSessionType.Client3,
            AccountNameProperty: prop.ToArray(),
            NicknameProperty: prop.ToArray(),
            CurrentLevelProperty: prop.ToArray(),
            XProperty: [64],
            YProperty: [65],
            AlignmentProperty: [66],
            IpAddressProperty: [32, 32, 32, 32, 33],
            LoginPropertySource: new PlayerPropertySource(
                Nickname: "Ruan",
                MaxPower: 3,
                Hitpoints: 4,
                Rupees: 0,
                Arrows: 0,
                Bombs: 0,
                GlovePower: 0,
                SwordPower: 0,
                SwordImage: "",
                ShieldPower: 0,
                ShieldImage: "",
                Gani: "",
                HeadImage: "",
                ChatMessage: "",
                Colors: [0, 0, 0, 0, 0],
                PlayerId: 7,
                X: 0,
                Y: 0,
                Sprite: 0,
                Status: 0,
                CarrySprite: 0,
                CurrentLevel: "start.nw",
                HorseImage: "",
                HorseBombCount: 0,
                CarryNpcId: 0,
                ApCounter: 0,
                MagicPoints: 0,
                Kills: 0,
                Deaths: 0,
                OnlineSeconds: 0,
                AccountIp: 1,
                Alignment: 0,
                AdditionalFlags: 0,
                AccountName: "pc:Ruan",
                BodyImage: "",
                EloRating: 0,
                EloDeviation: 0,
                GaniAttributes: new Dictionary<int, string>(),
                Os: "",
                TextCodePage: 0,
                CommunityName: "Ruan"),
            LoginPropertyIds: [],
            PlayerFlags: [],
            ServerFlags: []);
    }

    private sealed class MemoryLevelLookup : ILevelLookup
    {
        private readonly Dictionary<string, LevelEntrySnapshot> _levels = new(StringComparer.OrdinalIgnoreCase);

        public LevelEntrySnapshot? FindLevel(string levelName) =>
            _levels.GetValueOrDefault(levelName);

        public void Add(LevelEntrySnapshot level) =>
            _levels[level.LevelName] = level;
    }
}
