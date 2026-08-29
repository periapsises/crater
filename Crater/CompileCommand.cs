using System.ComponentModel;
using System.Text;
using Antlr4.Runtime;
using Crater.Antlr;
using Crater.Compilation;
using Crater.SemanticAnalysis;
using Crater.SyntaxTree;
using Spectre.Console;
using Spectre.Console.Cli;
using Environment = System.Environment;

namespace Crater;

public class CompileCommand : Command<CompileCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("The source file or directory to compile.")]
        [CommandArgument(0, "<TARGET>")]
        public required string target { get; init; }

        [Description("The output directory for compiled Lua files.")]
        [CommandOption("-o|--outDir <DIRECTORY>")]
        public string? outputDirectory { get; init; }
    }

    private readonly List<string> _sourceFiles = [];

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (File.Exists(settings.target))
        {
            _sourceFiles.Add(settings.target);
            return ValidationResult.Success();
        }

        if (Directory.Exists(settings.target))
        {
            _sourceFiles.AddRange(Directory.GetFiles(settings.target, "*.cra"));
            return ValidationResult.Success();
        }

        return ValidationResult.Error($"The target path '{settings.target}' does not exist.");
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var targetFullPath = Path.GetFullPath(settings.target);

        var exitCode = 0;

        AnsiConsole.Status().SpinnerStyle(Style.Parse("blue")).Start("Compiling", ctx =>
        {
            foreach (var sourceFile in _sourceFiles)
            {
                ctx.Status($"Compiling [cyan]{Path.GetFileName(sourceFile)}[/]");

                var inputStream = new AntlrFileStream(sourceFile);
                var craterLexer = new CraterLexer(inputStream);
                var tokenStream = new CommonTokenStream(craterLexer);
                var craterParser = new CraterParser(tokenStream);

                var reporter = new ErrorReporter();

                var syntaxTreeConverter = new SyntaxTreeConverter(reporter);
                var program = (Program)syntaxTreeConverter.Visit(craterParser.program());

                var semanticAnalyzer = new SemanticAnalyzer(reporter);
                semanticAnalyzer.AnalyzeProgram(program);

                if (reporter.hasErrors)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLineInterpolated($"Compiling [cyan]{Path.GetFileName(sourceFile)}[/] [red]FAILED[/]");
                    AnsiConsole.WriteLine();

                    exitCode = 1;
                    continue;
                }

                var output = Compiler.Compile(program);

                var fullSourcePath = Path.GetFullPath(sourceFile);

                string outputDirectory;
                if (string.IsNullOrEmpty(settings.outputDirectory))
                {
                    outputDirectory = Path.GetDirectoryName(fullSourcePath) ?? Environment.CurrentDirectory;
                }
                else
                {
                    outputDirectory = Path.IsPathRooted(settings.outputDirectory)
                        ? settings.outputDirectory
                        : Path.Combine(Path.GetDirectoryName(targetFullPath)!, settings.outputDirectory);

                    Directory.CreateDirectory(outputDirectory);
                }

                var outputFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(sourceFile) + ".lua");
                using var fileStream = File.Open(outputFile, FileMode.Create);
                fileStream.Write(Encoding.UTF8.GetBytes(output));
                fileStream.Close();

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLineInterpolated($"Compiling [cyan]{Path.GetFileName(sourceFile)}[/] [green]SUCCESS[/]");
                AnsiConsole.WriteLine();
            }
        });

        return exitCode;
    }
}
