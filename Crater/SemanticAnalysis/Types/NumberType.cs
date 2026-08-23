namespace Crater.SemanticAnalysis.Types;

public class NumberType(bool nullable = false) : Type("number", null, nullable)
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

        if (!Operators.Contains(op))
            return null;

        if (Nullable || other.Nullable)
            return null;

        if (other is not NumberType)
            return null;

        if (ArithmeticOperators.Contains(op))
            return this;

        return SemanticAnalyzer.BooleanType;
    }

    public override Type GetNullable() => new NumberType(true);
    public override Type GetNonNullable() => new NumberType();
}
