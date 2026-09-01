using Crater.SyntaxTree;

namespace Crater.Compilation;

public class CompilationUnit(string moduleName, string sourcePath, Program syntaxTree)
{
    public readonly string ModuleName = moduleName;
    public readonly string SourcePath = sourcePath;
    public readonly Program SyntaxTree = syntaxTree;
}
