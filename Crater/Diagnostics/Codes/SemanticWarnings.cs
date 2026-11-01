namespace Crater.Diagnostics.Codes;

public static class SemanticWarnings
{
    public const string VariableShadowing = "CRA400";
    public const string ModuleNotFound = "CRA401";
    public const string CyclicModuleDependency = "CRA402";
    public const string TopLevelReturnsMultipleValues = "CRA403";
    public const string MultipleModulesMatch = "CRA404";
    public const string MalformedModuleName = "CRA405";
}
