using System.CodeDom.Compiler;

namespace TomLonghurst.Events.NotifyValueChanged;

public class CodeGenerationTextWriter : IndentedTextWriter
{
    public CodeGenerationTextWriter() : base(new StringWriter())
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

    public override string ToString()
    {
        Flush();
        return InnerWriter.ToString();
    }
}