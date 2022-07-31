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
            description: "Adds a transaction to the pending list so that a next block to be " +
                "mined may contain the given transaction.",
            resolve: context => new { }
        );

        Field<TransactionType<PolymorphicAction<PlanetAction>>>(
            "transferAsset",
            description: "Mutates a new transaction about transfer and stage the transaction.",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>>
                {
                	    Name = "recipient",
                	    Description = "The recipient's 40-hex address.",
                	},
                new QueryArgument<NonNullGraphType<StringGraphType>>
                {
                	    Name = "amount",
                	    Description = "The amount to transfer in PNG.",
                	},
                new QueryArgument<NonNullGraphType<StringGraphType>>
                {
                	    Name = "privateKeyHex",
                	    Description = "A hex-encoded private key of the sender.  A made " +
                	        "transaction will be signed using this key.",
                	}
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
