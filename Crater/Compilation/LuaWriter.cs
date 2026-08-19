using System.Text;

namespace Crater.Compilation;

public class LuaWriter
{
    private readonly StringBuilder _builder = new();
    private bool _needsIndent;
    private int _indent;

    public void Indent() => _indent++;
    public void Outdent() => _indent--;

    private void ApplyIndent()
    {
        if (!_needsIndent)
            return;

        _builder.Append(' ', _indent * 4);
        _needsIndent = false;
    }
    
    public void Write(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        
        ApplyIndent();
        _builder.Append(text);
    }

    public void WriteLine(string text)
    {
        ApplyIndent();
        _builder.AppendLine(text);
        _needsIndent = true;
    }

    public override string ToString() => _builder.ToString();
}