namespace Crater.SemanticAnalysis.Types;

public class StringType() : Type("string")
{
    public override bool CanHold(Type other)
    {
        return other is StringType;
    }
}
