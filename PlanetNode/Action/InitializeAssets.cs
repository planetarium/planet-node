using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Assets;

namespace PlanetNode.Action;

[ActionType(nameof(InitializeAssets))]
public class InitializeAssets : PlanetAction
{
    private Dictionary<Address, FungibleAssetValue> _assets;

    public InitializeAssets()
    {
        _assets = new Dictionary<Address, FungibleAssetValue>();
    }

    public override IValue PlainValue => new Dictionary(
        _assets.Select(kv => new KeyValuePair<IKey, IValue>(
            (Binary)kv.Key.ToByteArray(),
            new List()
            {
                kv.Value.Currency.Serialize(),
                kv.Value.RawValue
            }
        )
    ));

    public override IAccountStateDelta Execute(IActionContext context)
    {
        IAccountStateDelta? states = context.PreviousStates;

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
                    new Address((Binary)kv.Key),
                    FungibleAssetValue.FromRawValue(
                        new Currency(((List)kv.Value)[0]),
                        ((Integer)((List)kv.Value)[1]).Value
                    )
                )
            )
        );
    }
}
