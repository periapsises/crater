namespace Crater.SemanticAnalysis;

public class Environment
{
    public readonly Environment? Parent;

    public Environment(Environment? parent = null)
    {
        Parent = parent;

        if (parent == null)
            Library.Load(this);
    }

    private readonly Dictionary<string, Type> _symbolTypes = [];

    public void Define(string name, Type type)
    {
        _symbolTypes[name] = type;
    }

    public Type? GetType(string name)
    {
        if (_symbolTypes.TryGetValue(name, out var type))
            return type;

        return Parent?.GetType(name);
    }
}
