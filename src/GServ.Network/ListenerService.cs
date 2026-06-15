using GServ.Core.Configuration;

namespace GServ.Network;

/// <summary>
/// Networking entry point placeholder. The real TCP accept loop will map to Server::onRecv.
/// </summary>
public sealed class ListenerService
{
    public ListenerService(ServerCompatibilityOptions compatibilityOptions)
    {
        CompatibilityOptions = compatibilityOptions;
    }

    public ServerCompatibilityOptions CompatibilityOptions { get; }

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}
