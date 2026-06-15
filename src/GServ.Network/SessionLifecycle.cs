namespace GServ.Network;

public enum SessionLifecycle
{
    AwaitingLoginPrelude,
    LoginPreludeParsed,
    WaitingForServerListAuth,
    ServerListAuthAcceptedPreWorld,
    ReadyForWorldEntry,
    ReadyForLevelWarp,
    ReadyForLevelRuntime,
    Authenticated,
    Rejected,
    Disconnecting,
    Disconnected
}
