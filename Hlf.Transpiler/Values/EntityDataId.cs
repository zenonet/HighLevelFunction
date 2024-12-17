using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class EntityDataId : DataId
{
    private static int counter;
    private static string Allocate()
    {
        return $"dataid{counter++}";
    }
    
    public string Id { get; set; }
    
    public EntityDataId()
    {
        Id = Allocate();
        Type = HlfType.Entity;
    }

    public override string Generate(GeneratorOptions options) => Id;
    public override string Free(GeneratorOptions gen)
    {
        return $"tag @e[tag={Generate(gen)}] remove {Generate(gen)}";
    }
}