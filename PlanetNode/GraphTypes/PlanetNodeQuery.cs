using GraphQL.Types;
using Libplanet.Action;
using Libplanet.Explorer.Queries;
using PlanetNode.Action;

namespace PlanetNode.GraphTypes;

public class PlanetNodeQuery : ObjectGraphType
{
    public PlanetNodeQuery()
    {
        Name = "PlanetNodeQuery";
        Field<ExplorerQuery<PolymorphicAction<PlanetAction>>>(
            "explorer",
            deprecationReason: "Use /graphql/explorer endpoint.",
            resolve: context => new { }
        );

        // For compatibility with libplanet-explorer-frontend.
        Field<ExplorerQuery<PolymorphicAction<PlanetAction>>>(
            "chainQuery",
            deprecationReason: "Use /graphql/explorer endpoint.",
            resolve: context => new { }
        );
        Field<ApplicationQuery>(
            "application",
            resolve: context => new { }
        );
    }
}
