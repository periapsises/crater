using System.Text;
using Antlr4.Runtime;
using Crater.Antlr;
using Crater.Diagnostics;
using Crater.SemanticAnalysis;
using Crater.SyntaxTree;
using Environment = Crater.SemanticAnalysis.Environment;
using Type = Crater.SemanticAnalysis.Type;

namespace Crater.Compilation;

public class BuildPipeline(string projectRoot, string? directory, string? pathOverride = null)
{
    public readonly string ProjectRoot = projectRoot;

    public readonly Environment GlobalEnvironment = new();
    public readonly PathResolver PathResolver = new(projectRoot, pathOverride);
    public readonly Stack<string> ModulesInProgress = [];
    public readonly Dictionary<string, Type> CompletedModules = [];

    public readonly Dictionary<string, string> OutputToSource = [];

    public EventHandler<string>? ModuleStart;
    public EventHandler<(string, IDiagnosticReporter)>? ModuleFinished;

    public void BuildFile(string sourcePath)
    {
        if (ModulesInProgress.Contains(sourcePath))
            throw new ArgumentException("This module is already in progress (cyclic dependency)", nameof(sourcePath));

        ModulesInProgress.Push(sourcePath);
        ModuleStart?.Invoke(this, sourcePath);

        var inputStream = new NamedFileStream(sourcePath, Path.GetRelativePath(ProjectRoot, sourcePath));
        var craterLexer = new CraterLexer(inputStream);
        var tokenStream = new CommonTokenStream(craterLexer);
        var craterParser = new CraterParser(tokenStream);

        var reporter = new DiagnosticBag();

        var syntaxTreeConverter = new SyntaxTreeConverter(reporter);
        var program = (Program)syntaxTreeConverter.Visit(craterParser.program());

        var semanticAnalyzer = new SemanticAnalyzer(this, reporter);
        var returnType = semanticAnalyzer.AnalyzeProgram(program);

        if (reporter.hasErrors)
        {
            CompletedModules[sourcePath] = TypeRegistry.UnknownType;

            ModulesInProgress.Pop();
            ModuleFinished?.Invoke(this, (sourcePath, reporter));
            return;
        }

        CompletedModules[sourcePath] = returnType;

        var output = Compiler.Compile(program);

        var outputFile = GetOutputPath(sourcePath);
        using var fileStream = File.Open(outputFile, FileMode.Create);
        fileStream.Write(Encoding.UTF8.GetBytes(output));
        fileStream.Close();

        ModulesInProgress.Pop();
        ModuleFinished?.Invoke(this, (sourcePath, reporter));
    }

    private string GetOutputPath(string sourceFilePath)
    {
        if (!Path.IsPathFullyQualified(sourceFilePath) || !File.Exists(sourceFilePath))
            throw new Exception($"Invalid source path: {sourceFilePath}");

        if (!Directory.Exists(projectRoot))
            throw new Exception($"Invalid project root: {sourceFilePath}");

        var outputDirectory = string.IsNullOrEmpty(directory) ? null : directory.Trim();
        string outputRoot;

        if (outputDirectory == null)
            outputRoot = Path.GetDirectoryName(sourceFilePath)!;
        else if (Path.IsPathFullyQualified(outputDirectory))
            outputRoot = Path.GetFullPath(outputDirectory);
        else
            outputRoot = Path.GetFullPath(Path.Combine(ProjectRoot, outputDirectory));

        var relativeSource = Path.GetRelativePath(ProjectRoot, sourceFilePath);
        var relativeOutput = Path.ChangeExtension(sourceFilePath, ".lua");

        var candidate = Path.GetFullPath(Path.Combine(outputRoot, relativeOutput));
        if (Directory.Exists(candidate))
            throw new Exception($"Output path exists as a directory: {candidate}");

        if (candidate.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            throw new Exception($"Output path contains invalid characters: {candidate}");

        var canonical = Path.GetFullPath(candidate);
        if (OutputToSource.TryGetValue(canonical, out var originalSource))
        {
            if (originalSource != sourceFilePath)
            {
                var firstSource = Path.GetRelativePath(ProjectRoot, originalSource);
                var secondSource = Path.GetRelativePath(ProjectRoot, sourceFilePath);
                throw new Exception($"Output path collision: '{firstSource}' and '{secondSource}' both map to {candidate}");
            }
        }

        var fullOutputDirectory = Path.GetDirectoryName(candidate)!;
        if (!Directory.Exists(fullOutputDirectory))
            Directory.CreateDirectory(fullOutputDirectory);

        OutputToSource.Add(canonical, sourceFilePath);
        return candidate;
    }

    public IReadOnlyList<string> GetCurrentRequireStack()
    {
        return ModulesInProgress.ToArray();
    }
}
