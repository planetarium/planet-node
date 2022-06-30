using GraphQL;
using GraphQL.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Assets;
using Libplanet.Blockchain;
using Libplanet.Crypto;
using Libplanet.Explorer.GraphTypes;
using Libplanet.Explorer.Mutations;
using PlanetNode.Action;

namespace PlanetNode.GraphTypes;

public class PlanetNodeMutation : ObjectGraphType
{
    public PlanetNodeMutation(BlockChain<PolymorphicAction<PlanetAction>> blockChain)
    {
        Field<TransactionMutation<PolymorphicAction<PlanetAction>>>(
            "transaction",
            resolve: context => new { }
        );

        Field<TransactionType<PolymorphicAction<PlanetAction>>>(
            "transferAsset",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "recipient" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "amount" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "privateKeyHex" }
            ),
            resolve: context =>
            {
                Address recipient = new Address(context.GetArgument<string>("recipient"));
                string amount = context.GetArgument<string>("amount");
                string privateKeyHex = context.GetArgument<string>("privateKeyHex");

                PrivateKey privateKey = PrivateKey.FromString(privateKeyHex);
                TransferAsset action = new TransferAsset(
                    privateKey.ToAddress(),
                    recipient,
                    FungibleAssetValue.Parse(
                        Currencies.PlanetNodeGold,
                        amount
                    )
                );

                return blockChain.MakeTransaction(
                    privateKey,
                    new PolymorphicAction<PlanetAction>[] { action }
                );
            }
        );
    }
}
