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