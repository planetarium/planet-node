using Libplanet;
using Libplanet.Assets;

namespace PlanetNode;

public static class Currencies
{
    public static Currency PlanetNodeGold => Currency.Uncapped("PNG", 18, null);
}
