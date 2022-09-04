using System.Collections.Immutable;
using System.Security.Cryptography;
using Libplanet.Action;
using Libplanet.Assets;
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
        IEnumerable<T> genesisActions,
        IImmutableSet<Currency> nativeTokens
    )
        where T : IAction, new()
    {
        services.AddSingleton<IBlockPolicy<T>>(
            _ => new BlockPolicy<T>(nativeTokens: nativeTokens)
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
                return store.GetBlock<T>(genesisHash);
            }
            else
            {
                return BlockChain<T>.MakeGenesisBlock(genesisActions);
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

        BoundPeer[] peers = configuration.PeerStrings is { } ? configuration.PeerStrings.
            Select(BoundPeer.ParsePeer).ToArray() : new BoundPeer[] { };

        services.AddHostedService(provider =>
            new SwarmService<T>(
                provider.GetRequiredService<Swarm<T>>(),
                peers
            )
        );

        if (configuration.MinerPrivateKeyString is { } minerPrivateKey)
        {
            services.AddHostedService(provider =>
                new MinerService<T>(
                    provider.GetRequiredService<BlockChain<T>>(),
                    PrivateKey.FromString(minerPrivateKey)
                )
            );
        }

        return services;
    }
}
