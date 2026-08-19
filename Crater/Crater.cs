using Antlr4.Runtime;
using Crater.Antlr;
using Crater.Compilation;
using Crater.SemanticAnalysis;
using Crater.SyntaxTree;

namespace Crater;

public static class Crater
{
    public static void Main(string[] args)
    {
        const string source = """
                              hello: world
                              world: hello
                              local hello: hi
                              
                              do
                                  local hi: what
                              end
                              """;
        
        var inputStream = new AntlrInputStream(source)
        {
            name = "Unknown"
        };
        
        var craterLexer = new CraterLexer(inputStream);
        var tokenStream = new CommonTokenStream(craterLexer);
        var craterParser = new CraterParser(tokenStream);

        var syntaxTreeConverter = new SyntaxTreeConverter();
        var node = syntaxTreeConverter.Visit(craterParser.program());

        if (node is not Program program)
            throw new Exception("Failed to convert resulting tree.");

        var semanticAnalyzer = new SemanticAnalyzer();
        semanticAnalyzer.AnalyzeProgram(program);

        var output = Compiler.Compile(program);
        Console.WriteLine(output);
    }
}
