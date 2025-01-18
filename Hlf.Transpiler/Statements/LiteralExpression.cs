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
        if (HlfType == HlfType.Int)
        {
            if (long.Parse(Code) > int.MaxValue) throw new LanguageException("Integer value can't be greater than 2^31-1 = 2147483647", Line, Column, Code.Length);
            if (long.Parse(Code) < int.MinValue) throw new LanguageException("Integer value can't be less than  -2^31 = -2147483648", Line, Column, Code.Length);
        }

        if(Result is VoidDataId)
            Result = HlfType.NewDataId();
    }

    public override string Generate(GeneratorOptions options)
    {
        if(HlfType.Kind == ValueKind.Nbt)
            return $"data modify storage {options.StorageNamespace} {Result.Generate(options)} set value {Code}";
        return "";
    }
}