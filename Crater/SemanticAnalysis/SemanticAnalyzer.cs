using Crater.SyntaxTree;

namespace Crater.SemanticAnalysis;

public class SemanticAnalyzer
{
    private readonly Dictionary<string, string> _localVariables = new();
    private readonly Dictionary<string, string> _globalVariables = new();
    
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
        var env = variableDeclaration.local ? _localVariables : _globalVariables;
        
        if (env.ContainsKey(variableDeclaration.name))
            throw new Exception($"{variableDeclaration.source.File}:{variableDeclaration.source.StartLine}:{variableDeclaration.source.StopColumn}\n  Variable '{variableDeclaration.name}' already exists");

        env[variableDeclaration.name] = variableDeclaration.type;
    }
}