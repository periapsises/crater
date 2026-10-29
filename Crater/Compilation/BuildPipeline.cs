using Antlr4.Runtime;
using Crater.Antlr;
using Crater.SemanticAnalysis;
using Crater.SyntaxTree;

namespace Crater.Compilation;

public class BuildPipeline
{
    public void BuildFile(string path)
    {
        var inputStream = new AntlrFileStream(path);
        var craterLexer = new CraterLexer(inputStream);
        var tokenStream = new CommonTokenStream(craterLexer);
        var craterParser = new CraterParser(tokenStream);

        var reporter = new ErrorReporter();

        var syntaxTreeConverter = new SyntaxTreeConverter(reporter);
        var program = (Program)syntaxTreeConverter.Visit(craterParser.program());

        var semanticAnalyzer = new SemanticAnalyzer(this, reporter);
        semanticAnalyzer.AnalyzeProgram(program);
    }
}
