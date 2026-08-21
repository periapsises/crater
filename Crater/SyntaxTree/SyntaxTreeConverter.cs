using Antlr4.Runtime;
using Crater.Antlr;

namespace Crater.SyntaxTree;

public class SyntaxTreeConverter : CraterParserBaseVisitor<Node>
{
    private T Get<T>(ParserRuleContext context)
    {
        var node = Visit(context);
        if (node is T t)
            return t;

        throw new Exception($"Could not convert node to {typeof(T).Name} (was {node.GetType().Name})");
    }

    public override Node VisitProgram(CraterParser.ProgramContext context)
    {
        var block = Get<Block>(context.block());
        return new Program(block, Source.FromContext(context));
    }

    public override Node VisitBlock(CraterParser.BlockContext context)
    {
        var nodes = new List<Node>();

        foreach (var variableDeclaration in context.statement())
            nodes.Add(Visit(variableDeclaration));

        return new Block(nodes, Source.FromContext(context));
    }

    public override Node VisitVariableDeclaration(CraterParser.VariableDeclarationContext context)
    {
        var local = context.LOCAL() != null;
        var name = context.name.Text;
        var type = Get<TypeName>(context.typeName());

        Expression? initializer = null;
        if (context.expression() != null)
            initializer = Get<Expression>(context.expression());

        return new VariableDeclaration(local, name, type, initializer, Source.FromContext(context));
    }

    public override Node VisitDoStatement(CraterParser.DoStatementContext context)
    {
        var block = Get<Block>(context.block());
        return new DoStatement(block, Source.FromContext(context));
    }

    public override Node VisitAssignment(CraterParser.AssignmentContext context)
    {
        var variable = context.IDENTIFIER().GetText();
        var value = Get<Expression>(context.expression());

        return new Assignment(variable, value, Source.FromContext(context));
    }

    public override Node VisitTypeName(CraterParser.TypeNameContext context)
    {
        var name = context.IDENTIFIER().GetText();
        var nullable = context.QMARK() != null;

        return new TypeName(name, nullable, Source.FromContext(context));
    }

    public override Node VisitUnaryExpression(CraterParser.UnaryExpressionContext context)
    {
        var op = context.op.Text;
        var expression = Get<Expression>(context.expression());

        return new UnaryOperation(op, expression, Source.FromContext(context));
    }

    public override Node VisitMultiplicativeOperation(CraterParser.MultiplicativeOperationContext context)
    {
        var left = Get<Expression>(context.left);
        var right = Get<Expression>(context.right);
        var op = context.@operator.Text;

        return new BinaryOperation(left, op, right, Source.FromContext(context));
    }

    public override Node VisitAdditiveOperation(CraterParser.AdditiveOperationContext context)
    {
        var left = Get<Expression>(context.left);
        var right = Get<Expression>(context.right);
        var op = context.@operator.Text;

        return new BinaryOperation(left, op, right, Source.FromContext(context));
    }

    public override Node VisitNumberLiteral(CraterParser.NumberLiteralContext context)
    {
        return new Literal(context.GetText(), LiteralKind.Number, Source.FromContext(context));
    }

    public override Node VisitStringLiteral(CraterParser.StringLiteralContext context)
    {
        return new Literal(context.GetText(), LiteralKind.String, Source.FromContext(context));
    }

    public override Node VisitBooleanLiteral(CraterParser.BooleanLiteralContext context)
    {
        return new Literal(context.GetText(), LiteralKind.Boolean, Source.FromContext(context));
    }

    public override Node VisitNilLiteral(CraterParser.NilLiteralContext context)
    {
        return new Literal(context.GetText(), LiteralKind.Nil, Source.FromContext(context));
    }
}
