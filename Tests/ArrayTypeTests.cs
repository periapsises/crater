using Crater.SemanticAnalysis;
using Crater.SemanticAnalysis.Types;
using Type = System.Type;

namespace Tests;

[TestFixture]
public class ArrayTypeTests
{
    [Test]
    public void CanHoldArrayOfSameType()
    {
        var arrayA = new ArrayType(TypeRegistry.StringType, TypeRegistry.AnyType);
        var arrayB = new ArrayType(TypeRegistry.StringType, TypeRegistry.AnyType);

        Assert.That(arrayA.CanHold(arrayB));
    }

    [Test]
    public void IsInvariant()
    {
        var anyArray = new ArrayType(TypeRegistry.AnyType, TypeRegistry.AnyType);
        var numberArray = new ArrayType(TypeRegistry.NumberType, TypeRegistry.AnyType);
        var nullableNumberArray = new ArrayType(new NullableType(TypeRegistry.NumberType), TypeRegistry.AnyType);
        var nullableStringArray = new ArrayType(new NullableType(TypeRegistry.StringType), TypeRegistry.AnyType);

        var nullableNumberArray2 = new ArrayType(new NullableType(TypeRegistry.NumberType), TypeRegistry.AnyType);

        Assert.Multiple(() =>
        {
            Assert.That(anyArray.CanHold(numberArray), Is.False);
            Assert.That(nullableNumberArray.CanHold(numberArray), Is.False);
            Assert.That(nullableNumberArray.CanHold(nullableStringArray), Is.False);

            Assert.That(nullableNumberArray.CanHold(nullableNumberArray2));
        });
    }

    [Test]
    public void CannotIndexNonArrayTypes()
    {
        Assert.That(TypeRegistry.NumberType.ResolveIndex(TypeRegistry.NumberType), Is.Null);
    }
}
