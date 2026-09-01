using Antlr4.Runtime;
using Crater.Antlr;
using Crater.Diagnostics;
using Crater.SemanticAnalysis;
using Crater.SyntaxTree;

namespace Crater.Compilation;

public class CompilationPipeline(ModuleResolver resolver, IDiagnosticReporter reporter)
{
    public CompilationUnit CompileEntry(string entryFile)
    {
        var moduleName = entryFile.Replace('/', '.').Replace('\\', '.');
        var sourcePath = $"{resolver.ProjectRoot}/{entryFile}";

        var inputStream = new AntlrFileStream(sourcePath);
        var craterLexer = new CraterLexer(inputStream);
        var tokenStream = new CommonTokenStream(craterLexer);
        var craterParser = new CraterParser(tokenStream);

        var syntaxTreeConverter = new SyntaxTreeConverter(reporter);
        var program = syntaxTreeConverter.Get<Program>(craterParser.program());

        var semanticAnalyzer = new SemanticAnalyzer(resolver, reporter);
        semanticAnalyzer.AnalyzeProgram(program);

        return new CompilationUnit(moduleName, sourcePath, program);
    }
}
