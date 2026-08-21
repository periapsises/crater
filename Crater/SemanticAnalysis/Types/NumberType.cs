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
        if (other is NumberType)
        {
            return op switch
            {
                "+" or "-" or "*" or "/" => this,
                _ => null
            };
        }

        return base.ResolveBinaryOperation(op, other);
    }

    public override Type GetNullable() => new NumberType(true);
    public override Type GetNonNullable() => new NumberType();
}
