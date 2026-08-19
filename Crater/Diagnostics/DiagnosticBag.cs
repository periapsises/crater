using System.Collections;

namespace Crater.Diagnostics;

public class DiagnosticBag : IDiagnosticReporter, IEnumerable<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = [];

    public bool hasErrors => _diagnostics.Any(diagnostic => diagnostic.severity is DiagnosticSeverity.Error or DiagnosticSeverity.Fatal);

    public IReadOnlyList<Diagnostic> diagnostics => _diagnostics;
    
    public void Report(Diagnostic diagnostic)
    {
        _diagnostics.Add(diagnostic);
    }

    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}