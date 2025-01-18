using Hlf.Transpiler.DatapackGen;

namespace Hlf.Transpiler.CodeGen;

public struct GeneratorOptions()
{
    public bool Optimize { get; set; } = false;
    public bool RuntimeErrors { get; set; } = true;
    public bool GenerateComments { get; set; } = true;
    public string StorageNamespace { get; set; } = "hlfdata";
    public string DatapackNamespace { get; set; } = "hlf";
    public string Scoreboard { get; set; } = "hlf";
    public string MarkerTag = "hlf_marker";
    public string OwnedEntityTag = "hlf_owned_entity";

    public string BlockMemoryDimension = "minecraft:overworld";
    public Vector3Int BlockMemoryBasePosition = new(0, 120, 0);
    public McVersion TargetVersion { get; set; } = McVersion.OneDot(21);

    internal List<(string, string)> ExtraFunctionsToGenerate { get; set; } = new();

    public string Comment(string comment) => GenerateComments ? $"# {comment}" : "";
}

public record struct Vector3Int(int X, int Y, int Z)
{
    public override string ToString()
    {
        return $"{X} {Y} {Z}";
    }
}