namespace GServ.Network;

public enum SessionLifecycle
{
    AwaitingLoginPrelude,
    LoginPreludeParsed,
    Authenticated,
    Rejected,
    Disconnecting,
    Disconnected
}
