using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Crater.Compilation;
using Crater.Diagnostics;
using Crater.Diagnostics.Codes;
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

        [Description("Override the default LUA_PATH patterns.")]
        [CommandOption("-p|--path <PATTERNS>")]
        public string? pathOverride { get; init; }
    }

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

        var projectSource = Path.GetFullPath(settings.source);
        var projectRoot = Path.GetDirectoryName(projectSource)!;
        var pipeline = new BuildPipeline(projectRoot, settings.outputDirectory, settings.pathOverride);

        var tasks = new Dictionary<string, ProgressTask>();

        var spinnerColumn = new SpinnerColumn();
        var descriptionColumn = new TaskDescriptionColumn
        {
            Alignment = Justify.Left
        };

        var diagnostics = new List<(string, IDiagnosticReporter)>();
        var hasErrors = false;

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
                {
                    task.Description = $"Compiling [gray]{relative}[/] [red]FAILED[/]";
                    hasErrors = true;
                }
                else
                {
                    task.Description = $"Compiling [gray]{relative}[/] [green]SUCCESS[/]";
                }

                task.Value = task.MaxValue;
                task.StopTask();

                diagnostics.Add((path, reporter));
            };

            pipeline.BuildFile(projectSource);
        });

        foreach (var diagnosticReport in diagnostics)
        {
            if (diagnosticReport.Item2.diagnostics.Count == 0)
                continue;

            var path = Path.GetRelativePath(projectRoot, diagnosticReport.Item1);
            AnsiConsole.MarkupLineInterpolated($"Diagnostics for [gray]{path}[/]:\n");

            foreach (var diagnostic in diagnosticReport.Item2.diagnostics)
            {
                PrintDiagnostic(diagnostic);
                AnsiConsole.WriteLine();
            }
        }

        stopwatch.Stop();
        AnsiConsole.MarkupLineInterpolated($"Compilation finished ({FormatElapsedTime(stopwatch)})");

        return hasErrors ? 1 : 0;
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

    private static void PrintDiagnostic(Diagnostic diagnostic)
    {
        var severityColor = diagnostic.severity switch
        {
            DiagnosticSeverity.Info => "blue",
            DiagnosticSeverity.Warning => "yellow",
            DiagnosticSeverity.Error => "red",
            DiagnosticSeverity.Fatal => "red",
            _ => throw new SwitchExpressionException(diagnostic.severity)
        };

        var severityCode = diagnostic.severity switch
        {
            DiagnosticSeverity.Info => "Info",
            DiagnosticSeverity.Warning => "Warn",
            DiagnosticSeverity.Error => "Error",
            DiagnosticSeverity.Fatal => "Fatal",
            _ => throw new SwitchExpressionException(diagnostic.severity)
        };

        AnsiConsole.MarkupLineInterpolated($"[yellow underline]{diagnostic.source.File}:{diagnostic.source.StartLine}:{diagnostic.source.StartColumn}[/] - [{severityColor} bold]{severityCode}[/] [lightcyan1]{diagnostic.code}[/]:");

        var message = diagnostic.message;
        if (diagnostic.code == SemanticWarnings.CyclicModuleDependency)
        {
            var insertPosition = message.LastIndexOf("->", StringComparison.Ordinal) + 2;
            message = message[..insertPosition] + "[red]" + message[insertPosition..] + "[/]";
        }

        AnsiConsole.MarkupLine("  " + message);
    }
}
