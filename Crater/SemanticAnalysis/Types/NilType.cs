namespace Crater.SemanticAnalysis.Types;

public class NilType() : Type("nil", null, true)
{
    public override Type GetNullable() => this;
    public override Type GetNonNullable() => this;
}
