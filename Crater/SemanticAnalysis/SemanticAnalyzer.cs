using System.Runtime.CompilerServices;
using Crater.Diagnostics;
using Crater.SemanticAnalysis.Types;
using Crater.SyntaxTree;

namespace Crater.SemanticAnalysis;

public class SemanticAnalyzer
{
    private readonly IDiagnosticReporter _reporter;
    
    private readonly Environment _global;
    private Environment _local;

    private readonly Dictionary<string, Type> _types = [];

    private static readonly Type UnknownType = new UnknownType();

    public SemanticAnalyzer(IDiagnosticReporter reporter)
    {
        _reporter = reporter;
        
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
                default:
                    throw new SwitchExpressionException(statement);
            }
        }
    }

    private void AnalyzeVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        var env = variableDeclaration.local ? _global : _local;
        
        if (env.GetType(variableDeclaration.name) != null)
            // TODO: Proper error codes
            _reporter.Report(new Diagnostic("0", $"Variable {variableDeclaration.name} shadows exiting binding", DiagnosticSeverity.Warning, variableDeclaration.source));
        
        if (_types.TryGetValue(variableDeclaration.type, out var type))
            env.Define(variableDeclaration.name, type);
        
        // TODO: Proper error codes
        _reporter.Report(new Diagnostic("0", $"Could not find type '{variableDeclaration.type}'", DiagnosticSeverity.Error, variableDeclaration.source));
        env.Define(variableDeclaration.name, UnknownType);
    }

    private void AnalyzeDoStatement(DoStatement doStatement)
    {
        EnterScope();
        AnalyzeBlock(doStatement.block);
        ExitScope();
    }
}