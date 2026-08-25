namespace Crater.SemanticAnalysis.Types;

public class FunctionType(IReadOnlyList<Type> parameterTypes, IReadOnlyList<Type> returnTypes, Type baseType) : Type(BuildSignature(parameterTypes, returnTypes), baseType)
{
    public readonly IReadOnlyList<Type> ParameterTypes = parameterTypes;
    public readonly IReadOnlyList<Type> ReturnTypes = returnTypes;

    private static string BuildSignature(IReadOnlyList<Type> parameterTypes, IReadOnlyList<Type> returnTypes)
    {
        if (parameterTypes.Count == 0 && returnTypes.Count == 0)
            return "function";

        var parameters = string.Join(", ", parameterTypes);
        var returns = returnTypes.Count == 0 ? "void" : string.Join(", ", returnTypes);

        return $"fun({parameters}): {returns}";
    }
}
