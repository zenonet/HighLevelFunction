using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class LiteralExpression : Statement
{
    public string Code;
    public HlfType HlfType;

    public LiteralExpression(HlfType hlfType, string code)
    {
        Code = code;
        HlfType = hlfType;
    }

    public LiteralExpression(DataId dataId)
    {
        Code = "";
        HlfType = dataId.Type;
        Result = dataId;
    }

    public override void Parse()
    {
        Result ??= HlfType.NewDataId();
    }

    public override string Generate(GeneratorOptions options)
    {
        if(HlfType.Kind == ValueKind.Nbt)
            return $"data modify storage {options.StorageNamespace} {Result.Generate(options)} set value {Code}";
        return "";
    }
}