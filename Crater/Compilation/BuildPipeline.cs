using System.Text;
using Antlr4.Runtime;
using Crater.Antlr;
using Crater.Diagnostics;
using Crater.SemanticAnalysis;
using Crater.SyntaxTree;
using Environment = Crater.SemanticAnalysis.Environment;
using Env = System.Environment;
using Type = Crater.SemanticAnalysis.Type;

namespace Crater.Compilation;

public class BuildPipeline(string projectRoot, string? directory)
{
    public readonly Environment GlobalEnvironment = new();
    public readonly HashSet<string> ModulesInProgress = [];
    public readonly Dictionary<string, List<(Type, Source)>> CompletedModules = [];

    public EventHandler<string>? ModuleStart;
    public EventHandler<(string, IDiagnosticReporter)>? ModuleFinished;

    public void BuildFile(string sourcePath)
    {
        if (!ModulesInProgress.Add(sourcePath))
            throw new ArgumentException("This module is already in progress (cyclic dependency)", nameof(sourcePath));

        ModuleStart?.Invoke(this, sourcePath);

        var inputStream = new NamedFileStream(sourcePath, Path.GetRelativePath(projectRoot, sourcePath));
        var craterLexer = new CraterLexer(inputStream);
        var tokenStream = new CommonTokenStream(craterLexer);
        var craterParser = new CraterParser(tokenStream);

        var reporter = new ErrorReporter();

        var syntaxTreeConverter = new SyntaxTreeConverter(reporter);
        var program = (Program)syntaxTreeConverter.Visit(craterParser.program());

        var semanticAnalyzer = new SemanticAnalyzer(this, reporter);
        var returns = semanticAnalyzer.AnalyzeProgram(program);

        if (reporter.hasErrors)
        {
            CompletedModules[sourcePath] = [];

            ModulesInProgress.Remove(sourcePath);
            ModuleFinished?.Invoke(this, (sourcePath, reporter));
            return;
        }

        CompletedModules[sourcePath] = returns;

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

        ModuleFinished?.Invoke(this, (sourcePath, reporter));
        ModulesInProgress.Remove(sourcePath);
    }
}
