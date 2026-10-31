namespace Crater.Compilation;

public class PathResolver(string projectRoot)
{
    private readonly string[] _searchPatterns = ["?.cra", "?/init.cra"];

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
            attempts.Add(Path.GetFullPath(Path.Combine(projectRoot, relative)));
        }

        return attempts;
    }
}
