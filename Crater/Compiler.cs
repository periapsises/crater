using System.Text;
using Crater.SyntaxTree;

namespace Crater;

public class Compiler
{
    private readonly StringBuilder _builder;
    
    private Compiler()
    {
        _builder = new StringBuilder();
    }

    public static string Compile(Program program)
    {
        var compiler = new Compiler();
        compiler.CompileProgram(program);

        return compiler._builder.ToString();
    }

    private void CompileProgram(Program program)
    {
        foreach (var node in program.nodes)
        {
            switch (node)
            {
                case VariableDeclaration variableDeclaration:
                    CompileVariableDeclaration(variableDeclaration);
                    break;
            }
        }
    }

    private void CompileVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        _builder.AppendLine(variableDeclaration.name);
    }
}