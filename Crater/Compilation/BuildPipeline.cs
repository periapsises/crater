using System.Text;
using Antlr4.Runtime;
using Crater.Antlr;
using Crater.SemanticAnalysis;
using Crater.SyntaxTree;
using Environment = System.Environment;

namespace Crater.Compilation;

public class BuildPipeline(string projectRoot, string? outputDirectory)
{
    private readonly string _projectRoot = projectRoot;
    private readonly string? _outputDirectory = outputDirectory;

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
        if (string.IsNullOrEmpty(_outputDirectory))
        {
            outputDirectory = Path.GetDirectoryName(fullSourcePath) ?? Environment.CurrentDirectory;
        }
        else
        {
            outputDirectory = Path.IsPathRooted(_outputDirectory)
                ? _outputDirectory
                : Path.Combine(Path.GetDirectoryName(_projectRoot)!, _outputDirectory);

            Directory.CreateDirectory(outputDirectory);
        }

        var outputFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(sourcePath) + ".lua");
        using var fileStream = File.Open(outputFile, FileMode.Create);
        fileStream.Write(Encoding.UTF8.GetBytes(output));
        fileStream.Close();

        ModuleFinished?.Invoke(this, (sourcePath, true));
    }
}
