using Crater.SemanticAnalysis.Types;

namespace Crater.SemanticAnalysis;

public class NullableType : Type
{
    public readonly Type InnerType;

    public override string GetName() => base.GetName() + "?";

    public NullableType(Type innerType) : base(innerType.GetName())
    {
        if (innerType is NullableType)
            throw new Exception("A nullable type cannot encapsulate another nullable type.");

        InnerType = innerType;
    }

    public override bool CanHold(Type other)
    {
        if (other is UnknownType)
            return true;

        if (other is NilType)
            return true;

        if (other is NullableType nullableOther)
            return InnerType.CanHold(nullableOther.InnerType);

        return InnerType.CanHold(other);
    }

    public override bool IsSameType(Type other)
    {
        return other is NullableType otherNullable && InnerType.IsSameType(otherNullable.InnerType);
    }
}
