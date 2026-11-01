using Antlr4.Runtime;

namespace Crater;

public class NamedFileStream(string filePath, string customName) : AntlrFileStream(filePath)
{
    public override string SourceName => customName;
}
