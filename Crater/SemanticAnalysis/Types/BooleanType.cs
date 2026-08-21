namespace Crater.SemanticAnalysis.Types;

public class BooleanType(bool nullable = false) : Type("bool", null, nullable)
{
    public override Type GetNullable() => new BooleanType(true);
    public override Type GetNonNullable() => new BooleanType();
}
