using GraphQL.Types;
using Libplanet.Action;
using Libplanet.Explorer.Queries;
using PlanetNode.Action;

namespace PlanetNode.GraphTypes;

public class PlanetNodeSchema : Schema
{
    public PlanetNodeSchema(IServiceProvider services)
        : base(services)
    {
        Query = services.GetRequiredService<ExplorerQuery<PolymorphicAction<PlanetAction>>>();
        Mutation = services.GetRequiredService<PlanetNodeMutation>();
    }
}
