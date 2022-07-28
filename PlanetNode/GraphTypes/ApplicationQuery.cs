using GraphQL;
using GraphQL.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Assets;
using Libplanet.Blockchain;
using PlanetNode.Action;

namespace PlanetNode.GraphTypes;

public class ApplicationQuery : ObjectGraphType
{
    public ApplicationQuery(BlockChain<PolymorphicAction<PlanetAction>> blockChain)
    {
        Field<StringGraphType>(
            "asset",
            description: "Queries address's asset(The unit of asset(balance) amount is PNG.)",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "address", Description = "Argument adress is public key. That is like account holder." }
            ),
            resolve: context =>
            {
                var accountAddress = new Address(context.GetArgument<string>("address"));
                FungibleAssetValue asset = blockChain.GetBalance(
                    accountAddress,
                    Currencies.PlanetNodeGold
                );

                return asset.ToString();
            }
        );
    }
}
