using Antlr4.Runtime;

namespace Crater;

public class Source(int startLine, int startColumn, int stopLine, int stopColumn, string file)
{
    public readonly int StartLine = startLine;
    public readonly int StartColumn = startColumn;
    public readonly int StopLine = stopLine;
    public readonly int StopColumn = stopColumn;
    public readonly string File = file;

    public bool isSingleLine => StartLine == StopLine;

    public static Source FromToken(IToken token)
    {
        var startLine = token.Line;
        var startColumn = token.Column;
        var stopColumn = startColumn + token.Text.Length;

        return new Source(startLine, startColumn, startLine, stopColumn, token.TokenSource.SourceName);
    }
    
    public static Source FromContext(ParserRuleContext context)
    {
        return new Source(context.Start.Line, context.Start.Column, context.Stop.Line, context.Stop.Column, context.Start.TokenSource.SourceName);
    }
}
