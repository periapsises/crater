using Crater.SemanticAnalysis.Types;

namespace Crater.SemanticAnalysis;

public abstract class Type(string name)
{
    public readonly string Name = name;

    public virtual bool CanHold(Type other)
    {
        return other == this || other is UnknownType;
    }

    public virtual Type? ResolveUnaryOperation(string op)
    {
        return null;
    }

    public virtual Type? ResolveBinaryOperation(string op, Type other)
    {
        return other is UnknownType ? other : null;
    }
}
