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

    public SemanticAnalyzer(IDiagnosticReporter reporter)
    {
        _reporter = reporter;

        _types = new Dictionary<string, Type>()
        {
            { "any", TypeRegistry.AnyType },
            { "number", TypeRegistry.NumberType },
            { "string", TypeRegistry.StringType },
            { "bool", TypeRegistry.BooleanType }
        };

        _global = new Environment();
        _local = new Environment(_global);

        Library.Load(_global);
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
                case FunctionCall functionCall:
                    AnalyzeFunctionCall(functionCall);
                    break;
                case Assignment assignment:
                    AnalyzeAssignment(assignment);
                    break;
                case WhileLoop whileLoop:
                    AnalyzeWhileLoop(whileLoop);
                    break;
                case RepeatLoop repeatLoop:
                    AnalyzeRepeatLoop(repeatLoop);
                    break;
                case NumericForLoop numericForLoop:
                    AnalyzeNumericForLoop(numericForLoop);
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
                else if (type is not NullableType)
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

        Type? varargType = null;
        if (functionDeclaration.varargParameter is not null)
            varargType = AnalyzeTypeName(functionDeclaration.varargParameter.type);

        var returnTypes = new List<Type>();
        foreach (var returnType in functionDeclaration.returnTypes)
            returnTypes.Add(AnalyzeTypeName(returnType));

        env.Define(functionDeclaration.name, new FunctionType(parameterTypes, varargType, returnTypes, TypeRegistry.FunctionType));

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
                if (variableType is not NullableType)
                    _reporter.Report(new Diagnostic(TypeErrors.NilAssignment, $"Cannot assign nil to non-nullable value '{variable}'", DiagnosticSeverity.Error, assignment.source));
            }
            else if (!variableType.CanHold(valueType.Item1))
            {
                _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Cannot assign value of type '{valueType.Item1}' to variable of type '{variableType}'", DiagnosticSeverity.Error, valueType.Item2));
            }
        }
    }

    private void AnalyzeWhileLoop(WhileLoop whileLoop)
    {
        AnalyzeExpression(whileLoop.condition);

        EnterScope();
        AnalyzeBlock(whileLoop.block);
        ExitScope();
    }

    private void AnalyzeRepeatLoop(RepeatLoop repeatLoop)
    {
        EnterScope();
        AnalyzeBlock(repeatLoop.block);
        AnalyzeExpression(repeatLoop.condition);
        ExitScope();
    }

    private void AnalyzeNumericForLoop(NumericForLoop numericForLoop)
    {
        var initializerType = AnalyzeExpression(numericForLoop.initializer).FirstOrDefault() ?? TypeRegistry.NilType;
        if (initializerType is not NumberType)
            _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Initializer for numeric for loop variable must be of type 'number' but got '{initializerType}'", DiagnosticSeverity.Error, numericForLoop.initializer.source));

        var limitType = AnalyzeExpression(numericForLoop.limit).FirstOrDefault() ?? TypeRegistry.NilType;
        if (limitType is not NumberType)
            _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Limit for numeric for loop must be of type 'number' but got '{limitType}'", DiagnosticSeverity.Error, numericForLoop.limit.source));

        if (numericForLoop.increment is not null)
        {
            var incrementType = AnalyzeExpression(numericForLoop.increment).FirstOrDefault() ?? TypeRegistry.NilType;
            if (incrementType is not NumberType)
                _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Increment for numeric for loop must be of type 'number' but got '{incrementType}'", DiagnosticSeverity.Error, numericForLoop.increment.source));
        }

        EnterScope();
        _local.Define(numericForLoop.variable, TypeRegistry.NumberType);
        AnalyzeBlock(numericForLoop.block);
        ExitScope();
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
            else if (expectedType is not NullableType)
            {
                // TODO: Determine if a custom code is needed
                _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Expected a '{expectedType}' value to be returned", DiagnosticSeverity.Error, returnStatement.source));
            }
        }
    }

    private Type AnalyzeTypeName(TypeName typeName)
    {
        return typeName switch
        {
            NullableTypeName nullableTypeName => AnalyzeNullableTypeName(nullableTypeName),
            ArrayTypeName arrayTypeName => AnalyzeArrayTypeName(arrayTypeName),
            NamedTypeName namedTypeName => AnalyzeNamedTypeName(namedTypeName),
            _ => throw new SwitchExpressionException(typeName)
        };
    }

    private Type AnalyzeNullableTypeName(NullableTypeName nullableTypeName)
    {
        var baseType = AnalyzeTypeName(nullableTypeName.baseTypeName);
        return new NullableType(baseType);
    }

    private Type AnalyzeArrayTypeName(ArrayTypeName arrayTypeName)
    {
        var baseType = AnalyzeTypeName(arrayTypeName.baseTypeName);
        return new ArrayType(baseType, TypeRegistry.AnyType);
    }

    private Type AnalyzeNamedTypeName(NamedTypeName namedTypeName)
    {
        var type = _types.GetValueOrDefault(namedTypeName.name);
        if (type != null)
            return type;

        _reporter.Report(new Diagnostic(TypeErrors.UndefinedType, $"Could not find type '{namedTypeName.name}'", DiagnosticSeverity.Error, namedTypeName.source));
        return TypeRegistry.UnknownType;
    }

    private List<Type> AnalyzeExpression(Expression expression)
    {
        return expression switch
        {
            UnaryOperation unaryOperation => AnalyzeUnaryOperation(unaryOperation),
            BinaryOperation binaryOperation => AnalyzeBinaryOperation(binaryOperation),
            NumberLiteral numberLiteral => AnalyzeNumberLiteral(numberLiteral),
            StringLiteral stringLiteral => AnalyzeStringLiteral(stringLiteral),
            BooleanLiteral booleanLiteral => AnalyzeBooleanLiteral(booleanLiteral),
            ArrayLiteral arrayLiteral => AnalyzeArrayLiteral(arrayLiteral),
            NilLiteral nilLiteral => AnalyzeNilLiteral(nilLiteral),
            VariableReference variableReference => AnalyzeVariableReference(variableReference),
            FunctionCall functionCall => AnalyzeFunctionCall(functionCall),
            BracketIndexing bracketIndexing => AnalyzeBracketIndexing(bracketIndexing),
            _ => throw new SwitchExpressionException(expression)
        };
    }

    private List<Type> AnalyzeUnaryOperation(UnaryOperation unaryOperation)
    {
        var expressionType = AnalyzeExpression(unaryOperation.expression).FirstOrDefault() ?? TypeRegistry.NilType;
        var resultType = expressionType.ResolveUnaryOperation(unaryOperation.op);
        if (resultType != null)
            return [resultType];

        _reporter.Report(new Diagnostic(TypeErrors.UnsupportedUnaryOperation, $"Cannot perform unary operation '{unaryOperation.op}' on type '{expressionType}'", DiagnosticSeverity.Error, unaryOperation.source));
        return [TypeRegistry.UnknownType];
    }

    private List<Type> AnalyzeBinaryOperation(BinaryOperation binaryOperation)
    {
        if (IsTernaryPattern(binaryOperation, out var expressionA, out var expressionB))
        {
            AnalyzeExpression(binaryOperation.left);

            var typeA = AnalyzeExpression(expressionA).FirstOrDefault() ?? TypeRegistry.NilType;
            var typeB = AnalyzeExpression(expressionB).FirstOrDefault() ?? TypeRegistry.NilType;

            var ternaryResultType = Type.GetCommonType(typeA, typeB);
            if (ternaryResultType != null)
                return [ternaryResultType];

            _reporter.Report(new Diagnostic(TypeErrors.FailedTypeInference, $"Ternary results have incompatible types '{typeA}' and '{typeB}'", DiagnosticSeverity.Error, binaryOperation.source));
            return [TypeRegistry.UnknownType];
        }

        var leftType = AnalyzeExpression(binaryOperation.left).FirstOrDefault() ?? TypeRegistry.NilType;
        var rightType = AnalyzeExpression(binaryOperation.right).FirstOrDefault() ?? TypeRegistry.NilType;

        var resultType = leftType.ResolveBinaryOperation(binaryOperation.op, rightType);
        if (resultType != null)
            return [resultType];

        _reporter.Report(new Diagnostic(TypeErrors.UnsupportedBinaryOperation, $"Cannot perform binary operation '{binaryOperation.op}' on types '{leftType}' and '{rightType}'", DiagnosticSeverity.Error, binaryOperation.source));
        return [TypeRegistry.UnknownType];
    }

    private List<Type> AnalyzeNumberLiteral(NumberLiteral numberLiteral)
    {
        return [TypeRegistry.NumberType];
    }

    private List<Type> AnalyzeStringLiteral(StringLiteral stringLiteral)
    {
        return [TypeRegistry.StringType];
    }

    private List<Type> AnalyzeBooleanLiteral(BooleanLiteral booleanLiteral)
    {
        return [TypeRegistry.BooleanType];
    }

    private List<Type> AnalyzeArrayLiteral(ArrayLiteral arrayLiteral)
    {
        Type? common = null;

        var values = ExpandExpressionList(arrayLiteral.values);
        foreach (var value in values)
        {
            if (common == null)
                common = value.Item1;
            else
                common = Type.GetCommonType(common, value.Item1);

            if (common == null)
            {
                _reporter.Report(new Diagnostic(TypeErrors.FailedTypeInference, $"Could not find a common type for array initializer", DiagnosticSeverity.Error, arrayLiteral.source));
                break;
            }
        }

        if (common == null)
            return [new EmptyArrayType()];

        return [new ArrayType(common, TypeRegistry.AnyType)];
    }

    private List<Type> AnalyzeNilLiteral(NilLiteral nilLiteral)
    {
        return [TypeRegistry.NilType];
    }

    private List<Type> AnalyzeVariableReference(VariableReference variableReference)
    {
        var type = _local.GetType(variableReference.name);
        if (type != null)
            return [type];

        _reporter.Report(new Diagnostic(NameResolution.UndefinedVariable, $"Variable '{variableReference.name}' does not exist in the current context", DiagnosticSeverity.Error, variableReference.source));
        return [TypeRegistry.UnknownType];
    }

    private List<Type> AnalyzeFunctionCall(FunctionCall functionCall)
    {
        var prefixType = AnalyzeExpression(functionCall.function).FirstOrDefault() ?? TypeRegistry.NilType;
        if (prefixType is not FunctionType functionType)
        {
            // TODO: Error code for call on a non function call
            _reporter.Report(new Diagnostic("0", $"Attempt to call a '{prefixType}' value", DiagnosticSeverity.Error, functionCall.source));
            return [TypeRegistry.UnknownType];
        }

        var argumentTypes = ExpandExpressionList(functionCall.arguments);

        for (var i = 0; i < functionType.ParameterTypes.Count; i++)
        {
            var parameterType = functionType.ParameterTypes[i];
            if (i >= argumentTypes.Count)
            {
                if (parameterType is not NullableType)
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

        if (functionType.VarargType == null)
            return functionType.ReturnTypes.ToList();

        for (var i = functionType.ParameterTypes.Count; i < argumentTypes.Count; i++)
        {
            if (!functionType.VarargType.CanHold(argumentTypes[i].Item1))
                _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Cannot pass value of type '{argumentTypes[i].Item1}' to vararg of type '{functionType.VarargType}'", DiagnosticSeverity.Error, argumentTypes[i].Item2));
        }

        return functionType.ReturnTypes.ToList();
    }

    private List<Type> AnalyzeBracketIndexing(BracketIndexing bracketIndexing)
    {
        var prefixType = AnalyzeExpression(bracketIndexing.prefix).FirstOrDefault() ?? TypeRegistry.NilType;
        var indexType = AnalyzeExpression(bracketIndexing.index).FirstOrDefault() ?? TypeRegistry.NilType;

        var resultType = prefixType.ResolveIndex(indexType);
        if (resultType != null)
            return [resultType];

        // TODO: Error code for indexing not supported
        _reporter.Report(new Diagnostic("0", $"Cannot perform indexing on '{prefixType}'", DiagnosticSeverity.Error, bracketIndexing.source));
        return [TypeRegistry.UnknownType];
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
                results.Add((types.ElementAtOrDefault(0) ?? TypeRegistry.NilType, expressions[i].source));
        }

        return results;
    }
}
