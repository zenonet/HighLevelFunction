namespace Hlf.Transpiler.DatapackGen;

public class Function(string name, string src)
{
    public string SourceCode { get; set; } = src;
    public string Name { get; set; } = name;
}