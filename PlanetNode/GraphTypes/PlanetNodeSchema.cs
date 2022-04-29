using GraphQL.Types;

namespace PlanetNode.GraphTypes;

public class PlanetNodeSchema : Schema
{
    public PlanetNodeSchema(IServiceProvider services)
        : base(services)
    {
        Query = services.GetRequiredService<PlanetNodeQuery>();
        Mutation = services.GetRequiredService<PlanetNodeMutation>();
    }
}
