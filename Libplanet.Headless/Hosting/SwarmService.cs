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

    private string getPeerString(Peer peer)
    {
        var pubKey = peer.PublicKey.ToString();
        var hostAndPort = peer.ToString().Split('/')[1];
        var host = hostAndPort.Split(':')[0];
        var port = hostAndPort.Split(':')[1];
        return $"peerString: {pubKey},{host},{port}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = _swarm.WaitForRunningAsync().ContinueWith(_ =>
        {
            var peer = _swarm.AsPeer;
            var result = getPeerString(peer);
            Console.WriteLine(result);
        });
        await _swarm.PreloadAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
        await _swarm.StartAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
    }
}
