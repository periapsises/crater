using Antlr4.Runtime;
using Crater.Antlr;

namespace Crater.SyntaxTree;

public class SyntaxTreeConverter : CraterParserBaseVisitor<object>
{
    private T Get<T>(ParserRuleContext context)
    {
        var node = Visit(context);
        if (node is T t)
            return t;

        throw new Exception($"Could not convert node to {typeof(T).Name} (was {node.GetType().Name})");
    }

    public override object VisitProgram(CraterParser.ProgramContext context)
    {
        var block = Get<Block>(context.block());
        return new Program(block, Source.FromContext(context));
    }

    public override object VisitBlock(CraterParser.BlockContext context)
    {
        var nodes = new List<Node>();

        foreach (var variableDeclaration in context.statement())
            nodes.Add(Get<Node>(variableDeclaration));

        return new Block(nodes, Source.FromContext(context));
    }

    public override object VisitVariableDeclaration(CraterParser.VariableDeclarationContext context)
    {
        var local = context.LOCAL() != null;
        List<VariableDeclarator> declarators = [];
        List<Expression> initializers;

        foreach (var variableDeclaratorContext in context.variableDeclarator())
            declarators.Add(Get<VariableDeclarator>(variableDeclaratorContext));

        if (context.expressionList() != null)
            initializers = Get<List<Expression>>(context.expressionList());
        else
            initializers = [];

        return new VariableDeclaration(local, declarators, initializers, Source.FromContext(context));
    }

    public override object VisitVariableDeclarator(CraterParser.VariableDeclaratorContext context)
    {
        var name = context.name.Text;
        var type = Get<TypeName>(context.typeName());

        return new VariableDeclarator(name, type, Source.FromContext(context));
    }

    public override object VisitFunctionDeclaration(CraterParser.FunctionDeclarationContext context)
    {
        var local = context.LOCAL() != null;
        var name = context.name.Text;

        List<Parameter> parameters;
        if (context.parameters() != null)
            parameters = Get<List<Parameter>>(context.parameters());
        else
            parameters = [];

        var returnTypes = Get<List<TypeName>>(context.returnTypes());
        var block = Get<Block>(context.block());

        return new FunctionDeclaration(local, name, parameters, returnTypes, block, Source.FromContext(context));
    }

    public override object VisitParameters(CraterParser.ParametersContext context)
    {
        var parameters = new List<Parameter>();
        foreach (var parameterContext in context.parameter())
            parameters.Add(Get<Parameter>(parameterContext));

        return parameters;
    }

    public override object VisitParameter(CraterParser.ParameterContext context)
    {
        var name = context.name.Text;
        var type = Get<TypeName>(context.typeName());

        return new Parameter(name, type, Source.FromContext(context));
    }

    public override object VisitReturnTypes(CraterParser.ReturnTypesContext context)
    {
        var returnTypes = new List<TypeName>();

        if (context.VOID() != null)
            return returnTypes;

        foreach (var returnTypeContext in context.typeName())
            returnTypes.Add(Get<TypeName>(returnTypeContext));

        return returnTypes;
    }

    public override object VisitDoStatement(CraterParser.DoStatementContext context)
    {
        var block = Get<Block>(context.block());
        return new DoStatement(block, Source.FromContext(context));
    }

    public override object VisitIfStatement(CraterParser.IfStatementContext context)
    {
        var condition = Get<Expression>(context.expression());
        var block = Get<Block>(context.block());

        var elseIfStatements = new List<ElseIfStatement>();
        foreach (var elseIfStatementContext in context.elseIfStatement())
            elseIfStatements.Add(Get<ElseIfStatement>(elseIfStatementContext));

        ElseStatement? elseStatement = null;
        if (context.elseStatement() != null)
            elseStatement = Get<ElseStatement>(context.elseStatement());

        return new IfStatement(condition, block, elseIfStatements, elseStatement, Source.FromContext(context));
    }

    public override object VisitElseIfStatement(CraterParser.ElseIfStatementContext context)
    {
        var condition = Get<Expression>(context.expression());
        var block = Get<Block>(context.block());

        return new ElseIfStatement(condition, block, Source.FromContext(context));
    }

    public override object VisitElseStatement(CraterParser.ElseStatementContext context)
    {
        var block = Get<Block>(context.block());
        return new ElseStatement(block, Source.FromContext(context));
    }

    public override object VisitAssignment(CraterParser.AssignmentContext context)
    {
        var variable = context.IDENTIFIER().GetText();
        var value = Get<Expression>(context.expression());

        return new Assignment(variable, value, Source.FromContext(context));
    }

    public override object VisitTypeName(CraterParser.TypeNameContext context)
    {
        var name = context.IDENTIFIER().GetText();
        var nullable = context.QMARK() != null;

        return new TypeName(name, nullable, Source.FromContext(context));
    }

    public override object VisitExpressionList(CraterParser.ExpressionListContext context)
    {
        var expressions = new List<Expression>();
        foreach (var expressionContext in context.expression())
            expressions.Add(Get<Expression>(expressionContext));

        return expressions;
    }

    public override object VisitVariableReference(CraterParser.VariableReferenceContext context)
    {
        var name = context.IDENTIFIER().GetText();
        return new VariableReference(name, Source.FromContext(context));
    }

    public override object VisitUnaryExpression(CraterParser.UnaryExpressionContext context)
    {
        var op = context.op.Text;
        var expression = Get<Expression>(context.expression());

        return new UnaryOperation(op, expression, Source.FromContext(context));
    }

    public override object VisitMultiplicativeOperation(CraterParser.MultiplicativeOperationContext context)
    {
        var left = Get<Expression>(context.left);
        var right = Get<Expression>(context.right);
        var op = context.@operator.Text;

        return new BinaryOperation(left, op, right, Source.FromContext(context));
    }

    public override object VisitAdditiveOperation(CraterParser.AdditiveOperationContext context)
    {
        var left = Get<Expression>(context.left);
        var right = Get<Expression>(context.right);
        var op = context.@operator.Text;

        return new BinaryOperation(left, op, right, Source.FromContext(context));
    }

    public override object VisitAndOperation(CraterParser.AndOperationContext context)
    {
        var left = Get<Expression>(context.left);
        var right = Get<Expression>(context.right);
        var op = context.AND().GetText();

        return new BinaryOperation(left, op, right, Source.FromContext(context));
    }

    public override object VisitOrOperation(CraterParser.OrOperationContext context)
    {
        var left = Get<Expression>(context.left);
        var right = Get<Expression>(context.right);
        var op = context.OR().GetText();

        return new BinaryOperation(left, op, right, Source.FromContext(context));
    }

    public override object VisitNumberLiteral(CraterParser.NumberLiteralContext context)
    {
        return new Literal(context.GetText(), LiteralKind.Number, Source.FromContext(context));
    }

    public override object VisitStringLiteral(CraterParser.StringLiteralContext context)
    {
        return new Literal(context.GetText(), LiteralKind.String, Source.FromContext(context));
    }

    public override object VisitBooleanLiteral(CraterParser.BooleanLiteralContext context)
    {
        return new Literal(context.GetText(), LiteralKind.Boolean, Source.FromContext(context));
    }

    public override object VisitNilLiteral(CraterParser.NilLiteralContext context)
    {
        return new Literal(context.GetText(), LiteralKind.Nil, Source.FromContext(context));
    }
}
