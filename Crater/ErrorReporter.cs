using System.Runtime.CompilerServices;
using Crater.Diagnostics;
using Crater.Diagnostics.Codes;
using Spectre.Console;

namespace Crater;

public class ErrorReporter : IDiagnosticReporter
{
    public bool hasErrors { get; private set; } = false;

    public void Report(Diagnostic diagnostic)
    {
        if (diagnostic.severity is DiagnosticSeverity.Error or DiagnosticSeverity.Fatal)
            hasErrors = true;

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
