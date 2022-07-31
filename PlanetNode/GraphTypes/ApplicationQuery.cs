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
            description: "The specified address's balance in PNG.",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>>
                {
                    Name = "address",
                    Description = "The account holder's 40-hex address",
                }
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
