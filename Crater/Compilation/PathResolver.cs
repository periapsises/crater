namespace Crater.Compilation;

public class PathResolver
{
    private readonly string _projectRoot;
    private readonly string[] _searchPatterns;

    public PathResolver(string projectRoot, string? pattern = null)
    {
        _projectRoot = projectRoot;

        if (string.IsNullOrEmpty(pattern))
        {
            _searchPatterns = ["?.cra", "?/init.cra"];
            return;
        }

        _searchPatterns = pattern.Split(';');
    }

    public IReadOnlyList<string> FindModule(string moduleName)
    {
        var candidates = GetAttemptPaths(moduleName);
        var matches = new List<string>();

        foreach (var candidate in candidates)
        {
            if (!File.Exists(candidate))
                continue;

            matches.Add(candidate);
        }

        return matches;
    }

    public IReadOnlyList<string> GetAttemptPaths(string moduleName)
    {
        var attempts = new List<string>();

        var modulePath = moduleName.Replace('.', Path.DirectorySeparatorChar);

        foreach (var pattern in _searchPatterns)
        {
            var relative = pattern.Replace("?", modulePath);
            attempts.Add(Path.GetFullPath(Path.Combine(_projectRoot, relative)));
        }

        return attempts;
    }
}
