using Crater.SemanticAnalysis.Types;

namespace Crater.SemanticAnalysis;

public static class Library
{
    public static void Load(Environment global)
    {
        global.Define("print", new FunctionType([], new NullableType(TypeRegistry.AnyType), [], TypeRegistry.FunctionType));
    }
}
