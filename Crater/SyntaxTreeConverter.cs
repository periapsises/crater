using Crater.Antlr;

namespace Crater;

public class SyntaxTreeConverter : CraterParserBaseVisitor<object>
{
    public override object VisitProgram(CraterParser.ProgramContext context)
    {
        var variableDeclarations = new List<object>();
        
        foreach (var variableDeclaration in context.variableDeclaration())
            variableDeclarations.Add(Visit(variableDeclaration));

        return variableDeclarations;
    }

    public override object VisitVariableDeclaration(CraterParser.VariableDeclarationContext context)
    {
        var name = context.name.Text;
        var type = context.type.Text;

        return (name, type);
    }
}
