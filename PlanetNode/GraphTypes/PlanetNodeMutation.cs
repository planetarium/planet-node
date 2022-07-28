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
            description: "Adds transaction to the pending list so that a next Block to be mined may contain given transaction.",
            resolve: context => new { }
        );

        Field<TransactionType<PolymorphicAction<PlanetAction>>>(
            "transferAsset",
            description: "Mutates a new transaction about transfer and stage the transaction.",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "recipient", Description = "Argument adress is the recipient's public key." },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "amount", Description = "Argument amount is transfer amount. The unit of transfer amount is PNG." },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "privateKeyHex", Description = "Argument privateKeyHex is the sender's hexadecimal private key used for signing." }
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
