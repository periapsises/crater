namespace Crater.Diagnostics;

public interface IDiagnosticReporter
{
    bool hasErrors { get; }

    IReadOnlyList<Diagnostic> diagnostics { get; }

    void Report(Diagnostic diagnostic);
}
