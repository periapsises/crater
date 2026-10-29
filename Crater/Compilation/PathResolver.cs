using System.Diagnostics.CodeAnalysis;

namespace Crater.Compilation;

public static class PathResolver
{
    private static readonly string[] SearchPatterns = ["?.cra", "?/init.cra"];

    public static bool TryGetPath(string moduleName, [NotNullWhen(true)] out string? resolvedPath)
    {
        var modulePath = moduleName.Replace('.', Path.DirectorySeparatorChar);

        foreach (var pattern in SearchPatterns)
        {
            var relative = pattern.Replace("?", modulePath);
            var candidate = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relative));
            if (!File.Exists(candidate))
                continue;

            resolvedPath = candidate;
            return true;
        }

        resolvedPath = null;
        return false;
    }
}
