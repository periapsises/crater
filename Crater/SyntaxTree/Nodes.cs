namespace Crater.SyntaxTree;

public abstract record Node(Source source);

public sealed record Program(Block block, Source source) : Node(source);

public sealed record Block(List<Node> statements, Source source) : Node(source);

public sealed record VariableDeclaration(bool local, List<VariableDeclarator> declarators, List<Expression> initializers, Source source) : Node(source);

public sealed record VariableDeclarator(string name, TypeName type, Source source) : Node(source);

public sealed record FunctionDeclaration(bool local, string name, List<Parameter> parameters, VarargParameter? varargParameter, List<TypeName> returnTypes, Block block, Source source) : Node(source);

public record Parameter(string name, TypeName type, Source source) : Node(source);

public sealed record VarargParameter(TypeName type, Source source) : Parameter("...", type, source);

public sealed record DoStatement(Block block, Source source) : Node(source);

public sealed record IfStatement(Expression condition, Block block, List<ElseIfStatement> elseIfStatements, ElseStatement? elseStatement, Source source) : Node(source);

public sealed record ElseIfStatement(Expression condition, Block block, Source source) : Node(source);

public sealed record ElseStatement(Block block, Source source) : Node(source);

public sealed record Assignment(List<StorageType> variables, List<Expression> values, Source source) : Node(source);

public abstract record StorageType(Source source) : Node(source);

public sealed record ArrayStorage(StorageType prefix, Expression index, Source source) : StorageType(source);

public sealed record MemberStorage(StorageType prefix, string key, Source source) : StorageType(source);

public sealed record VariableStorage(string name, Source source) : StorageType(source);

public sealed record WhileLoop(Expression condition, Block block, Source source) : Node(source);

public sealed record RepeatLoop(Block block, Expression condition, Source source) : Node(source);

public sealed record NumericForLoop(string variable, Expression initializer, Expression limit, Expression? increment, Block block, Source source) : Node(source);

public sealed record GenericForLoop(List<VariableDeclarator> declarators, Expression expression, Block block, Source source) : Node(source);

public sealed record ReturnStatement(List<Expression> returnValues, Source source) : Node(source);

public sealed record BreakStatement(Source source) : Node(source);

public abstract record TypeName(Source source) : Node(source);

public sealed record NamedTypeName(string name, Source source) : TypeName(source);

public sealed record FunctionTypeName(List<TypeName> parameters, bool vararg, List<TypeName> returns, Source source) : TypeName(source);

public sealed record NullableTypeName(TypeName baseTypeName, Source source) : TypeName(source);

public sealed record ArrayTypeName(TypeName baseTypeName, Source source) : TypeName(source);

public sealed record TableTypeName(List<VariableDeclarator> fields, Source source) : TypeName(source);

public abstract record Expression(Source source) : Node(source);

public sealed record ParenthesizedExpression(Expression innerExpression, Source source) : Expression(source);

public sealed record VariableReference(string name, Source source) : Expression(source);

public sealed record FunctionCall(Expression function, List<Expression> arguments, Source source) : Expression(source);

public sealed record BracketIndexing(Expression prefix, Expression index, Source source) : Expression(source);

public sealed record DotIndexing(Expression prefix, string key, Source source) : Expression(source);

public sealed record UnaryOperation(string op, Expression expression, Source source) : Expression(source);

public sealed record BinaryOperation(Expression left, string op, Expression right, Source source) : Expression(source);

public sealed record NumberLiteral(string value, Source source) : Expression(source);

public sealed record StringLiteral(string value, Source source) : Expression(source);

public sealed record BooleanLiteral(string value, Source source) : Expression(source);

public sealed record TableLiteral(List<TableValue> values, Source source) : Expression(source);

public sealed record ArrayLiteral(List<Expression> values, Source source) : Expression(source);

public sealed record TableValue(string index, Expression value, Source source) : Node(source);

public sealed record NilLiteral(string value, Source source) : Expression(source);
