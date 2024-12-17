namespace Hlf.Transpiler.DatapackGen;

public class Directory(string name) : Node(name)
{
    public List<Node> Children { get; set; } = new();

    public Directory CreateChild(string name)
    {
        var dir = new Directory(name);
        Children.Add(dir);
        return dir;
    }
    public File CreateFile(string name, string content)
    {
        var file = new File(name);
        file.Content = content;
        Children.Add(file);
        return file;
    }

    public override string Type => "dir";
}