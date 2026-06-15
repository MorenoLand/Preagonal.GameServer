namespace GServ.Network.Session;

/// <summary>
/// Session categories selected by PlayerLogin::msgLoginPacket in the original C++ server.
/// </summary>
public enum SessionKind
{
    AwaitingLogin,
    OriginalClient,
    Client,
    RemoteControl,
    NpcControl,
    NpcServer
}
