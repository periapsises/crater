using Antlr4.Runtime;
using Crater.Antlr;
using Crater.Compilation;
using Crater.Diagnostics;
using Crater.SemanticAnalysis;
using Crater.SyntaxTree;

namespace Crater;

public static class Crater
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Expected a file path to compile");
            return 1;
        }

        var inputStream = new AntlrFileStream(args[0]);
        var craterLexer = new CraterLexer(inputStream);
        var tokenStream = new CommonTokenStream(craterLexer);
        var craterParser = new CraterParser(tokenStream);

        var reporter = new DiagnosticBag();

        var syntaxTreeConverter = new SyntaxTreeConverter(reporter);
        var node = syntaxTreeConverter.Visit(craterParser.program());

        if (node is not Program program)
            throw new Exception("Failed to convert resulting tree.");

        var semanticAnalyzer = new SemanticAnalyzer(reporter);
        semanticAnalyzer.AnalyzeProgram(program);

        foreach (var diagnostic in reporter)
        {
            var diagnosticKind = diagnostic.severity switch
            {
                DiagnosticSeverity.Info => "info",
                DiagnosticSeverity.Warning => "warn",
                DiagnosticSeverity.Error => "error",
                _ => "fatal"
            };

            Console.WriteLine($"{diagnostic.source.File}:{diagnostic.source.StartLine}:{diagnostic.source.StartColumn}: {diagnosticKind} [{diagnostic.code}]");
            Console.WriteLine($"    {diagnostic.message}");
        }

        if (reporter.hasErrors)
            return 1;

        var output = Compiler.Compile(program);
        Console.WriteLine(output);

        return 0;
    }
}
