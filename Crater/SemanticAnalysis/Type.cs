namespace Crater.SemanticAnalysis;

public abstract class Type(string name)
{
    public readonly string Name = name;

    public abstract bool CanHold(Type other);
}
