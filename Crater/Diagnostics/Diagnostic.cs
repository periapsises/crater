using Crater.SyntaxTree;

namespace Crater.Diagnostics;

public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error,
    Fatal
}

public record Diagnostic(string code, string message, DiagnosticSeverity severity, Source source);
