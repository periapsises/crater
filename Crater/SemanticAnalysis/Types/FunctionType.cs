namespace Crater.SemanticAnalysis.Types;

public class FunctionType(IReadOnlyList<Type> parameterTypes, IReadOnlyList<Type> returnTypes, bool nullable = false) : Type(BuildSignature(parameterTypes, returnTypes), null, nullable)
{
    public readonly IReadOnlyList<Type> ParameterTypes = parameterTypes;
    public readonly IReadOnlyList<Type> ReturnTypes = returnTypes;

    protected override bool IsSameTypeAs(Type other)
    {
        if (other is not FunctionType func)
            return false;

        if (ParameterTypes.Count != func.ParameterTypes.Count)
            return false;

        if (ReturnTypes.Count != func.ReturnTypes.Count)
            return false;

        for (var i = 0; i < ParameterTypes.Count; i++)
        {
            if (!ParameterTypes[i].CanHold(func.ParameterTypes[i]))
                return false;
        }

        for (var i = 0; i < ReturnTypes.Count; i++)
        {
            if (!func.ReturnTypes[i].CanHold(ReturnTypes[i]))
                return false;
        }

        return true;
    }

    public override Type GetNullable() => new FunctionType(ParameterTypes, ReturnTypes, true);
    public override Type GetNonNullable() => new FunctionType(ParameterTypes, ReturnTypes);

    private static string BuildSignature(IReadOnlyList<Type> parameterTypes, IReadOnlyList<Type> returnTypes)
    {
        var parameters = string.Join(", ", parameterTypes);
        var returns = returnTypes.Count == 0 ? "void" : string.Join(", ", returnTypes);

        return $"fun({parameters}): {returns}";
    }
}
