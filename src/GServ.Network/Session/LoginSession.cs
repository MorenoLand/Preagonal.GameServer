using GServ.Core.Ids;

namespace GServ.Network.Session;

/// <summary>
/// Temporary session corresponding to C++ PlayerLogin before the first packet selects a concrete player type.
/// </summary>
public sealed class LoginSession : ClientSessionBase
{
    public LoginSession(PlayerId id) : base(id, SessionKind.AwaitingLogin)
    {
    }
}
