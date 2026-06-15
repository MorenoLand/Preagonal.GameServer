using GServ.Core.Ids;

namespace GServ.Network.Session;

public abstract class ClientSessionBase : IClientSession
{
    protected ClientSessionBase(PlayerId id, SessionKind kind)
    {
        Id = id;
        Kind = kind;
    }

    public PlayerId Id { get; }
    public SessionKind Kind { get; }
    public bool IsLoaded { get; protected set; }

    public virtual ValueTask ReceiveAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask DisconnectAsync(string? message, CancellationToken cancellationToken)
    {
        IsLoaded = false;
        return ValueTask.CompletedTask;
    }
}
