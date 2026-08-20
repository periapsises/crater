namespace Crater.SemanticAnalysis.Types;

public class NullableType(Type innerType) : Type($"{innerType.Name}?")
{
    public readonly Type InnerType = innerType;

    public override bool CanHold(Type other)
    {
        if (other is NullableType otherNullable)
            return InnerType.CanHold(otherNullable.InnerType);

        return other is NilType || InnerType.CanHold(other);
    }
}
