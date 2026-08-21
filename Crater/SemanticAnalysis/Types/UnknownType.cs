namespace Crater.SemanticAnalysis.Types;

public class UnknownType() : Type("unknown")
{
    public override bool CanHold(Type other) => true;

    public override Type? ResolveUnaryOperation(string op) => this;

    public override Type? ResolveBinaryOperation(string op, Type other) => this;
}
