using GServ.Core.Ids;

namespace GServ.Network.Session;

public sealed class GameClientSession : ClientSessionBase
{
    public GameClientSession(PlayerId id, bool originalClient = false)
        : base(id, originalClient ? SessionKind.OriginalClient : SessionKind.Client)
    {
    }
}
