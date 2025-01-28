using System.Text;
using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.Values;

namespace Hlf.Transpiler;

public class StructInstantiationStatement : Statement
{
    public Token StructName;
    public override void Parse()
    {
        HlfType? structType = ParentScope.Types.FirstOrDefault(x => x.Name == StructName.Content);

        if (structType == null) throw Utils.TypeDoesNotExistError(StructName);

        if (structType is not StructType) throw new LanguageException("The new-keyword can only be used to instantiate struct types.", StructName);
        
        Result = DataId.FromType(structType);
    }

    public override string Generate(GeneratorOptions gen)
    {
        switch (Result.Type.Kind)
        {
            case ValueKind.Nbt:
                StringBuilder sb = new();
                sb.SmartAppendL(gen.Comment($"Instantiating struct '{StructName}':"));
                sb.SmartAppend($"data modify storage {gen.StorageNamespace} {Result.Generate(gen)} set value {{}}");

                foreach ((Member? member, string memberName, Statement? initializer) in ((StructType)Result.Type).Initializers)
                {
                    sb.AppendLine();
                    sb.SmartAppendL(gen.Comment($"Initializing struct field '{StructName.Content}.{memberName}'"));
                    sb.SmartAppendL(initializer.Generate(gen));
                    DataId value = initializer.Result.ConvertImplicitly(gen, member.Type, out string code);
                    sb.SmartAppendL(code);
                    sb.SmartAppend(gen.CopyDataToData(value.Generate(gen), $"{Result.Generate(gen)}.{memberName}"));
                }
                
                return sb.ToString();
            default:                
                throw new LanguageException($"Cannot instantiate Type {Result.Type.Name}", StructName);
        }
    }
}