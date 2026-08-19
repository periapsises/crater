using Antlr4.Runtime;
using Crater.Antlr;

namespace Crater.SyntaxTree;

public class SyntaxTreeConverter : CraterParserBaseVisitor<Node>
{
    private T Get<T>(ParserRuleContext context)
    {
        var node = Visit(context);
        if (node is T t)
            return t;

        throw new Exception($"Could not convert node to {typeof(T).Name} (was {node.GetType().Name})");
    }
    
    public override Node VisitProgram(CraterParser.ProgramContext context)
    {
        var block = Get<Block>(context.block());
        return new Program(block, Source.FromContext(context));
    }

    public override Node VisitBlock(CraterParser.BlockContext context)
    {
        var nodes = new List<Node>();
        
        foreach (var variableDeclaration in context.statement())
            nodes.Add(Visit(variableDeclaration));

        return new Block(nodes, Source.FromContext(context));
    }

    public override Node VisitVariableDeclaration(CraterParser.VariableDeclarationContext context)
    {
        var local = context.LOCAL() != null;
        var name = context.name.Text;
        var type = context.type.Text;

        return new VariableDeclaration(local, name, type, Source.FromContext(context));
    }
    
    public override Node VisitDoStatement(CraterParser.DoStatementContext context)
    {
        var block = Get<Block>(context.block());
        return new DoStatement(block, Source.FromContext(context));
    }
}
