namespace Crater.SemanticAnalysis.Types;

public class NumberType(Type baseType) : Type("number", baseType)
{
    private static readonly HashSet<string> Operators = ["+", "-", "*", "/", "<", ">", "<=", ">="];
    private static readonly HashSet<string> ArithmeticOperators = ["+", "-", "*", "/"];

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

        if (other is NullableType)
            return null;

        if (other is not NumberType)
            return null;

        if (!Operators.Contains(op))
            return null;

        if (ArithmeticOperators.Contains(op))
            return this;

        return TypeRegistry.BooleanType;
    }
}
