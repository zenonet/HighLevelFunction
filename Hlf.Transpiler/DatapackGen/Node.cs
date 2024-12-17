namespace Hlf.Transpiler.DatapackGen;

public abstract class Node(string name)
{
    public string Name { get; } = name;
    public abstract string Type { get; }

    public override string ToString()
    {
        return Name;
    }
}