using Crater.SemanticAnalysis.Types;

namespace Crater.SemanticAnalysis;

public static class TypeRegistry
{
    public static readonly Type AnyType = new AnyType();
    public static readonly Type NumberType = new NumberType(AnyType);
    public static readonly Type StringType = new StringType(AnyType);
    public static readonly Type BooleanType = new BooleanType(AnyType);

    public static readonly Type FunctionType = new FunctionType([], null, [], AnyType);

    public static readonly Type NilType = new NilType();
    public static readonly Type UnknownType = new UnknownType();
}
