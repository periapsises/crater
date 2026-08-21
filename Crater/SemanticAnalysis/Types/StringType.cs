namespace Crater.SemanticAnalysis.Types;

public class StringType(bool nullable = false) : Type("string", null, nullable)
{
    public override Type GetNullable() => new StringType(true);
    public override Type GetNonNullable() => new StringType();
}
