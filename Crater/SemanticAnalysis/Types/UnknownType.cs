namespace Crater.SemanticAnalysis.Types;

public class UnknownType() : Type("unknown")
{
    public override bool CanHold(Type other) => true;
}