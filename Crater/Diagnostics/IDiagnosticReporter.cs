namespace Crater.Diagnostics;

public interface IDiagnosticReporter
{
    bool hasErrors { get; }
    
    void Report(Diagnostic diagnostic);
}