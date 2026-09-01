using System.Diagnostics.CodeAnalysis;

namespace Crater.Compilation;

public class ModuleResolver(string projectRoot)
{
    private readonly string _projectRoot = projectRoot;
    private readonly string _searchPatterns = "?.cra;?/init.cra";

    public bool TryResolve(string moduleName, [NotNullWhen(true)] out string? path)
    {
        var searchPaths = _searchPatterns.Replace("?", moduleName).Split(';');

        foreach (var searchPath in searchPaths)
        {
            var attemptPath = $"{_projectRoot}/{searchPath}";
            if (!File.Exists(attemptPath))
                continue;

            path = attemptPath;
            return true;
        }

        path = null;
        return false;
    }
}
