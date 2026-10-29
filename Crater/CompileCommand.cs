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
        [Description("The source file to compile.")]
        [CommandArgument(0, "<SOURCE>")]
        public required string source { get; init; }

        [Description("The output directory for compiled Lua files.")]
        [CommandOption("-o|--outDir <DIRECTORY>")]
        public string? outputDirectory { get; init; }
    }

    private readonly List<string> _sourceFiles = [];

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (File.Exists(settings.source))
            return ValidationResult.Success();

        return ValidationResult.Error($"The source file '{settings.source}' does not exist.");
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.Status().SpinnerStyle(Style.Parse("blue")).Start("Compiling", ctx =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ctx.Status($"Compiling [cyan]{Path.GetFileName(settings.source)}[/]");

            var pipeline = new BuildPipeline(Path.GetDirectoryName(settings.source)!, settings.outputDirectory);
            pipeline.BuildFile(settings.source);

            stopwatch.Stop();
            AnsiConsole.MarkupLineInterpolated($"Compiling [cyan]{Path.GetFileName(settings.source)}[/] [green]SUCCESS[/] [gray]({FormatElapsedTime(stopwatch)})[/]");
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
