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

        if (this is AnyType)
            return true;

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

    public static Type? GetCommonType(Type left, Type right)
    {
        if (left is UnknownType || right is UnknownType)
            return new UnknownType();

        var leftNonNullable = left.Nullable ? left.GetNonNullable() : left;
        var rightNonNullable = right.Nullable ? right.GetNonNullable() : right;

        var currentAncestor = leftNonNullable;
        Type? matchedType = null;

        while (currentAncestor != null)
        {
            if (currentAncestor.CanHold(rightNonNullable))
            {
                matchedType = currentAncestor;
                break;
            }

            currentAncestor = currentAncestor.BaseType;
        }

        if (matchedType == null)
            return null;

        var nullable = left.Nullable || right.Nullable;
        return nullable ? matchedType.GetNullable() : matchedType.GetNonNullable();
    }

    public virtual Type? ResolveUnaryOperation(string op)
    {
        return op == "not" ? SemanticAnalyzer.BooleanType : null;
    }

    public virtual Type? ResolveBinaryOperation(string op, Type other)
    {
        if (op is "==" or "~=")
            return SemanticAnalyzer.BooleanType;

        if (other is UnknownType)
            return other;

        return op switch
        {
            "or" => ResolveLogicalOr(other),
            "and" => ResolveLogicalAnd(other),
            _ => null
        };
    }

    protected Type? ResolveLogicalOr(Type other)
    {
        var common = GetCommonType(this, other);
        if (common == null)
            return null;

        var nullable = Nullable && other.Nullable;
        return nullable ? common.GetNullable() : common.GetNonNullable();
    }

    protected Type? ResolveLogicalAnd(Type other)
    {
        var common = GetCommonType(this, other);
        if (common == null)
            return null;

        var nullable = Nullable || other.Nullable;
        return nullable ? common.GetNullable() : common.GetNonNullable();
    }

    public abstract Type GetNullable();
    public abstract Type GetNonNullable();

    public override string ToString()
    {
        if (Nullable)
            return Name + "?";

        return Name;
    }
}
