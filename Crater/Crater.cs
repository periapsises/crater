using Antlr4.Runtime;
using Crater.Antlr;
using Crater.Compilation;
using Crater.Diagnostics;
using Crater.SemanticAnalysis;
using Crater.SyntaxTree;

namespace Crater;

public static class Crater
{
    public static void Main(string[] args)
    {
        const string source = """
                              hello: number = 5
                              world: number = "Hi"
                              local hello: string = "Hello"

                              do
                                  local hi: bool = true
                                  local hello: bool = false
                                  local sup: bool = 5
                              end

                              local a: number = nil
                              local b: number? = nil
                              local c: number? = 5
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

        var reporter = new DiagnosticBag();

        var semanticAnalyzer = new SemanticAnalyzer(reporter);
        semanticAnalyzer.AnalyzeProgram(program);

        foreach (var diagnostic in reporter)
            Console.WriteLine($"[{diagnostic.code}] {diagnostic.message} at line {diagnostic.source.StartLine}");

        if (reporter.hasErrors)
            return;

        var output = Compiler.Compile(program);
        Console.WriteLine(output);
    }
}
