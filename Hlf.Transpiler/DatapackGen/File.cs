namespace Hlf.Transpiler.DatapackGen;

public class File(string name) : Node(name)
{
    public string Content { get; set; }

    public override string Type => "File";
}