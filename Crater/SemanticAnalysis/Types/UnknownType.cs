namespace Crater.SemanticAnalysis.Types;

public class UnknownType() : Type("unknown", null, true)
{
    public override Type? ResolveUnaryOperation(string op) => this;

    public override Type? ResolveBinaryOperation(string op, Type other) => this;

    public override Type GetNullable() => this;
    public override Type GetNonNullable() => this;
}
