namespace Crater.SemanticAnalysis.Types;

public class BooleanType() : Type("bool")
{
    public override bool CanHold(Type other)
    {
        return other is BooleanType;
    }
}
