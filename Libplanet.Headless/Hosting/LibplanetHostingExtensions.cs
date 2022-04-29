using System.Security.Cryptography;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Blocks;
using Libplanet.Crypto;
using Libplanet.Net;
using Libplanet.Store;
using Microsoft.Extensions.DependencyInjection;

namespace Libplanet.Headless.Hosting;

public static class LibplanetServicesExtensions
{
    public static IServiceCollection AddLibplanet<T>(
        this IServiceCollection services,
        Configuration configuration,
        IEnumerable<T> genesisActions)
        where T : IAction, new()
    {
        services.AddSingleton<IBlockPolicy<T>>(
            _ => new BlockPolicy<T>()
        );
        services.AddSingleton<IStagePolicy<T>>(
            _ => new VolatileStagePolicy<T>()
        );
        services.AddSingleton<IStore>(_ => new RocksDBStore.RocksDBStore(configuration.StorePath));
        services.AddSingleton<IStateStore>(_ => new TrieStateStore(
            new RocksDBStore.RocksDBKeyValueStore(
                Path.Combine(configuration.StorePath!, "states")
            )
        ));
        services.AddSingleton(provider =>
        {
            IStore store = provider.GetRequiredService<IStore>();
            IBlockPolicy<T> blockPolicy =
                provider.GetRequiredService<IBlockPolicy<T>>();
            if (store.GetCanonicalChainId() is Guid cid &&
                store.CountIndex(cid) > 0)
            {
                BlockHash genesisHash = store.IterateIndexes(cid, 0, 1).Single();
                return store.GetBlock<T>(blockPolicy.GetHashAlgorithm, genesisHash);
            }
            else
            {
                return BlockChain<T>.MakeGenesisBlock(
                    HashAlgorithmType.Of<SHA256>(),
                    genesisActions
                );
            }
        });
        services.AddSingleton(provider =>
        {
            return new Swarm<T>(
                provider.GetRequiredService<BlockChain<T>>(),
                new PrivateKey(),
                default,
                host: configuration.Host,
                listenPort: configuration.Port
            );
        });
        services.AddSingleton<BlockChain<T>>();
        services.AddSingleton(_ => configuration);
        services.AddHostedService<SwarmService<T>>();
        services.AddHostedService<MinerService<T>>();

        return services;
    }
}
