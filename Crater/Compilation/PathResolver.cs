using System.Diagnostics.CodeAnalysis;

namespace Crater.Compilation;

public enum PathResolutionResult
{
    InvalidModuleName,
    ModuleNotFound,
    MultipleModulesFound,
    Success
}

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

    public PathResolutionResult TryGetModule(string moduleName, out IReadOnlyList<string> path)
    {
        if (!TryGetPathsFromName(moduleName, out var paths))
        {
            path = [];
            return PathResolutionResult.InvalidModuleName;
        }

        var matches = new List<string>();
        foreach (var candidate in paths)
        {
            if (!File.Exists(candidate))
                continue;

            matches.Add(candidate);
        }

        if (matches.Count == 0)
        {
            path = [];
            return PathResolutionResult.ModuleNotFound;
        }

        path = matches;
        return matches.Count == 1 ? PathResolutionResult.Success : PathResolutionResult.MultipleModulesFound;
    }

    public bool TryGetPathsFromName(string moduleName, [NotNullWhen(true)] out IReadOnlyList<string>? paths)
    {
        if (!TryGetRelativeFragment(moduleName, out var fragment))
        {
            paths = null;
            return false;
        }

        var candidates = new List<string>();

        foreach (var pattern in _searchPatterns)
        {
            var relative = pattern.Replace("?", fragment);
            var candidate = Path.GetFullPath(Path.Combine(_projectRoot, relative));

            candidates.Add(candidate);
        }

        paths = candidates;
        return true;
    }

    private static bool TryGetRelativeFragment(string moduleName, out string fragment)
    {
        if (string.IsNullOrEmpty(moduleName))
        {
            fragment = "";
            return false;
        }

        var segments = moduleName.Split('.');
        foreach (var segment in segments)
        {
            if (IsIdentifier(segment))
                continue;

            fragment = "";
            return false;
        }

        fragment = string.Join(Path.DirectorySeparatorChar, segments);
        return true;
    }

    private static bool IsIdentifier(string segment)
    {
        if (segment.Length == 0 || !char.IsAsciiLetter(segment[0]) && segment[0] != '_')
            return false;

        for (var i = 1; i < segment.Length; i++)
        {
            var c = segment[i];
            if (!char.IsAsciiLetterOrDigit(c) && c != '_')
                return false;
        }

        return true;
    }
}
