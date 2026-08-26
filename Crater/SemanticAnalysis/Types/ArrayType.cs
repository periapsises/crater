namespace Crater.SemanticAnalysis.Types;

public class ArrayType : Type
{
    public readonly Type ElementType;
    private readonly Type _returnType;

    public ArrayType(Type elementType, Type baseType) : base($"{elementType}[]", baseType)
    {
        ElementType = elementType;
        _returnType = elementType is NullableType ? elementType : new NullableType(elementType);
    }

    public override bool CanHold(Type other)
    {
        return IsSameType(other);
    }

    public override bool IsSameType(Type other)
    {
        return other is ArrayType otherArray && ElementType.IsSameType(otherArray.ElementType);
    }

    public override Type? ResolveIndex(Type index)
    {
        if (index is NumberType)
            return _returnType;

        return null;
    }
}
