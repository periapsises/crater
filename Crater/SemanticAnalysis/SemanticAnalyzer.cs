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

    private void AnalyzeBlock(Block block)
    {
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
                    AnalyzeIfStatement(ifStatement);
                    break;
                case Assignment assignment:
                    AnalyzeAssignment(assignment);
                    break;
                default:
                    throw new SwitchExpressionException(statement);
            }
        }
    }

    private void AnalyzeVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        var env = variableDeclaration.local ? _local : _global;

        for (var i = 0; i < variableDeclaration.declarators.Count; i++)
        {
            var (name, type) = AnalyzeVariableDeclarator(env, variableDeclaration.declarators[i]);
            var initializer = variableDeclaration.initializers.ElementAtOrDefault(i);

            if (initializer is not null)
            {
                var initializerType = AnalyzeExpression(initializer);
                if (!type.CanHold(initializerType))
                {
                    if (initializerType is NilType)
                        _reporter.Report(new Diagnostic(TypeErrors.NilAssignment, $"Cannot assign nil to '{name}' as it is declared with the non-nullable type '{type}'", DiagnosticSeverity.Error, variableDeclaration.source));
                    else
                        _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"The value assigned to '{name}' of type '{initializerType}' is incompatible with the declared type of '{type}'", DiagnosticSeverity.Error, variableDeclaration.source));
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

        AnalyzeBlock(functionDeclaration.block);

        // TODO: Verify that the block returns the expected values

        ExitScope();
    }

    private void AnalyzeDoStatement(DoStatement doStatement)
    {
        EnterScope();
        AnalyzeBlock(doStatement.block);
        ExitScope();
    }

    private void AnalyzeIfStatement(IfStatement ifStatement)
    {
        AnalyzeExpression(ifStatement.condition);

        EnterScope();
        AnalyzeBlock(ifStatement.block);
        ExitScope();

        foreach (var elseIfStatement in ifStatement.elseIfStatements)
            AnalyzeElseIfStatement(elseIfStatement);

        if (ifStatement.elseStatement is not null)
            AnalyzeElseStatement(ifStatement.elseStatement);
    }

    private void AnalyzeElseIfStatement(ElseIfStatement elseIfStatement)
    {
        AnalyzeExpression(elseIfStatement.condition);

        EnterScope();
        AnalyzeBlock(elseIfStatement.block);
        ExitScope();
    }

    private void AnalyzeElseStatement(ElseStatement elseStatement)
    {
        EnterScope();
        AnalyzeBlock(elseStatement.block);
        ExitScope();
    }

    private void AnalyzeAssignment(Assignment assignment)
    {
        var variableType = _local.GetType(assignment.variable);
        if (variableType == null)
        {
            _reporter.Report(new Diagnostic(NameResolution.UndefinedVariable, $"Variable '{assignment.variable}' does not exist in the current context", DiagnosticSeverity.Error, assignment.source));
            return;
        }

        var valueType = AnalyzeExpression(assignment.value);
        if (variableType.CanHold(valueType))
            return;

        _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Cannot assign value of type '{valueType}' to variable of type '{variableType}'", DiagnosticSeverity.Error, assignment.source));
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

    private Type AnalyzeExpression(Expression expression)
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

    private Type AnalyzeUnaryOperation(UnaryOperation unaryOperation)
    {
        var expressionType = AnalyzeExpression(unaryOperation.expression);
        var resultType = expressionType.ResolveUnaryOperation(unaryOperation.op);
        if (resultType != null)
            return resultType;

        _reporter.Report(new Diagnostic(TypeErrors.UnsupportedUnaryOperation, $"Cannot perform unary operation '{unaryOperation.op}' on type '{expressionType}'", DiagnosticSeverity.Error, unaryOperation.source));
        return UnknownType;
    }

    private Type AnalyzeBinaryOperation(BinaryOperation binaryOperation)
    {
        if (IsTernaryPattern(binaryOperation, out var expressionA, out var expressionB))
        {
            var typeA = AnalyzeExpression(expressionA);
            var typeB = AnalyzeExpression(expressionB);
            AnalyzeExpression(binaryOperation.left);


            var ternaryResultType = Type.GetCommonType(typeA, typeB);
            if (ternaryResultType != null)
                return ternaryResultType;

            _reporter.Report(new Diagnostic(TypeErrors.FailedTypeInference, $"Ternary results have incompatible types '{typeA}' and '{typeB}'", DiagnosticSeverity.Error, binaryOperation.source));
            return UnknownType;
        }

        var leftType = AnalyzeExpression(binaryOperation.left);
        var rightType = AnalyzeExpression(binaryOperation.right);

        var resultType = leftType.ResolveBinaryOperation(binaryOperation.op, rightType);
        if (resultType != null)
            return resultType;

        _reporter.Report(new Diagnostic(TypeErrors.UnsupportedBinaryOperation, $"Cannot perform binary operation '{binaryOperation.op}' on types '{leftType}' and '{rightType}'", DiagnosticSeverity.Error, binaryOperation.source));
        return UnknownType;
    }

    private Type AnalyzeLiteral(Literal literal)
    {
        return literal.kind switch
        {
            LiteralKind.Number => NumberType,
            LiteralKind.String => StringType,
            LiteralKind.Boolean => BooleanType,
            LiteralKind.Nil => NilType,
            _ => throw new SwitchExpressionException(literal.kind)
        };
    }

    private Type AnalyzeVariableReference(VariableReference variableReference)
    {
        var type = _local.GetType(variableReference.name);
        if (type != null)
            return type;

        _reporter.Report(new Diagnostic(NameResolution.UndefinedVariable, $"Variable '{variableReference.name}' does not exist in the current context", DiagnosticSeverity.Error, variableReference.source));
        return UnknownType;
    }

    private Type AnalyzeFunctionCall(FunctionCall functionCall)
    {
        var prefixType = AnalyzeExpression(functionCall.function);
        if (prefixType is not FunctionType functionType)
        {
            // TODO: Error code for call on a non function call
            _reporter.Report(new Diagnostic("0", $"Attempt to call a value of type '{prefixType}'", DiagnosticSeverity.Error, functionCall.source));
            return UnknownType;
        }

        for (var i = 0; i < functionType.ParameterTypes.Count; i++)
        {
            var parameterType = functionType.ParameterTypes[i];

            if (i >= functionCall.arguments.Count)
            {
                if (!parameterType.Nullable)
                    // TODO: Error code for missing argument
                    _reporter.Report(new Diagnostic("0", $"Missing argument #{i + 1}. Expecting '{parameterType}'", DiagnosticSeverity.Error, functionCall.source));
            }
            else
            {
                var argument = functionCall.arguments[i];
                var argumentType = AnalyzeExpression(argument);
                if (!parameterType.CanHold(argumentType))
                    _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Argument #{i + 1} expected '{parameterType}' but got '{argumentType}'", DiagnosticSeverity.Error, argument.source));
            }
        }

        // TODO: Figure out how to return multiple values or "nothing"
        return UnknownType;
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
}
