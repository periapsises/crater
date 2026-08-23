using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Crater.Diagnostics;
using Crater.Diagnostics.Codes;
using Crater.SemanticAnalysis.Types;
using Crater.SyntaxTree;

namespace Crater.SemanticAnalysis;

public class SemanticAnalyzer
{
    private readonly IDiagnosticReporter _reporter;

    private readonly Environment _global;
    private Environment _local;

    private readonly Dictionary<string, Type> _types;

    public static readonly Type AnyType = new AnyType();
    public static readonly Type NumberType = new NumberType();
    public static readonly Type StringType = new StringType();
    public static readonly Type BooleanType = new BooleanType();
    public static readonly Type NilType = new NilType();
    public static readonly Type UnknownType = new UnknownType();

    public SemanticAnalyzer(IDiagnosticReporter reporter)
    {
        _reporter = reporter;

        _types = new Dictionary<string, Type>()
        {
            { "any", AnyType },
            { "number", NumberType },
            { "string", StringType },
            { "bool", BooleanType }
        };

        _global = new Environment();
        _local = new Environment(_global);
    }

    private void EnterScope() => _local = new Environment(_local);

    private void ExitScope() => _local = _local.Parent ?? throw new NullReferenceException("Cannot exit global scope");

    public void AnalyzeProgram(Program program)
    {
        AnalyzeBlock(program.block);
    }

    private bool AnalyzeBlock(Block block, List<Type>? expectedReturns = null)
    {
        var blocking = false;

        foreach (var statement in block.statements)
        {
            switch (statement)
            {
                case VariableDeclaration variableDeclaration:
                    AnalyzeVariableDeclaration(variableDeclaration);
                    break;
                case FunctionDeclaration functionDeclaration:
                    AnalyzeFunctionDeclaration(functionDeclaration);
                    break;
                case DoStatement doStatement:
                    AnalyzeDoStatement(doStatement);
                    break;
                case IfStatement ifStatement:
                    blocking |= AnalyzeIfStatement(ifStatement, expectedReturns);
                    break;
                case Assignment assignment:
                    AnalyzeAssignment(assignment);
                    break;
                case ReturnStatement returnStatement:
                    AnalyzeReturnStatement(returnStatement, expectedReturns);
                    blocking = true;
                    break;
                default:
                    throw new SwitchExpressionException(statement);
            }
        }

        return blocking;
    }

    private void AnalyzeVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        var env = variableDeclaration.local ? _local : _global;

        var initializerTypes = ExpandExpressionList(variableDeclaration.initializers);

