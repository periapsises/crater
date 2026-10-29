using System.Text;
using Antlr4.Runtime;
using Crater.Antlr;
using Crater.SemanticAnalysis;
using Crater.SyntaxTree;
using Environment = Crater.SemanticAnalysis.Environment;
using Env = System.Environment;

namespace Crater.Compilation;

public class BuildPipeline(string projectRoot, string? directory)
{
    public readonly Environment GlobalEnvironment = new();

    public EventHandler<string>? ModuleStart;
    public EventHandler<(string, bool)>? ModuleFinished;

    public void BuildFile(string sourcePath)
    {
        ModuleStart?.Invoke(this, sourcePath);

        var inputStream = new AntlrFileStream(sourcePath);
        var craterLexer = new CraterLexer(inputStream);
        var tokenStream = new CommonTokenStream(craterLexer);
        var craterParser = new CraterParser(tokenStream);

        var reporter = new ErrorReporter();

        var syntaxTreeConverter = new SyntaxTreeConverter(reporter);
        var program = (Program)syntaxTreeConverter.Visit(craterParser.program());

        var semanticAnalyzer = new SemanticAnalyzer(this, reporter);
        semanticAnalyzer.AnalyzeProgram(program);

        if (reporter.hasErrors)
        {
            ModuleFinished?.Invoke(this, (sourcePath, false));
            return;
        }

        var output = Compiler.Compile(program);
        var fullSourcePath = Path.GetFullPath(sourcePath);

        string outputDirectory;
        if (string.IsNullOrEmpty(directory))
        {
            outputDirectory = Path.GetDirectoryName(fullSourcePath) ?? Env.CurrentDirectory;
        }
        else
        {
            outputDirectory = Path.IsPathRooted(directory) ? directory : Path.Combine(Path.GetDirectoryName(projectRoot)!, directory);
            Directory.CreateDirectory(outputDirectory);
        }

        var outputFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(sourcePath) + ".lua");
        using var fileStream = File.Open(outputFile, FileMode.Create);
        fileStream.Write(Encoding.UTF8.GetBytes(output));
        fileStream.Close();

        ModuleFinished?.Invoke(this, (sourcePath, true));
    }
}
