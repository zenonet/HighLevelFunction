using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class NbtDataId : DataId
{
    private static int counter;
    private static string Allocate()
    {
        return $"dataid{counter++}";
    }
    public string Id { get; set; }
    
    public NbtDataId(HlfType type)
    {
        Id = Allocate();
        Type = type;
    }

    public override string Generate(GeneratorOptions options) => Id;
    public override string Free(GeneratorOptions gen)
    {
        return $";fr:data remove storage {gen.StorageNamespace} {Id}";
    }
}