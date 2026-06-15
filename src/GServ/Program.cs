using GServ.Core.Configuration;
using GServ.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

builder.Services.AddSingleton(ServerCompatibilityOptions.Default);
builder.Services.AddSingleton<ListenerService>();
builder.Services.AddHostedService<GServFoundationService>();

await builder.Build().RunAsync();

internal sealed class GServFoundationService : IHostedService
{
    private readonly ListenerService _listener;
    private readonly ILogger<GServFoundationService> _logger;

    public GServFoundationService(ListenerService listener, ILogger<GServFoundationService> logger)
    {
        _listener = listener;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GServharp foundation starting. Gameplay systems are not implemented yet.");
        _logger.LogInformation("Compatibility mode: C++ GServer behavior is the source of truth.");
        await _listener.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GServharp foundation stopping.");
        await _listener.StopAsync(cancellationToken);
    }
}
