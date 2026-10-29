using System.Diagnostics.CodeAnalysis;

namespace Crater.Compilation;

public class PathResolver(string projectRoot)
{
    private readonly string[] _searchPatterns = ["?.cra", "?/init.cra"];

    public bool TryGetPath(string moduleName, [NotNullWhen(true)] out string? resolvedPath)
    {
        var modulePath = moduleName.Replace('.', Path.DirectorySeparatorChar);

        foreach (var pattern in _searchPatterns)
        {
            var relative = pattern.Replace("?", modulePath);
            var candidate = Path.GetFullPath(Path.Combine(projectRoot, relative));
            if (!File.Exists(candidate))
                continue;

            resolvedPath = candidate;
            return true;
        }

        resolvedPath = null;
        return false;
    }

    public IReadOnlyList<string> GetAttemptPaths(string moduleName)
    {
        var attempts = new List<string>();

        var modulePath = moduleName.Replace('.', Path.DirectorySeparatorChar);

        foreach (var pattern in _searchPatterns)
        {
            var relative = pattern.Replace("?", modulePath);
            attempts.Add(Path.GetFullPath(Path.Combine(projectRoot, relative)));
        }

        return attempts;
    }
}
