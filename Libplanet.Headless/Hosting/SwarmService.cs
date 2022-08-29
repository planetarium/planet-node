using Libplanet.Action;
using Libplanet.Net;
using Microsoft.Extensions.Hosting;

namespace Libplanet.Headless.Hosting;

public class SwarmService<T> : BackgroundService, IDisposable
    where T : IAction, new()
{
    private readonly Swarm<T> _swarm;
    private readonly Peer[] _peers;

    public SwarmService(Swarm<T> swarm, Peer[] peers)
    {
        _swarm = swarm;
        _peers = peers;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _swarm.AddPeersAsync(_peers, default, cancellationToken: stoppingToken).ConfigureAwait(false);
        await _swarm.PreloadAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
        await _swarm.StartAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
    }
}
