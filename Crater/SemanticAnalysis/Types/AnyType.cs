namespace Crater.SemanticAnalysis.Types;

public class AnyType(bool nullable = false) : Type("any", null, nullable)
{
    public override Type GetNullable() => new AnyType(true);
    public override Type GetNonNullable() => new AnyType();
}
