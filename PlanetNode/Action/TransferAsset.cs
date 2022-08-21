using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Assets;
using Libplanet.Headless.Extensions;

namespace PlanetNode.Action;

// TODO: As PNG is now a native token, this action should be replaced by Libplanet's system action
// Transfer.
[ActionType(nameof(TransferAsset))]
public class TransferAsset : PlanetAction
{
    public TransferAsset()
    {
    }

    public TransferAsset(Address sender, Address recipient, FungibleAssetValue amount)
    {
        Sender = sender;
        Recipient = recipient;
        Amount = amount;
    }

    public Address Sender { get; private set; }

    public Address Recipient { get; private set; }

    public FungibleAssetValue Amount { get; private set; }

    public override IValue PlainValue
    {
        get
        {
            IEnumerable<KeyValuePair<IKey, IValue>> pairs = new[]
            {
                new KeyValuePair<IKey, IValue>((Text)nameof(Sender), Sender.ToIValue()),
                new KeyValuePair<IKey, IValue>((Text)nameof(Recipient), Recipient.ToIValue()),
                new KeyValuePair<IKey, IValue>((Text)nameof(Amount), Amount.ToIValue()),
            };

            return new Dictionary(pairs);
        }
    }

    public override IAccountStateDelta Execute(IActionContext context)
    {
        IAccountStateDelta? state = context.PreviousStates;

        if (Sender != context.Signer)
        {
            throw new InvalidTransferSignerException(context.Signer, Sender, Recipient);
        }

        return state.TransferAsset(Sender, Recipient, Amount);
    }

    public override void LoadPlainValue(IValue plainValue)
    {
        var asDict = (Dictionary)plainValue;

        Sender = asDict[nameof(Sender)].ToAddress();
        Recipient = asDict[nameof(Recipient)].ToAddress();
        Amount = asDict[nameof(Amount)].ToFungibleAssetValue();
    }
}
