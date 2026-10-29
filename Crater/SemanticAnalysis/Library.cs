using Crater.SemanticAnalysis.Types;

namespace Crater.SemanticAnalysis;

public class RequireFunction() : FunctionType([TypeRegistry.StringType], null, [TypeRegistry.AnyType], TypeRegistry.FunctionType);

public static class Library
{
    public static void Load(Environment global)
    {
        global.Define("print", new FunctionType([], new NullableType(TypeRegistry.AnyType), [], TypeRegistry.FunctionType));
        global.Define("require", new RequireFunction());
    }
}
