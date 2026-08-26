namespace Crater.SyntaxTree;

public abstract record Node(Source source);

public sealed record Program(Block block, Source source) : Node(source);

public sealed record Block(List<Node> statements, Source source) : Node(source);

public sealed record VariableDeclaration(bool local, List<VariableDeclarator> declarators, List<Expression> initializers, Source source) : Node(source);

public sealed record VariableDeclarator(string name, TypeName type, Source source) : Node(source);

public sealed record FunctionDeclaration(bool local, string name, List<Parameter> parameters, List<TypeName> returnTypes, Block block, Source source) : Node(source);

public sealed record Parameter(string name, TypeName type, Source source) : Node(source);

public sealed record DoStatement(Block block, Source source) : Node(source);

public sealed record IfStatement(Expression condition, Block block, List<ElseIfStatement> elseIfStatements, ElseStatement? elseStatement, Source source) : Node(source);

public sealed record ElseIfStatement(Expression condition, Block block, Source source) : Node(source);

public sealed record ElseStatement(Block block, Source source) : Node(source);

public sealed record Assignment(List<string> variables, List<Expression> values, Source source) : Node(source);

public sealed record ReturnStatement(List<Expression> returnValues, Source source) : Node(source);

public abstract record TypeName(Source source) : Node(source);

public sealed record NamedTypeName(string name, Source source) : TypeName(source);

public sealed record NullableTypeName(TypeName baseTypeName, Source source) : TypeName(source);

public sealed record ArrayTypeName(TypeName baseTypeName, Source source) : TypeName(source);

public abstract record Expression(Source source) : Node(source);

public sealed record VariableReference(string name, Source source) : Expression(source);

public sealed record FunctionCall(Expression function, List<Expression> arguments, Source source) : Expression(source);

public sealed record BracketIndexing(Expression prefix, Expression index, Source source) : Expression(source);

public sealed record UnaryOperation(string op, Expression expression, Source source) : Expression(source);

public sealed record BinaryOperation(Expression left, string op, Expression right, Source source) : Expression(source);

public enum LiteralKind
{
    Number,
    String,
    Boolean,
    Nil
}

public sealed record Literal(string value, LiteralKind kind, Source source) : Expression(source);
