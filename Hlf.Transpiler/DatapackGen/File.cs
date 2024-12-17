namespace Hlf.Transpiler.DatapackGen;

public class File(string path, string content)
{
    public string Path { get; set; } = path;
    public string Content { get; set; } = content;
}