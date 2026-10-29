using System.ComponentModel;
using System.Diagnostics;
using Crater.Compilation;
using Spectre.Console;
using Spectre.Console.Cli;

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
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var projectRoot = Path.GetDirectoryName(Path.GetFullPath(settings.source))!;
        var pipeline = new BuildPipeline(projectRoot, settings.outputDirectory);

        var tasks = new Dictionary<string, ProgressTask>();

        var spinnerColumn = new SpinnerColumn();
        var descriptionColumn = new TaskDescriptionColumn
        {
            Alignment = Justify.Left
        };

        AnsiConsole.Progress().Columns(spinnerColumn, descriptionColumn).Start(ctx =>
        {
            pipeline.ModuleStart += (_, path) =>
            {
                var relative = Path.GetRelativePath(projectRoot, path);

                foreach (var kvp in tasks)
                {
                    if (kvp.Value.IsFinished && kvp.Key != path)
                        kvp.Value.Description = $"Compiling [gray]{Path.GetRelativePath(projectRoot, kvp.Key)} (waiting)[/]";
                }

                var task = ctx.AddTask($"Compiling [yellow]{relative}[/]", autoStart: true);
                task.IsIndeterminate = true;
                tasks[path] = task;
            };

            pipeline.ModuleFinished += (_, status) =>
            {
                var path = status.Item1;
                var reporter = status.Item2;
                var relative = Path.GetRelativePath(projectRoot, path);

                if (!tasks.TryGetValue(path, out var task))
                    return;

                if (reporter.hasErrors)
                    task.Description = $"Compiling [gray]{relative}[/] [red]FAILED[/]";
                else
                    task.Description = $"Compiling [gray]{relative}[/] [green]SUCCESS[/]";

                task.Value = task.MaxValue;
                task.StopTask();
            };

            pipeline.BuildFile(settings.source);
        });

        stopwatch.Stop();
        AnsiConsole.MarkupLineInterpolated($"Compilation finished ({FormatElapsedTime(stopwatch)})");

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
