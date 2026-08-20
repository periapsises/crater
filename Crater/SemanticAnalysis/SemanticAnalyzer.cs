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

    private static readonly Type NumberType = new NumberType();
    private static readonly Type StringType = new StringType();
    private static readonly Type BooleanType = new BooleanType();
    private static readonly Type NilType = new NilType();
    private static readonly Type UnknownType = new UnknownType();

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
                case DoStatement doStatement:
                    AnalyzeDoStatement(doStatement);
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
        var env = variableDeclaration.local ? _global : _local;

        if (env.GetType(variableDeclaration.name) != null)
            _reporter.Report(new Diagnostic(SemanticWarnings.VariableShadowing, $"Variable {variableDeclaration.name} shadows exiting binding", DiagnosticSeverity.Warning, variableDeclaration.source));

        var type = _types.GetValueOrDefault(variableDeclaration.type.name);
        if (type == null)
        {
            _reporter.Report(new Diagnostic(TypeErrors.UndefinedType, $"Could not find type '{variableDeclaration.type}'", DiagnosticSeverity.Error, variableDeclaration.source));
            type = UnknownType;
        }

        if (variableDeclaration.type.nullable)
            type = new NullableType(type);

        if (variableDeclaration.initializer is not null)
        {
            var initializerType = AnalyzeExpression(variableDeclaration.initializer);
            if (!type.CanHold(initializerType))
            {
                if (initializerType is NilType)
                    _reporter.Report(new Diagnostic(TypeErrors.NilAssignment, $"Cannot assign nil to '{variableDeclaration.name}' as it is declared with the non-nullable type '{type.Name}'", DiagnosticSeverity.Error, variableDeclaration.source));
                else
                    _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"The value assigned to '{variableDeclaration.name}' of type '{initializerType.Name}' is incompatible with the declared type of '{type.Name}'", DiagnosticSeverity.Error, variableDeclaration.source));
            }
        }
        else
        {
            if (!variableDeclaration.local)
                _reporter.Report(new Diagnostic(TypeErrors.UninitializedVariable, $"Global variable '{variableDeclaration.name}' must have an initializer", DiagnosticSeverity.Error, variableDeclaration.source));
            else if (type is not NullableType)
                _reporter.Report(new Diagnostic(TypeErrors.UninitializedVariable, $"The variable '{variableDeclaration.name}' is not initialized but not marked as nullable", DiagnosticSeverity.Error, variableDeclaration.source));
        }

        env.Define(variableDeclaration.name, type);
    }

    private void AnalyzeDoStatement(DoStatement doStatement)
    {
        EnterScope();
        AnalyzeBlock(doStatement.block);
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

        _reporter.Report(new Diagnostic(TypeErrors.TypeMismatch, $"Cannot assign value of type '{valueType.Name}' to variable of type '{variableType.Name}'", DiagnosticSeverity.Error, assignment.source));
    }

    private Type AnalyzeExpression(Expression expression)
    {
        return expression switch
        {
            Literal literal => AnalyzeLiteral(literal),
            _ => throw new SwitchExpressionException(expression)
        };
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
}
