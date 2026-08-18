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
    
    public void AnalyzeProgram(Program program)
    {
        foreach (var node in program.nodes)
        {
            switch (node)
            {
                case VariableDeclaration variableDeclaration:
                    AnalyzeVariableDeclaration(variableDeclaration);
                    break;
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
}