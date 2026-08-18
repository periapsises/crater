using Crater.Antlr;

namespace Crater.SyntaxTree;

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
        var local = context.LOCAL() != null;
        var name = context.name.Text;
        var type = context.type.Text;

        return new VariableDeclaration(local, name, type, Source.FromContext(context));
    }
}
