namespace Crater.SemanticAnalysis.Types;

public class NumberType() : Type("number")
{
    public override bool CanHold(Type other)
    {
        return other is NumberType;
    }
}
