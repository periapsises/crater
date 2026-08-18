using Crater.Antlr;

namespace Crater;

public class SyntaxTreeConverter : CraterParserBaseVisitor<Node>
{
    public override Node VisitProgram(CraterParser.ProgramContext context)
    {
        var variableDeclarations = new List<Node>();
        
        foreach (var variableDeclaration in context.variableDeclaration())
            variableDeclarations.Add(Visit(variableDeclaration));

        return new Program(variableDeclarations, Source.FromContext(context));
    }

    public override Node VisitVariableDeclaration(CraterParser.VariableDeclarationContext context)
    {
        var name = context.name.Text;
        var type = context.type.Text;

        return new VariableDeclaration(name, type, Source.FromContext(context));
    }
}
