namespace Crater.SemanticAnalysis;

public class Environment(Environment? parent = null)
{
    public readonly Environment? Parent = parent;

    private readonly Dictionary<string, string> _symbolTypes = [];

    public void Define(string name, string type)
    {
        _symbolTypes[name] = type;
    }
    
    public string? GetType(string name)
    {
        if (_symbolTypes.TryGetValue(name, out var type))
            return type;

        return Parent?.GetType(name);
    }
}