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

                              local d: number
                              local e: number?

                              MyGlobalNumber: number

                              hello = "A string"
                              a = 5 + 7
                              b = -4

                              local first: number, second: string, third: number = 1, "2", 3
                              local one: number, two: string = 1, 2

                              if first then
                                  local test: number = 1
                              elseif fourth then
                                  test = 2
                              else
                                  NewGlobal: number = 10 + a
                              end

                              local myNullable: number?
                              local myNonNullable: number = 5

                              local myInvalidNot: number = not myNullable
                              local myValidNot: bool = not nil

                              local myInvalidOr: number = myNullable or myNullable
                              local myValidOr: number = myNullable or myNonNullable

                              local myInvalidAnd: number = myNullable and myNonNullable
                              local myOtherInvalidAnd: number = myNonNullable and myNullable
                              local myValidAnd: number = myNonNullable and myNonNullable

                              local myInvalidAndOr: number = myNullable and myNullable or myNullable
                              local myOtherInvalidAndOr: number = myNullable and myNonNullable or myNullable
                              local myThirdInvalidAndOr: number = myNonNullable and myNullable or myNullable
                              local myValidAndOr: number = myNullable and myNonNullable or myNonNullable
                              local myOtherValidAndOr: number = myNullable and myNullable or myNonNullable

                              local myBoolean: bool = false

                              local myInvalidTernary: number = myBoolean and myNonNullable or myBoolean
                              local myValidTernary: number = myBoolean and myNonNullable or myNonNullable
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
