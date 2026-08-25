using Crater.SemanticAnalysis;
using Crater.SemanticAnalysis.Types;
using Type = Crater.SemanticAnalysis.Type;

namespace Tests;

[TestFixture]
public class TypeSystemTests
{
    [Test]
    public void AnyCanHoldBuiltinTypes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(TypeRegistry.AnyType.CanHold(TypeRegistry.AnyType));
            Assert.That(TypeRegistry.AnyType.CanHold(TypeRegistry.NumberType));
            Assert.That(TypeRegistry.AnyType.CanHold(TypeRegistry.StringType));
            Assert.That(TypeRegistry.AnyType.CanHold(TypeRegistry.BooleanType));
            Assert.That(TypeRegistry.AnyType.CanHold(TypeRegistry.FunctionType));
        });
    }

    [Test]
    public void BuiltinTypesCannotHoldAny()
    {
        Assert.Multiple(() =>
        {
            Assert.That(!TypeRegistry.NumberType.CanHold(TypeRegistry.AnyType));
            Assert.That(!TypeRegistry.StringType.CanHold(TypeRegistry.AnyType));
            Assert.That(!TypeRegistry.BooleanType.CanHold(TypeRegistry.AnyType));
            Assert.That(!TypeRegistry.FunctionType.CanHold(TypeRegistry.AnyType));
        });
    }

    [Test]
    public void NullableCanHoldNil()
    {
        var nullable = new NullableType(TypeRegistry.AnyType);
        Assert.That(nullable.CanHold(TypeRegistry.NilType));
    }

    [Test]
    public void NullablesCanHoldNonNullables()
    {
        var nullable = new NullableType(TypeRegistry.AnyType);
        Assert.That(nullable.CanHold(TypeRegistry.AnyType));
    }

    [Test]
    public void NonNullablesCannotHoldNullables()
    {
        var nullableAny = new NullableType(TypeRegistry.AnyType);
        var nullableNumber = new NullableType(TypeRegistry.NumberType);
        var nullableString = new NullableType(TypeRegistry.StringType);
        var nullableBoolean = new NullableType(TypeRegistry.BooleanType);
        var nullableFunction = new NullableType(TypeRegistry.FunctionType);

        Assert.Multiple(() =>
        {
            Assert.That(!TypeRegistry.AnyType.CanHold(nullableAny));
            Assert.That(!TypeRegistry.NumberType.CanHold(nullableNumber));
            Assert.That(!TypeRegistry.StringType.CanHold(nullableString));
            Assert.That(!TypeRegistry.BooleanType.CanHold(nullableBoolean));
            Assert.That(!TypeRegistry.FunctionType.CanHold(nullableFunction));
        });
    }

    [Test]
    public void BaseFunctionCanHoldOtherFunctions()
    {
        var complexFunction = new FunctionType([TypeRegistry.AnyType], [], TypeRegistry.FunctionType);

        Assert.Multiple(() =>
        {
            Assert.That(TypeRegistry.FunctionType.CanHold(TypeRegistry.FunctionType));
            Assert.That(TypeRegistry.FunctionType.CanHold(complexFunction));
        });
    }

    [Test]
    public void ComplexFunctionsCannotHoldBaseFunctions()
    {
        var complexFunction = new FunctionType([TypeRegistry.AnyType], [], TypeRegistry.FunctionType);
        Assert.That(!complexFunction.CanHold(TypeRegistry.FunctionType));
    }

    [Test]
    public void ComplexFunctionsCanHoldSameSignature()
    {
        var complexFunctionA = new FunctionType([TypeRegistry.NumberType], [TypeRegistry.StringType], TypeRegistry.FunctionType);
        var complexFunctionB = new FunctionType([TypeRegistry.NumberType], [TypeRegistry.StringType], TypeRegistry.FunctionType);

        Assert.That(complexFunctionA.CanHold(complexFunctionB));
    }

    [Test]
    public void BuiltinTypesHaveAnyAsCommonBase()
    {
        var common = Type.GetCommonType(TypeRegistry.NumberType, TypeRegistry.StringType);
        Assert.That(common, Is.EqualTo(TypeRegistry.AnyType));
    }
}
