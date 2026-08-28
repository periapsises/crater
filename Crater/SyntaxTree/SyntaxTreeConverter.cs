using Antlr4.Runtime;
using Crater.Antlr;
using Crater.Diagnostics;

namespace Crater.SyntaxTree;

public class SyntaxTreeConverter(IDiagnosticReporter reporter) : CraterParserBaseVisitor<object>
{
    private readonly IDiagnosticReporter _reporter = reporter;

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

        VarargParameter? varargParameter = null;
        for (var i = 0; i < parameters.Count; i++)
        {
            if (parameters[i] is not VarargParameter varargParam)
                continue;

            if (i != parameters.Count - 1)
                _reporter.Report(new Diagnostic("0", $"Vararg parameter must be the last parameter in a function declaration", DiagnosticSeverity.Error, varargParam.source));

            varargParameter = varargParam;
        }

        parameters.RemoveAll(parameter => parameter is VarargParameter);

        var returnTypes = Get<List<TypeName>>(context.returnTypes());
        var block = Get<Block>(context.block());

        return new FunctionDeclaration(local, name, parameters, varargParameter, returnTypes, block, Source.FromContext(context));
    }

    public override object VisitParameters(CraterParser.ParametersContext context)
    {
        var parameters = new List<Parameter>();
        foreach (var parameterContext in context.parameter())
            parameters.Add(Get<Parameter>(parameterContext));

        return parameters;
    }

    public override object VisitNamedParameter(CraterParser.NamedParameterContext context)
    {
        var name = context.name.Text;
        var type = Get<TypeName>(context.typeName());

        return new Parameter(name, type, Source.FromContext(context));
    }

    public override object VisitVarargParameter(CraterParser.VarargParameterContext context)
    {
        var type = Get<TypeName>(context.typeName());
        return new VarargParameter(type, Source.FromContext(context));
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

    public override object VisitFunctionCall(CraterParser.FunctionCallContext context)
    {
        var function = Get<Expression>(context.expression());
        if (context.expressionList() == null)
            return new FunctionCall(function, [], Source.FromContext(context));

        var arguments = Get<List<Expression>>(context.expressionList());
        return new FunctionCall(function, arguments, Source.FromContext(context));
    }

    public override object VisitAssignment(CraterParser.AssignmentContext context)
    {
        var variables = new List<string>();
        foreach (var identifier in context.IDENTIFIER())
            variables.Add(identifier.GetText());

        var values = Get<List<Expression>>(context.expressionList());

        return new Assignment(variables, values, Source.FromContext(context));
    }

    public override object VisitWhileLoop(CraterParser.WhileLoopContext context)
    {
        var condition = Get<Expression>(context.condition);
        var block = Get<Block>(context.block());

        return new WhileLoop(condition, block, Source.FromContext(context));
    }

    public override object VisitRepeatLoop(CraterParser.RepeatLoopContext context)
    {
        var block = Get<Block>(context.block());
        var condition = Get<Expression>(context.condition);

        return new RepeatLoop(block, condition, Source.FromContext(context));
    }

    public override object VisitNumericForLoop(CraterParser.NumericForLoopContext context)
    {
        var variable = context.variable.Text;
        var initializer = Get<Expression>(context.initializer);
        var limit = Get<Expression>(context.limit);

        Expression? increment = null;
        if (context.increment != null)
            increment = Get<Expression>(context.increment);

        var block = Get<Block>(context.block());

        return new NumericForLoop(variable, initializer, limit, increment, block, Source.FromContext(context));
    }

    public override object VisitGenericForLoop(CraterParser.GenericForLoopContext context)
    {
        List<VariableDeclarator> declarators = [];
        foreach (var variableDeclaratorContext in context.variableDeclarator())
            declarators.Add(Get<VariableDeclarator>(variableDeclaratorContext));

        var expression = Get<Expression>(context.expression());
        var block = Get<Block>(context.block());

        return new GenericForLoop(declarators, expression, block, Source.FromContext(context));
    }

    public override object VisitReturnStatement(CraterParser.ReturnStatementContext context)
    {
        if (context.expressionList() == null)
            return new ReturnStatement([], Source.FromContext(context));

        var returnValues = Get<List<Expression>>(context.expressionList());
        return new ReturnStatement(returnValues, Source.FromContext(context));
    }

    public override object VisitTypeName(CraterParser.TypeNameContext context)
    {
        if (context.primaryType() != null)
            return Get<TypeName>(context.primaryType());

        var baseTypeName = Get<TypeName>(context.typeName());

        if (context.QMARK() != null)
            return new NullableTypeName(baseTypeName, Source.FromContext(context));

        if (context.LSQRBRACKET() != null)
            return new ArrayTypeName(baseTypeName, Source.FromContext(context));

        throw new Exception($"Unsupported decorated typename '{context.GetText()}'");
    }

    public override object VisitPrimaryType(CraterParser.PrimaryTypeContext context)
    {
        return new NamedTypeName(context.IDENTIFIER().GetText(), Source.FromContext(context));
    }

    public override object VisitExpressionList(CraterParser.ExpressionListContext context)
    {
        var expressions = new List<Expression>();
        foreach (var expressionContext in context.expression())
            expressions.Add(Get<Expression>(expressionContext));

        return expressions;
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

    public override object VisitConcatenationOperation(CraterParser.ConcatenationOperationContext context)
    {
        var left = Get<Expression>(context.left);
        var right = Get<Expression>(context.right);
        var op = context.CONCAT().GetText();

        return new BinaryOperation(left, op, right, Source.FromContext(context));
    }

    public override object VisitLogicalOperation(CraterParser.LogicalOperationContext context)
    {
        var left = Get<Expression>(context.left);
        var right = Get<Expression>(context.right);
        var op = context.logicalOperator().GetText();

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
        return new NumberLiteral(context.GetText(), Source.FromContext(context));
    }

    public override object VisitStringLiteral(CraterParser.StringLiteralContext context)
    {
        return new StringLiteral(context.GetText(), Source.FromContext(context));
    }

    public override object VisitBooleanLiteral(CraterParser.BooleanLiteralContext context)
    {
        return new BooleanLiteral(context.GetText(), Source.FromContext(context));
    }

    public override object VisitArrayLiteral(CraterParser.ArrayLiteralContext context)
    {
        if (context.expressionList() == null)
            return new ArrayLiteral([], Source.FromContext(context));

        var values = Get<List<Expression>>(context.expressionList());
        return new ArrayLiteral(values, Source.FromContext(context));
    }

    public override object VisitNilLiteral(CraterParser.NilLiteralContext context)
    {
        return new NilLiteral(context.GetText(), Source.FromContext(context));
    }

    public override object VisitPrimaryExpression(CraterParser.PrimaryExpressionContext context)
    {
        var expression = Get<Expression>(context.prefixExpression());
        foreach (var postfixExpressionContext in context.postfixExpression())
            expression = BuildPostfixExpression(postfixExpressionContext, expression);

        return expression;
    }

    public override object VisitVariableReference(CraterParser.VariableReferenceContext context)
    {
        var name = context.IDENTIFIER().GetText();
        return new VariableReference(name, Source.FromContext(context));
    }

    private Expression BuildPostfixExpression(CraterParser.PostfixExpressionContext context, Expression prefix)
    {
        if (context.postfixFunctionCall() is { } postfixFunctionCallContext)
            return BuildPostfixFunctionCall(postfixFunctionCallContext, prefix);

        if (context.postfixBracketIndexing() is { } postfixBracketIndexingContext)
            return BuildPostfixBracketIndexing(postfixBracketIndexingContext, prefix);

        throw new InvalidOperationException($"Unsupported postfix expression type: {context.GetText()}");
    }

    private FunctionCall BuildPostfixFunctionCall(CraterParser.PostfixFunctionCallContext context, Expression prefix)
    {
        if (context.expressionList() == null)
            return new FunctionCall(prefix, [], Source.FromContext(context));

        var arguments = Get<List<Expression>>(context.expressionList());
        return new FunctionCall(prefix, arguments, Source.FromContext(context));
    }

    private BracketIndexing BuildPostfixBracketIndexing(CraterParser.PostfixBracketIndexingContext context, Expression prefix)
    {
        var index = Get<Expression>(context.expression());
        return new BracketIndexing(prefix, index, Source.FromContext(context));
    }
}
