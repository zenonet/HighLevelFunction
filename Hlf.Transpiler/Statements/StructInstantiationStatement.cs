using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class StructInstantiationStatement : Statement
{
    public Token StructName;
    public override void Parse()
    {
        HlfType? structType = ParentScope.Types.FirstOrDefault(x => x.Name == StructName.Content);

        if (structType == null) throw Utils.TypeDoesNotExistError(StructName);
        
        Result = DataId.FromType(structType);
    }

    public override string Generate(GeneratorOptions gen)
    {
        return Result.Type.Kind switch
        {
            ValueKind.Nbt => $"data modify storage {gen.StorageNamespace} {Result.Generate(gen)} set value {{}}",
            _ => throw new LanguageException($"Cannot instantiate Type {Result.Type.Name}", StructName)
        };
    }
}