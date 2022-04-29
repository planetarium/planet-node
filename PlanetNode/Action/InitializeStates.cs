using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Assets;
using Libplanet.Headless.Extensions;

namespace PlanetNode.Action;

[ActionType(nameof(InitializeStates))]
public class InitializeStates : PlanetAction
{
    private Dictionary<Address, FungibleAssetValue> _assets;

    public InitializeStates()
    {
        _assets = new Dictionary<Address, FungibleAssetValue>();
    }

    public InitializeStates(Dictionary<Address, FungibleAssetValue> assets)
    {
        _assets = assets;
    }

    public override IValue PlainValue => new Dictionary(
        _assets.Select(kv => new KeyValuePair<IKey, IValue>(
            (Binary)kv.Key.ToIValue(),
            kv.Value.ToIValue()
        )
    ));

    public override IAccountStateDelta Execute(IActionContext context)
    {
        IAccountStateDelta? states = context.PreviousStates;

        if (context.BlockIndex != 0)
        {
            return states;
        }

        foreach ((Address address, FungibleAssetValue value) in _assets)
        {
            states = states.MintAsset(address, value);
        }

        return states;
    }

    public override void LoadPlainValue(IValue plainValue)
    {
        var asDict = (Dictionary)plainValue;

        _assets = new Dictionary<Address, FungibleAssetValue>(
            asDict.Select(kv =>
                new KeyValuePair<Address, FungibleAssetValue>(
                    kv.Key.ToAddress(),
                    kv.Value.ToFungibleAssetValue()
                )
            )
        );
    }
}
