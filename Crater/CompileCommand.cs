using System.ComponentModel;
using System.Diagnostics;
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

        AnsiConsole.Status().SpinnerStyle(Style.Parse("blue")).Start("Compiling", ctx =>
        {
            foreach (var sourceFile in _sourceFiles)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                ctx.Status($"Compiling [cyan]{Path.GetFileName(sourceFile)}[/]");

                var pipeline = new BuildPipeline(settings.target, settings.outputDirectory);
                pipeline.BuildFile(sourceFile);

                stopwatch.Stop();
                AnsiConsole.MarkupLineInterpolated($"Compiling [cyan]{Path.GetFileName(sourceFile)}[/] [green]SUCCESS[/] [gray]({FormatElapsedTime(stopwatch)})[/]");
            }
        });

        return 0;
    }

    private static string FormatElapsedTime(Stopwatch stopwatch)
    {
        var totalMicroseconds = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency * 1000000;

        if (totalMicroseconds < 1000)
            return $"{totalMicroseconds:F0} µs";

        if (stopwatch.ElapsedMilliseconds < 1000)
            return $"{stopwatch.ElapsedMilliseconds} ms";

        return stopwatch.Elapsed.ToString(@"mm\:ss");
    }
}
