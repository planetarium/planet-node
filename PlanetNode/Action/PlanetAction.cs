using Bencodex.Types;
using Libplanet.Action;

namespace PlanetNode.Action;

public class PlanetAction : IAction
{
    public IValue PlainValue => throw new NotImplementedException();

    public IAccountStateDelta Execute(IActionContext context)
    {
        throw new NotImplementedException();
    }

    public void LoadPlainValue(IValue plainValue)
    {
        throw new NotImplementedException();
    }
}
