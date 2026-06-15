using GServ.Core.Ids;

namespace GServ.Network.Session;

public interface IClientSession
{
    PlayerId Id { get; }
    SessionKind Kind { get; }
    bool IsLoaded { get; }
    ValueTask ReceiveAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken);
    ValueTask DisconnectAsync(string? message, CancellationToken cancellationToken);
}
