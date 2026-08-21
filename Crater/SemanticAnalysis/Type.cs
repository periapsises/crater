using Crater.SemanticAnalysis.Types;

namespace Crater.SemanticAnalysis;

public abstract class Type
{
    public readonly string Name;
    public readonly Type? BaseType;
    public readonly bool Nullable;

    protected Type(string name, Type? baseType = null, bool nullable = false)
    {
        Name = name;
        BaseType = baseType;
        Nullable = nullable;
    }

    public bool CanHold(Type other)
    {
        if (this is UnknownType || other is UnknownType)
            return true;

        if (!Nullable && other.Nullable)
            return false;

        if (other is NilType)
            return Nullable;

        return other.IsSubtypeOf(this);
    }

    private bool IsSubtypeOf(Type target)
    {
        var current = this;
        while (current != null)
        {
            if (current.IsSameTypeAs(target))
                return true;

            current = current.BaseType;
        }

        return false;
    }

    protected virtual bool IsSameTypeAs(Type other)
    {
        return Name == other.Name;
    }

    public virtual Type? ResolveUnaryOperation(string op)
    {
        return op == "not" ? SemanticAnalyzer.BooleanType : null;
    }

    public virtual Type? ResolveBinaryOperation(string op, Type other)
    {
        return other is UnknownType ? other : null;
    }

    public abstract Type GetNullable();
    public abstract Type GetNonNullable();
}
