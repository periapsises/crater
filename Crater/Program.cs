using Antlr4.Runtime;
using Crater.Antlr;

namespace Crater;

public static class Program
{
    public static void Main(string[] args)
    {
        var inputStream = new AntlrInputStream("hello: world");
        var craterLexer = new CraterLexer(inputStream);
        var tokenStream = new CommonTokenStream(craterLexer);
        var craterParser = new CraterParser(tokenStream);

        var syntaxTreeConverter = new SyntaxTreeConverter();
        var result = syntaxTreeConverter.Visit(craterParser.program());
        
        Console.WriteLine(result);
    }
}
