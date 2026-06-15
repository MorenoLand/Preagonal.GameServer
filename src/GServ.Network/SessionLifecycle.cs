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
    LevelPayloadSent,
    Authenticated,
    Rejected,
    Disconnecting,
    Disconnected
}
