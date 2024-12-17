using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class ConstDataId : DataId
{
    public string Code;

    public ConstDataId(HlfType type, string code)
    {
        Code = code;
        Type = type;
    }
    public override string Generate(GeneratorOptions options)
    {
        return Code;
    }

    public override string Free(GeneratorOptions gen) => "";
}