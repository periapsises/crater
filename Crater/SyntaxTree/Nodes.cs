namespace Crater.SyntaxTree;

public abstract record Node(Source source);

public sealed record Program(Block block, Source source) : Node(source);

public sealed record Block(List<Node> statements, Source source) : Node(source);

public sealed record VariableDeclaration(bool local, List<VariableDeclarator> declarators, List<Expression> initializers, Source source) : Node(source);

public sealed record VariableDeclarator(string name, TypeName type, Source source) : Node(source);

public sealed record DoStatement(Block block, Source source) : Node(source);

public sealed record Assignment(string variable, Expression value, Source source) : Node(source);

public sealed record TypeName(string name, bool nullable, Source source) : Node(source)
{
    public override string ToString()
    {
        if (nullable) return name + "?";
        return name;
    }
}

public abstract record Expression(Source source) : Node(source);

public sealed record BinaryOperation(Expression left, string op, Expression right, Source source) : Expression(source);

public sealed record UnaryOperation(string op, Expression expression, Source source) : Expression(source);

public enum LiteralKind
{
    Number,
    String,
    Boolean,
    Nil
}

public sealed record Literal(string value, LiteralKind kind, Source source) : Expression(source);
