using Libplanet.Action;
using Libplanet.Net;
using Microsoft.Extensions.Hosting;

namespace Libplanet.Headless.Hosting;

public class SwarmService<T> : BackgroundService, IDisposable
    where T : IAction, new()
{
    private readonly Swarm<T> _swarm;

    public SwarmService(Swarm<T> swarm)
    {
        _swarm = swarm;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _swarm.PreloadAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
        await _swarm.StartAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
    }
}
