using Antlr4.Runtime;
using Crater.Antlr;
using Crater.SemanticAnalysis;

namespace Crater;

public static class Crater
{
    public static void Main(string[] args)
    {
        var inputStream = new AntlrInputStream("hello: world");
        var craterLexer = new CraterLexer(inputStream);
        var tokenStream = new CommonTokenStream(craterLexer);
        var craterParser = new CraterParser(tokenStream);

        var syntaxTreeConverter = new SyntaxTreeConverter();
        var node = syntaxTreeConverter.Visit(craterParser.program());

        if (node is not Program program)
            throw new Exception("Failed to convert resulting tree.");

        var semanticAnalyzer = new SemanticAnalyzer();
        semanticAnalyzer.AnalyzeProgram(program);
        
        Console.WriteLine(program);
    }
}
