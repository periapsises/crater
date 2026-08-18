namespace Crater.SyntaxTree;

public abstract record Node(Source source);

public sealed record Program(List<Node> nodes, Source source) : Node(source);

public sealed record VariableDeclaration(string name, string type, Source source) : Node(source);