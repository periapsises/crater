namespace Crater.SemanticAnalysis.Types;

public class NumberType() : Type("number")
{
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
}
