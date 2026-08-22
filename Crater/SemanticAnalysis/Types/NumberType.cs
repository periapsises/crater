namespace Crater.SemanticAnalysis.Types;

public class NumberType(bool nullable = false) : Type("number", null, nullable)
{
    public override Type? ResolveUnaryOperation(string op)
    {
        if (op == "-")
            return this;

        return base.ResolveUnaryOperation(op);
    }

    public override Type? ResolveBinaryOperation(string op, Type other)
    {
        var baseResult = base.ResolveBinaryOperation(op, other);
        if (baseResult != null)
            return baseResult;

        if (op is not ("+" or "-" or "*" or "/"))
            return null;

        if (Nullable || other.Nullable)
            return null;

        if (other is NumberType)
            return this;

        return null;
    }

    public override Type GetNullable() => new NumberType(true);
    public override Type GetNonNullable() => new NumberType();
}
