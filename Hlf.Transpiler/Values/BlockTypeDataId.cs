using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class BlockTypeDataId : DataId
{
    internal static int counter = 0;

    internal static string GetReservedAreaBox(GeneratorOptions gen) => $"{gen.BlockMemoryBasePosition} {gen.BlockMemoryBasePosition.X+counter*2+1} {gen.BlockMemoryBasePosition.Y+2} {gen.BlockMemoryBasePosition.Z+2}";
    public BlockTypeDataId()
    {
        X = counter+1;
        Y = 1;
        Z = 1;
        counter += 2;
        Type = HlfType.BlockType;
    }
    public int X;
    public int Y;
    public int Z;

    public override string Generate(GeneratorOptions opt) => $"{opt.BlockMemoryBasePosition.X + X} {opt.BlockMemoryBasePosition.Y + Y} {opt.BlockMemoryBasePosition.Z + Z}";
    public override string Free(GeneratorOptions gen)
    {
        return /*$"setblock {gen.BlockMemoryBasePosition.X + X} {gen.BlockMemoryBasePosition.Y + Y} {gen.BlockMemoryBasePosition.Z + Z} obsidian\n" +
               $"setblock {gen.BlockMemoryBasePosition.X + X} {gen.BlockMemoryBasePosition.Y + Y} {gen.BlockMemoryBasePosition.Z + Z} obsidian\n" +*/
               $"setblock {Generate(gen)} minecraft:air";
    }

    public static string GenerateAsNbt(BlockTypeDataId id, GeneratorOptions opt) => $"[{id.X} {id.Y} {id.Z}]";
}