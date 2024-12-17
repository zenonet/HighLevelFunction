namespace Hlf.Transpiler.DatapackGen;

public class Function
{
    public string SourceCode { get; set; }
    public string Name { get; set; }

    public Function()
    {
    }

    public Function(string name, string src)
    {
        SourceCode = src;
        Name = name;
    }
}