        for (var i = 0; i < variableDeclaration.declarators.Count; i++)
        {
            var (name, type) = AnalyzeVariableDeclarator(env, variableDeclaration.declarators[i]);
            var initializer = initializerTypes.ElementAtOrDefault(i);

            if (initializer.Item1 is not null)
            {
                if (!type.CanHold(initializer.Item1))
                {
                    if (initializer.Item1 is NilType)
                        _reporter.Report(new Diagnostic(TypeErrors.NilAssignment, $"Cannot assign nil to '{name}' as it is declared with the non-nullable type '{type}'", DiagnosticSeverity.Error, variableDeclaration.source));
                    else
                        _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"The value assigned to '{name}' of type '{initializer.Item1}' is incompatible with the declared type of '{type}'", DiagnosticSeverity.Error, variableDeclaration.source));
                }
            }
            else
            {
                if (!variableDeclaration.local)
                    _reporter.Report(new Diagnostic(TypeErrors.UninitializedVariable, $"Global variable '{name}' must have an initializer", DiagnosticSeverity.Error, variableDeclaration.source));
                else if (!type.Nullable)
                    _reporter.Report(new Diagnostic(TypeErrors.UninitializedVariable, $"The variable '{name}' is not initialized but not marked as nullable", DiagnosticSeverity.Error, variableDeclaration.source));
            }

            env.Define(name, type);
        }
    }

    private (string, Type) AnalyzeVariableDeclarator(Environment env, VariableDeclarator variableDeclarator)
    {
        if (_local.GetType(variableDeclarator.name) != null)
            _reporter.Report(new Diagnostic(SemanticWarnings.VariableShadowing, $"Variable '{variableDeclarator.name}' shadows exiting binding", DiagnosticSeverity.Warning, variableDeclarator.source));

        var name = variableDeclarator.name;
        var type = AnalyzeTypeName(variableDeclarator.type);

        return (name, type);
    }

    private void AnalyzeFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var env = functionDeclaration.local ? _local : _global;

        if (_local.GetType(functionDeclaration.name) != null)
            _reporter.Report(new Diagnostic(SemanticWarnings.VariableShadowing, $"Function '{functionDeclaration.name}' shadows an existing binding", DiagnosticSeverity.Warning, functionDeclaration.source));

        var parameterTypes = new List<Type>();
        foreach (var parameter in functionDeclaration.parameters)
            parameterTypes.Add(AnalyzeTypeName(parameter.type));

        var returnTypes = new List<Type>();
        foreach (var returnType in functionDeclaration.returnTypes)
            returnTypes.Add(AnalyzeTypeName(returnType));

        env.Define(functionDeclaration.name, new FunctionType(parameterTypes, returnTypes));

        EnterScope();

        for (var i = 0; i < parameterTypes.Count; i++)
        {
            var name = functionDeclaration.parameters[i].name;

            if (_local.GetType(name) != null)
                _reporter.Report(new Diagnostic(SemanticWarnings.VariableShadowing, $"Parameter '{name}' shadows an existing binding", DiagnosticSeverity.Warning, functionDeclaration.source));

            _local.Define(name, parameterTypes[i]);
        }

        var blocked = AnalyzeBlock(functionDeclaration.block, returnTypes);
        if (!blocked && returnTypes.Count != 0)
            // TODO: Error code for when not all paths return
            _reporter.Report(new Diagnostic("0", $"Not all code paths return a value in function '{functionDeclaration.name}'", DiagnosticSeverity.Error, functionDeclaration.source));

        ExitScope();
    }

    private void AnalyzeDoStatement(DoStatement doStatement)
    {
        EnterScope();
        AnalyzeBlock(doStatement.block);
        ExitScope();
    }

    private bool AnalyzeIfStatement(IfStatement ifStatement, List<Type>? expectedReturns = null)
    {
        AnalyzeExpression(ifStatement.condition);

        EnterScope();
        var mainIsBlocking = AnalyzeBlock(ifStatement.block, expectedReturns);
        ExitScope();

        var allElseIfsBlocking = true;
        foreach (var elseIfStatement in ifStatement.elseIfStatements)
            allElseIfsBlocking &= AnalyzeElseIfStatement(elseIfStatement, expectedReturns);

        var elseIsBlocking = false;
        if (ifStatement.elseStatement is not null)
            elseIsBlocking = AnalyzeElseStatement(ifStatement.elseStatement, expectedReturns);

        return mainIsBlocking && allElseIfsBlocking && elseIsBlocking;
    }

    private bool AnalyzeElseIfStatement(ElseIfStatement elseIfStatement, List<Type>? expectedReturns = null)
    {
        AnalyzeExpression(elseIfStatement.condition);

        EnterScope();
        var blocking = AnalyzeBlock(elseIfStatement.block, expectedReturns);
        ExitScope();

        return blocking;
    }

    private bool AnalyzeElseStatement(ElseStatement elseStatement, List<Type>? expectedReturns = null)
    {
        EnterScope();
        var blocking = AnalyzeBlock(elseStatement.block, expectedReturns);
        ExitScope();

        return blocking;
    }

    private void AnalyzeAssignment(Assignment assignment)
    {
        var assignedTypes = ExpandExpressionList(assignment.values);

        for (var i = 0; i < assignment.variables.Count; i++)
        {
            var variable = assignment.variables[i];

            var variableType = _local.GetType(variable);
            if (variableType == null)
            {
                _reporter.Report(new Diagnostic(NameResolution.UndefinedVariable, $"Variable '{variable}' does not exist in the current context", DiagnosticSeverity.Error, assignment.source));
                return;
            }

            var valueType = assignedTypes.ElementAtOrDefault(i);
            if (valueType.Item1 == null)
            {
                if (!variableType.Nullable)
                    _reporter.Report(new Diagnostic(TypeErrors.NilAssignment, $"Cannot assign nil to non-nullable value '{variable}'", DiagnosticSeverity.Error, assignment.source));
            }
            else if (!variableType.CanHold(valueType.Item1))
            {
                _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Cannot assign value of type '{valueType.Item1}' to variable of type '{variableType}'", DiagnosticSeverity.Error, valueType.Item2));
            }
        }
    }

    private void AnalyzeReturnStatement(ReturnStatement returnStatement, List<Type>? expectedReturnTypes = null)
    {
        var returnTypes = ExpandExpressionList(returnStatement.returnValues);
        if (expectedReturnTypes == null)
            return;

        for (var i = 0; i < expectedReturnTypes.Count; i++)
        {
            var expectedType = expectedReturnTypes[i];

            if (i < returnTypes.Count)
            {
                if (expectedType.CanHold(returnTypes[i].Item1))
                    continue;

                _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Expected to return '{expectedType}' but got '{returnTypes[i].Item1}", DiagnosticSeverity.Error, returnTypes[i].Item2));
            }
            else if (!expectedType.Nullable)
            {
                // TODO: Determine if a custom code is needed
                _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Expected a '{expectedType}' value to be returned", DiagnosticSeverity.Error, returnStatement.source));
            }
        }
    }

    private Type AnalyzeTypeName(TypeName typeName)
    {
        var type = _types.GetValueOrDefault(typeName.name);
        if (type == null)
        {
            _reporter.Report(new Diagnostic(TypeErrors.UndefinedType, $"Could not find type '{typeName}'", DiagnosticSeverity.Error, typeName.source));
            type = UnknownType;
        }

        if (typeName.nullable)
            type = type.GetNullable();

        return type;
    }

    private List<Type> AnalyzeExpression(Expression expression)
    {
        return expression switch
        {
            UnaryOperation unaryOperation => AnalyzeUnaryOperation(unaryOperation),
            BinaryOperation binaryOperation => AnalyzeBinaryOperation(binaryOperation),
            Literal literal => AnalyzeLiteral(literal),
            VariableReference variableReference => AnalyzeVariableReference(variableReference),
            FunctionCall functionCall => AnalyzeFunctionCall(functionCall),
            _ => throw new SwitchExpressionException(expression)
        };
    }

    private List<Type> AnalyzeUnaryOperation(UnaryOperation unaryOperation)
    {
        var expressionType = AnalyzeExpression(unaryOperation.expression).FirstOrDefault() ?? NilType;
        var resultType = expressionType.ResolveUnaryOperation(unaryOperation.op);
        if (resultType != null)
            return [resultType];

        _reporter.Report(new Diagnostic(TypeErrors.UnsupportedUnaryOperation, $"Cannot perform unary operation '{unaryOperation.op}' on type '{expressionType}'", DiagnosticSeverity.Error, unaryOperation.source));
        return [UnknownType];
    }

    private List<Type> AnalyzeBinaryOperation(BinaryOperation binaryOperation)
    {
        if (IsTernaryPattern(binaryOperation, out var expressionA, out var expressionB))
        {
            AnalyzeExpression(binaryOperation.left);

            var typeA = AnalyzeExpression(expressionA).FirstOrDefault() ?? NilType;
            var typeB = AnalyzeExpression(expressionB).FirstOrDefault() ?? NilType;

            var ternaryResultType = Type.GetCommonType(typeA, typeB);
            if (ternaryResultType != null)
                return [ternaryResultType];

            _reporter.Report(new Diagnostic(TypeErrors.FailedTypeInference, $"Ternary results have incompatible types '{typeA}' and '{typeB}'", DiagnosticSeverity.Error, binaryOperation.source));
            return [UnknownType];
        }

        var leftType = AnalyzeExpression(binaryOperation.left).FirstOrDefault() ?? NilType;
        var rightType = AnalyzeExpression(binaryOperation.right).FirstOrDefault() ?? NilType;

        var resultType = leftType.ResolveBinaryOperation(binaryOperation.op, rightType);
        if (resultType != null)
            return [resultType];

        _reporter.Report(new Diagnostic(TypeErrors.UnsupportedBinaryOperation, $"Cannot perform binary operation '{binaryOperation.op}' on types '{leftType}' and '{rightType}'", DiagnosticSeverity.Error, binaryOperation.source));
        return [UnknownType];
    }

    private List<Type> AnalyzeLiteral(Literal literal)
    {
        return literal.kind switch
        {
            LiteralKind.Number => [NumberType],
            LiteralKind.String => [StringType],
            LiteralKind.Boolean => [BooleanType],
            LiteralKind.Nil => [NilType],
            _ => throw new SwitchExpressionException(literal.kind)
        };
    }

    private List<Type> AnalyzeVariableReference(VariableReference variableReference)
    {
        var type = _local.GetType(variableReference.name);
        if (type != null)
            return [type];

        _reporter.Report(new Diagnostic(NameResolution.UndefinedVariable, $"Variable '{variableReference.name}' does not exist in the current context", DiagnosticSeverity.Error, variableReference.source));
        return [UnknownType];
    }

    private List<Type> AnalyzeFunctionCall(FunctionCall functionCall)
    {
        var prefixType = AnalyzeExpression(functionCall.function).FirstOrDefault() ?? NilType;
        if (prefixType is not FunctionType functionType)
        {
            // TODO: Error code for call on a non function call
            _reporter.Report(new Diagnostic("0", $"Attempt to call a '{prefixType}' value", DiagnosticSeverity.Error, functionCall.source));
            return [UnknownType];
        }

        var argumentTypes = ExpandExpressionList(functionCall.arguments);

        for (var i = 0; i < functionType.ParameterTypes.Count; i++)
        {
            var parameterType = functionType.ParameterTypes[i];
            if (i >= argumentTypes.Count)
            {
                if (!parameterType.Nullable)
                    // TODO: Error code for missing argument
                    _reporter.Report(new Diagnostic("0", $"Missing argument #{i + 1}. Expecting '{parameterType}'", DiagnosticSeverity.Error, functionCall.source));
            }
            else
            {
                var argument = argumentTypes[i];
                if (!parameterType.CanHold(argument.Item1))
                    _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Argument #{i + 1} expected '{parameterType}' but got '{argument.Item1}'", DiagnosticSeverity.Error, argument.Item2));
            }
        }

        return functionType.ReturnTypes.ToList();
    }

    private bool IsTernaryPattern(BinaryOperation binaryOperation, [NotNullWhen(true)] out Expression? valueA, [NotNullWhen(true)] out Expression? valueB)
    {
        valueA = null;
        valueB = null;

        if (binaryOperation.op != "or")
            return false;

        if (binaryOperation.left is not BinaryOperation { op: "and" } leftBinaryOperation)
            return false;

        valueA = leftBinaryOperation.right;
        valueB = binaryOperation.right;
        return true;
    }

    private List<(Type, Source)> ExpandExpressionList(List<Expression> expressions)
    {
        var results = new List<(Type, Source)>();

        for (var i = 0; i < expressions.Count; i++)
        {
            var types = AnalyzeExpression(expressions[i]);
            if (i == expressions.Count - 1)
                results.AddRange(types.Select(t => (t, expressions[i].source)));
            else
                results.Add((types.ElementAtOrDefault(0) ?? NilType, expressions[i].source));
        }

        return results;
    }
}
