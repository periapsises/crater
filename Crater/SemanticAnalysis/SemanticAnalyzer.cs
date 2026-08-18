using Crater.SyntaxTree;

namespace Crater.SemanticAnalysis;

public class SemanticAnalyzer
{
    private readonly Dictionary<string, string> _variables = new();
    
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
        if (_variables.ContainsKey(variableDeclaration.name))
            throw new Exception($"{variableDeclaration.source.File}:{variableDeclaration.source.StartLine}:{variableDeclaration.source.StopColumn}\n  Variable '{variableDeclaration.name}' already exists");

        _variables[variableDeclaration.name] = variableDeclaration.type;
    }
}