namespace Crater.SyntaxTree;

public abstract record Node(Source source);

public sealed record Program(Block block, Source source) : Node(source);

public sealed record Block(List<Node> statements, Source source) : Node(source);

public sealed record VariableDeclaration(bool local, string name, TypeName type, Expression? initializer, Source source) : Node(source);

public sealed record DoStatement(Block block, Source source) : Node(source);

public sealed record TypeName(string name, bool nullable, Source source) : Node(source);

public abstract record Expression(Source source) : Node(source);

public enum LiteralKind
{
    Number,
    String,
    Boolean,
    Nil
}

public sealed record Literal(string value, LiteralKind kind, Source source) : Expression(source);
