using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class VoidDataId : DataId
{
    public VoidDataId()
    {
        Type = HlfType.Void;
    }
    public override string Generate(GeneratorOptions options)
    {
        throw new NotImplementedException();
    }

    public override string Free(GeneratorOptions gen)
    {
        throw new NotImplementedException();
    }

    public static VoidDataId Void = new();
}