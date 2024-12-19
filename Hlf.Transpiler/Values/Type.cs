using System.Diagnostics.CodeAnalysis;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class HlfType(string? name, ValueKind kind, Conversion[]? implicitConversions = null, OperationDefinition[]? operations = null)
{
    public static HlfType String = new("string", ValueKind.Nbt);
    public static HlfType Int = new("int", ValueKind.Nbt, [], []);
    public static HlfType Float = new("float", ValueKind.Nbt);
    public static HlfType Bool = new("bool", ValueKind.Nbt);
    public static HlfType Entity = new("Entity", ValueKind.EntityTag);
    public static HlfType Vector = new("Vector", ValueKind.Nbt);
    public static HlfType BlockType = new("BlockType", ValueKind.Block);
    public static HlfType Void = new("void", ValueKind.Void);

    
    public static HlfType ConstString = new("const string", ValueKind.Constant,
    [
        new(String,
            (gen, from, to) =>
                $"data modify storage {gen.StorageNamespace} {to.Generate(gen)} set value \"{from.Generate(gen)}\"")
    ]);


    static HlfType()
    {
        Int.Operations =
        [
            OperationImplementations.IntEq,
        ];

        BlockType.Operations =
        [
            OperationImplementations.BlockTypeEq,
        ];

        Entity.Operations =
        [
            OperationImplementations.EntityRefEq,
        ];

        Bool.ImplicitConversions =
        [
            new(Int, (gen, from, to) =>
            {
                return gen.Convert(from.Generate(gen), to.Generate(gen), "int");
            }),
        ];

        Int.ImplicitConversions =
        [
            new(Bool, (gen, from, to) =>
            {
                return gen.Convert(from.Generate(gen), to.Generate(gen), "byte");
            }),
            new(Float, (gen, from, to) =>
            {
                return gen.Convert(from.Generate(gen), to.Generate(gen), "double");
            }),
        ];
    }

    public static HlfType[] Types =
    [
        String,
        Int,
        Float,
        Bool,
        Entity,
        Vector,
        BlockType,
        Void,
    ];
    
    public ValueKind Kind = kind;
    public string? Name = name;
    
    public Conversion[] ImplicitConversions = implicitConversions ?? [];
    public OperationDefinition[] Operations = operations ?? [];

    public static bool TryGetTypeFromName(string name, [NotNullWhen(true)]out HlfType? type)
    {
        type = Types.FirstOrDefault(x => x.Name == name);
        return type != null;
    }

    public bool TryGenerateImplicitConversion(GeneratorOptions gen, HlfType otherType, DataId inputId, [NotNullWhen(true)]out string? code, [NotNullWhen(true)]out DataId? resultId)
    {
        Conversion? conversion = ImplicitConversions.FirstOrDefault(x => x.ResultType == otherType);
        if (conversion == null)
        {
            code = null;
            resultId = null;
            return false;
        }

        resultId = otherType.NewDataId();
        code = conversion.Generator.Invoke(gen, inputId, resultId);
        return true;
    }
    
    /// <summary>
    /// Checks if the two types match or if there is a way to convert the value to the target type
    /// </summary>
    /// <param name="hlfType">The target type</param>
    /// <returns>Whether a value of this type can be assigned to a variable of the target type</returns>
    public bool IsAssignableTo(HlfType hlfType) => this == hlfType || ImplicitConversions.Any(x => x.ResultType == hlfType);

    public override string ToString()
    {
        return $"Type: {Name}";
    }

    public DataId NewDataId()
    {
        DataId dataId = DataId.FromType(this);
        return dataId;
    }
}

public static class OperationImplementations
{
    public static OperationDefinition IntEq = new(TokenType.DoubleEquals, HlfType.Bool, HlfType.Int, (gen, a, b, result) =>
    {
        return gen.Comment("Comaring 2 ints via scoreboard\n") +
               $"{gen.CopyDataToScoreboard(a.Generate(gen), "a")}\n" +
               $"{gen.CopyDataToScoreboard(b.Generate(gen), "b")}\n" +
               $"execute store success storage {gen.StorageNamespace} {result.Generate(gen)} byte 1 if score a hlf = b hlf";
    });
    
    public static OperationDefinition BlockTypeEq = new(TokenType.DoubleEquals, HlfType.Bool, HlfType.BlockType, (gen, a, b, result) =>
    {
        return gen.Comment("Comparing 2 blocks by type\n") +
               $"execute store result storage {gen.StorageNamespace} {result.Generate(gen)} byte 1 if blocks {a.Generate(gen)} {a.Generate(gen)} {b.Generate(gen)} all\n";
    });

    private static readonly Lazy<BlockTypeDataId> LazyDataCompBlockIdA = new(() => new());
    private static readonly Lazy<BlockTypeDataId> LazyDataCompBlockIdB = new(() => new());
    private static BlockTypeDataId DataCompBlockIdA => LazyDataCompBlockIdA.Value;
    private static BlockTypeDataId DataCompBlockIdB => LazyDataCompBlockIdB.Value;
    public static OperationDefinition EntityRefEq = new(TokenType.DoubleEquals, HlfType.Bool, HlfType.Entity, (gen, a, b, result) =>
    {
        return gen.Comment($"Comparing entity with tag '{a.Generate(gen)}' to entity with tag '{b.Generate(gen)}' by UUID\n") +
               $"setblock {DataCompBlockIdA.Generate(gen)} minecraft:barrel\n" +
               $"setblock {DataCompBlockIdB.Generate(gen)} minecraft:barrel\n" +
               $"data modify block {DataCompBlockIdA.Generate(gen)} Items set value [{{id:\"paper\", \"components\":{{\"minecraft:custom_data\":{{\"uuid\":[]}}}}}}]\n" +
               $"data modify block {DataCompBlockIdB.Generate(gen)} Items set value [{{id:\"paper\", \"components\":{{\"minecraft:custom_data\":{{\"uuid\":[]}}}}}}]\n" +
               $"data modify block {DataCompBlockIdA.Generate(gen)} Items[0].components.\"minecraft:custom_data\".uuid set from entity @e[tag={a.Generate(gen)}, limit=1] UUID\n" +
               $"data modify block {DataCompBlockIdB.Generate(gen)} Items[0].components.\"minecraft:custom_data\".uuid set from entity @e[tag={b.Generate(gen)}, limit=1] UUID\n" +
               $"execute store success storage {gen.StorageNamespace} {result.Generate(gen)} byte 1 if blocks {DataCompBlockIdA.Generate(gen)} {DataCompBlockIdA.Generate(gen)} {DataCompBlockIdB.Generate(gen)} all";
    });
}

public class Conversion(HlfType resultType, ConversionGenerator generator)
{
    public HlfType ResultType = resultType;
    public ConversionGenerator Generator = generator;
}

public class OperationDefinition(TokenType @operator, HlfType resultType, HlfType otherType, OperationGenerator generator)
{
    public TokenType Operator = @operator;
    public HlfType ResultType = resultType;
    public HlfType OtherType = otherType;
    public OperationGenerator Generator = generator;
}
public delegate string ConversionGenerator(GeneratorOptions gen, DataId from, DataId to);
public delegate string OperationGenerator(GeneratorOptions gen, DataId a, DataId b, DataId result);
