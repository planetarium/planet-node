using Libplanet;
using Libplanet.Assets;

namespace PlanetNode;

public static class Currencies
{
    public static Currency PlanetNodeGold => new("PNG", 18, default(Address?));
}
