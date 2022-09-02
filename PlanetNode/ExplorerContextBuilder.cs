using GraphQL.Server.Transports.AspNetCore;
using Libplanet.Action;
using Libplanet.Explorer.Interfaces;
using Libplanet.Store;
using PlanetNode.Action;

namespace PlanetNode;

internal class ExplorerContextBuilder : IUserContextBuilder
{
    private readonly IStore _store;

    public ExplorerContextBuilder(IStore store)
    {
        _store = store;
    }

    public Task<IDictionary<string, object>> BuildUserContext(HttpContext httpContext) =>
        new ValueTask<IDictionary<string, object?>>(new Dictionary<string, object?>
        {
            [nameof(IBlockChainContext<PolymorphicAction<PlanetAction>>.Store)] = _store,
        }).AsTask()!;
}
