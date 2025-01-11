using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class BlockTypeDataId : DataId
{
    internal static int counter = 0;
    public BlockTypeDataId()
    {
        X = counter++;
        Type = HlfType.BlockType;
    }
    public int X;
    public int Y;
    public int Z;

    public override string Generate(GeneratorOptions opt) => $"{opt.BlockMemoryBasePosition.X + X} {opt.BlockMemoryBasePosition.Y + Y} {opt.BlockMemoryBasePosition.Z + Z}";
    public override string Free(GeneratorOptions gen)
    {
        return $"setblock {Generate(gen)} minecraft:air";
    }

    public static string GenerateAsNbt(BlockTypeDataId id, GeneratorOptions opt) => $"[{id.X} {id.Y} {id.Z}]";
}