using Libplanet.Blockchain;
using Libplanet.Explorer.Interfaces;
using Libplanet.Net;
using Libplanet.Store;
using PlanetNode.Action;

namespace PlanetNode;

public class ExplorerContext : IBlockChainContext<PlanetAction>
{
    private readonly Swarm<PlanetAction> _swarm;
    public ExplorerContext(
        BlockChain<PlanetAction> blockChain,
        IStore store,
        Swarm<PlanetAction> swarm
    )
    {
        BlockChain = blockChain;
        Store = store;
        _swarm = swarm;
    }

    public bool Preloaded => _swarm.Running;

    public BlockChain<PlanetAction> BlockChain { get; private set; }

    public IStore Store { get; private set; }
}
