namespace Crater.SemanticAnalysis.Types;

public class FunctionType(IReadOnlyList<Type> parameterTypes, Type? varargType, IReadOnlyList<Type> returnTypes, Type baseType) : Type(BuildSignature(parameterTypes, returnTypes), baseType)
{
    public readonly IReadOnlyList<Type> ParameterTypes = parameterTypes;
    public readonly IReadOnlyList<Type> ReturnTypes = returnTypes;

    public readonly Type? VarargType = varargType;

    public override bool CanHold(Type other)
    {
        if (BaseType == TypeRegistry.AnyType && other is FunctionType)
            return true;

        return IsSameType(other);
    }

    public override bool IsSameType(Type other)
    {
        if (other is not FunctionType otherFunction)
            return false;

        if (ParameterTypes.Count != otherFunction.ParameterTypes.Count)
            return false;

        if (ReturnTypes.Count != otherFunction.ReturnTypes.Count)
            return false;

        for (var i = 0; i < ParameterTypes.Count; i++)
        {
            if (!ParameterTypes[i].CanHold(otherFunction.ParameterTypes[i]))
                return false;
        }

        for (var i = 0; i < ReturnTypes.Count; i++)
        {
            if (!otherFunction.ReturnTypes[i].CanHold(ReturnTypes[i]))
                return false;
        }

        return true;
    }

    private static string BuildSignature(IReadOnlyList<Type> parameterTypes, IReadOnlyList<Type> returnTypes)
    {
        if (parameterTypes.Count == 0 && returnTypes.Count == 0)
            return "function";

        var parameters = string.Join(", ", parameterTypes);
        var returns = returnTypes.Count == 0 ? "void" : string.Join(", ", returnTypes);

        return $"fun({parameters}): {returns}";
    }
}
