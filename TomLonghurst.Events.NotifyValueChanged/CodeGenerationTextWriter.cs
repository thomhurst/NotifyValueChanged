using System.CodeDom.Compiler;

namespace TomLonghurst.Events.NotifyValueChanged;

public class CodeGenerationTextWriter : IndentedTextWriter
{
    public CodeGenerationTextWriter(TextWriter writer) : base(writer)
    {
    }

    public CodeGenerationTextWriter(TextWriter writer, string tabString) : base(writer, tabString)
    {
    }

    public override void WriteLine(string s)
    {
        if (s.Trim().StartsWith("}"))
        {
            Indent--;
        }
        
        base.WriteLine(s);
        
        if (s.Trim().StartsWith("{"))
        {
            Indent++;
        }
    }
}