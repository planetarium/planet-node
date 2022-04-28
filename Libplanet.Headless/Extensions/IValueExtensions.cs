using Bencodex.Types;
using Libplanet.Assets;

namespace Libplanet.Headless.Extensions;

public static class IValueExtensions
{
    public static IValue ToIValue(this FungibleAssetValue fav) =>
        new List(fav.Currency.Serialize(), (Integer)fav.RawValue);

    public static FungibleAssetValue ToFungibleAssetValue(this IValue value) =>
        FungibleAssetValue.FromRawValue(
            new Currency(((List)value)[0]),
            ((Integer)((List)value)[1]).Value
        );

    public static IValue ToIValue(this Address address) => (Binary)address.ToByteArray();
    public static Address ToAddress(this IValue value) => new((Binary)value);
}
