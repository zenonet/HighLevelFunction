using System.Diagnostics.CodeAnalysis;
using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.Values;

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
            OperationImplementations.IntScoreboardOperation(TokenType.Plus, "+="),
            OperationImplementations.IntScoreboardOperation(TokenType.Minus, "-="),
            OperationImplementations.IntScoreboardOperation(TokenType.Asterisk, "*="),
            OperationImplementations.IntScoreboardOperation(TokenType.Slash, "/="),
            OperationImplementations.IntCompOperation(TokenType.GreaterThan, ">"),
            OperationImplementations.IntCompOperation(TokenType.LessThan, "<"),
            OperationImplementations.IntCompOperation(TokenType.DoubleEquals, "="),
            OperationImplementations.IntCompOperation(TokenType.GreaterThanOrEqual, ">="),
            OperationImplementations.IntCompOperation(TokenType.LessThanOrEqual, "<="),
        ];

        // Yeah, as of now, floats are actually a scam and are actually fixed-point-numbers
        Float.Operations =
        [
            OperationImplementations.FixedScoreboardOperation(TokenType.Plus, "+="),
            OperationImplementations.FixedScoreboardOperation(TokenType.Minus, "-="),
            OperationImplementations.FixedScoreboardOperation(TokenType.Asterisk, "*=", "0.000001"),
            OperationImplementations.FixedScoreboardOperation(TokenType.Slash, "/=", "0.001", "1000000", "1000"),
            OperationImplementations.FixedCompOperation(TokenType.GreaterThan, ">"),
            OperationImplementations.FixedCompOperation(TokenType.LessThan, "<"),
            OperationImplementations.FixedCompOperation(TokenType.DoubleEquals, "="),
            OperationImplementations.FixedCompOperation(TokenType.GreaterThanOrEqual, ">="),
            OperationImplementations.FixedCompOperation(TokenType.LessThanOrEqual, "<="),
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

        Vector.Members = new()
        {
            {
                "x", new()
                {
                    Type = Float,
                    GetterGenerator = (gen, value, resultId) => $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from storage {gen.StorageNamespace} {value.Generate(gen)}[0]",
                    SetterGenerator = (gen, value, valueId) => $"data modify storage {gen.StorageNamespace} {value.Generate(gen)}[0] set from storage {gen.StorageNamespace} {valueId.Generate(gen)}",
                }
            },            {
                "y", new()
                {
                    Type = Float,
                    GetterGenerator = (gen, value, resultId) => $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from storage {gen.StorageNamespace} {value.Generate(gen)}[1]",
                    SetterGenerator = (gen, value, valueId) => $"data modify storage {gen.StorageNamespace} {value.Generate(gen)}[1] set from storage {gen.StorageNamespace} {valueId.Generate(gen)}",
                }
            },            {
                "z", new()
                {
                    Type = Float,
                    GetterGenerator = (gen, value, resultId) => $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from storage {gen.StorageNamespace} {value.Generate(gen)}[2]",
                    SetterGenerator = (gen, value, valueId) => $"data modify storage {gen.StorageNamespace} {value.Generate(gen)}[2] set from storage {gen.StorageNamespace} {valueId.Generate(gen)}",
                }
            },
        };
        
        Entity.Members = new()
        {
            {
                "Position", new ()
                {
                    Type = Vector,
                    GetterGenerator = (gen, self, resultId) => $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from entity @e[tag={self.Generate(gen)}, limit=1] Pos",
                    SetterGenerator = (gen, self, valueId) => $"data modify entity @e[tag={self.Generate(gen)}, limit=1] Pos set from storage {gen.StorageNamespace} {valueId.Generate(gen)}",
                }
            },
            {
                "Motion", new ()
                {
                    Type = Vector,
                    GetterGenerator = (gen, self, resultId) => $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from entity @e[tag={self.Generate(gen)}, limit=1] Motion",
                    SetterGenerator = (gen, self, valueId) => $"data modify entity @e[tag={self.Generate(gen)}, limit=1] Motion set from storage {gen.StorageNamespace} {valueId.Generate(gen)}",
                }
            },
        };
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

    public Dictionary<string, Member> Members;
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
    public static OperationDefinition IntScoreboardOperation(TokenType hlfOperator, string mcOperator) => new(hlfOperator, HlfType.Int, HlfType.Int, (gen, a, b, result) =>
        $"{gen.CopyDataToScoreboard(a.Generate(gen), "a")}\n" +
               $"{gen.CopyDataToScoreboard(b.Generate(gen), "b")}\n" +
               $"{gen.ScoreboardOpIntoA("a", "b", mcOperator)}\n" +
               $"{gen.CopyScoreToData("a", result.Generate(gen))}");
    
    public static OperationDefinition IntCompOperation(TokenType hlfOperator, string mcOperator) => new(hlfOperator, HlfType.Bool, HlfType.Int, (gen, a, b, result) =>
            gen.Comment("Comparing 2 ints via scoreboard\n") +
                   $"{gen.CopyDataToScoreboard(a.Generate(gen), "a")}\n" +
                   $"{gen.CopyDataToScoreboard(b.Generate(gen), "b")}\n" +
                   $"execute store success storage {gen.StorageNamespace} {result.Generate(gen)} byte 1 if score a hlf {mcOperator} b hlf");
    
    public static OperationDefinition FixedCompOperation(TokenType hlfOperator, string mcOperator) => new(hlfOperator, HlfType.Bool, HlfType.Float, (gen, a, b, result) =>
            gen.Comment("Comparing 2 ints via scoreboard\n") +
                   $"{gen.CopyDataToScoreboard(a.Generate(gen), "a", FixedPointScale)}\n" +
                   $"{gen.CopyDataToScoreboard(b.Generate(gen), "b", FixedPointScale)}\n" +
                   $"execute store success storage {gen.StorageNamespace} {result.Generate(gen)} byte 1 if score a hlf {mcOperator} b hlf");
    
    public static OperationDefinition BlockTypeEq = new(TokenType.DoubleEquals, HlfType.Bool, HlfType.BlockType, (gen, a, b, result) =>
    {
        return gen.Comment("Comparing 2 blocks by type\n") +
               $"execute store result storage {gen.StorageNamespace} {result.Generate(gen)} byte 1 if blocks {a.Generate(gen)} {a.Generate(gen)} {b.Generate(gen)} all\n";
    });

    private const string FixedPointScale = "1000";
    private const string InverseFixedPointScale = "0.001";
    public static OperationDefinition FixedScoreboardOperation(TokenType hlfOperator, string mcOperator, string outputScale = InverseFixedPointScale, string scaleA = FixedPointScale, string scaleB = FixedPointScale) => new(hlfOperator, HlfType.Float, HlfType.Float, (gen, a, b, result) =>
    $"{gen.CopyDataToScoreboard(a.Generate(gen), "a", scaleA)}\n" +
    $"{gen.CopyDataToScoreboard(b.Generate(gen), "b", scaleB)}\n" +
    $"{gen.ScoreboardOpIntoA("a", "b", mcOperator)}\n" +
    $"{gen.CopyScoreToData("a", result.Generate(gen), outputScale, type:"double")}");

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
