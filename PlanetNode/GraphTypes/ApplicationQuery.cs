using GraphQL;
using GraphQL.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Assets;
using Libplanet.Blockchain;
using Libplanet.Net;
using PlanetNode.Action;

namespace PlanetNode.GraphTypes;

public class ApplicationQuery : ObjectGraphType
{
    private string getPeerString(BoundPeer peer)
    {
        var pubKey = peer.PublicKey.ToString();
        var hostAndPort = peer.ToString().Split('/')[1];
        var host = hostAndPort.Split(':')[0];
        var port = hostAndPort.Split(':')[1];
        return $"{pubKey},{host},{port}";
    }

    public ApplicationQuery(BlockChain<PolymorphicAction<PlanetAction>> blockChain
    , Swarm<PolymorphicAction<PlanetAction>> _swarm)
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
        Field<StringGraphType>(
            "peerString",
            resolve: context =>
            {
                var peer = _swarm.AsPeer;
                var peerString = getPeerString(peer);

                return peerString;
            }
        );
    }
}
