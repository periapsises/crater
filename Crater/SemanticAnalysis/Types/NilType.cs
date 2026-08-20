namespace Crater.SemanticAnalysis.Types;

public class NilType() : Type("nil")
{
    public override bool CanHold(Type other)
    {
        throw new Exception("NilType should never end up assigned to a variable's type.");
    }
}
