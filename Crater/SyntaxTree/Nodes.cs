namespace Crater.SyntaxTree;

public abstract record Node(Source source);

public sealed record Program(Block block, Source source) : Node(source);

public sealed record Block(List<Node> statements, Source source) : Node(source);

public sealed record VariableDeclaration(bool local, string name, string type, Source source) : Node(source);

public sealed record DoStatement(Block block, Source source) : Node(source);