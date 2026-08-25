using Crater.SemanticAnalysis.Types;

namespace Crater.SemanticAnalysis;

public abstract class Type(string name, Type? baseType = null)
{
    protected readonly string Name = name;

    public readonly Type? BaseType = baseType;

    public virtual string GetName() => Name;

    public virtual bool CanHold(Type other)
    {
        if (other is UnknownType)
            return true;

        if (GetType() == other.GetType())
            return true;

        return other.BaseType != null && CanHold(other.BaseType);
    }

    public static Type? GetCommonType(Type left, Type right)
    {
        if (left is UnknownType || right is UnknownType)
            return TypeRegistry.UnknownType;

        var leftNonNullable = left is NullableType nullableLeftType ? nullableLeftType.InnerType : left;
        var rightNonNullable = right is NullableType nullableRightType ? nullableRightType.InnerType : right;

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

        var nullable = left is NullableType || right is NullableType;
        return nullable ? new NullableType(matchedType) : matchedType;
    }

    public virtual Type? ResolveUnaryOperation(string op)
    {
        return op == "not" ? TypeRegistry.BooleanType : null;
    }

    public virtual Type? ResolveBinaryOperation(string op, Type other)
    {
        if (op is "==" or "~=")
            return TypeRegistry.BooleanType;

        if (other is UnknownType)
            return other;

        return op switch
        {
            "or" => ResolveLogicalOr(other),
            "and" => ResolveLogicalAnd(other),
            _ => null
        };
    }

    private Type? ResolveLogicalOr(Type other)
    {
        var common = GetCommonType(this, other);
        if (common == null)
            return null;

        if (this is NullableType && other is NullableType)
            return common is NullableType ? common : new NullableType(common);

        return common is NullableType nullableCommon ? nullableCommon.InnerType : common;
    }

    private Type? ResolveLogicalAnd(Type other)
    {
        var common = GetCommonType(this, other);
        if (common == null)
            return null;

        if (this is NullableType || other is NullableType)
            return common is NullableType ? common : new NullableType(common);

        return common is NullableType nullableCommon ? nullableCommon.InnerType : common;
    }

    public override string ToString() => GetName();
}
