using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Crypto;
using Microsoft.Extensions.Hosting;

namespace Libplanet.Headless.Hosting;

public class MinerService<T> : BackgroundService, IDisposable
    where T : IAction, new()
{
    private readonly BlockChain<T> _blockChain;

    private readonly PrivateKey _privateKey;

    public MinerService(BlockChain<T> blockChain, Configuration configuration)
    {
        _blockChain = blockChain;
        _privateKey = PrivateKey.FromString(configuration.MinerPrivateKeyString);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _blockChain.MineBlock(_privateKey).ConfigureAwait(false);
            stoppingToken.ThrowIfCancellationRequested();
        }
    }
}
