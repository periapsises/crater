using System.Runtime.CompilerServices;
using Crater.SyntaxTree;

namespace Crater.SemanticAnalysis;

public class SemanticAnalyzer
{
    private readonly Environment _global;
    private Environment _local;

    public SemanticAnalyzer()
    {
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
            throw new Exception($"{variableDeclaration.source.File}:{variableDeclaration.source.StartLine}:{variableDeclaration.source.StopColumn}\n  Variable '{variableDeclaration.name}' already exists");

        env.Define(variableDeclaration.name, variableDeclaration.type);
    }

    private void AnalyzeDoStatement(DoStatement doStatement)
    {
        EnterScope();
        AnalyzeBlock(doStatement.block);
        ExitScope();
    }
}