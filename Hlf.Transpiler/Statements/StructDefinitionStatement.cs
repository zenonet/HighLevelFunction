﻿using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.Values;

namespace Hlf.Transpiler;

public class StructDefinitionStatement : Statement
{
    public string StructName;
    public List<(Token fieldName, Token fieldType, Statement? initializer)> Fields;
    public override bool NeedsSemicolon => false;

    public override void Parse()
    {
        StructType type = new (StructName, ValueKind.Nbt);
        type.Members = new();
        foreach ((Token fieldNameToken, Token fieldTypeToken, Statement? initializer) in Fields)
        {
            HlfType? fieldType = ParentScope.Types.FirstOrDefault(x => x.Name == fieldTypeToken.Content);
            if (fieldType == null) throw Utils.TypeDoesNotExistError(fieldTypeToken);
            
            Member member;
            switch (fieldType.Kind)
            {
                case ValueKind.Nbt:
                    member = new()
                    {
                        Type = fieldType,
                        GetterGenerator = (gen, self, resultId) =>
                            $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from storage {gen.StorageNamespace} {self.Generate(gen)}.{fieldNameToken.Content}",
                        SetterGenerator = (gen, self, value) =>
                            $"data modify storage {gen.StorageNamespace} {self.Generate(gen)}.{fieldNameToken.Content} set from storage {gen.StorageNamespace} {value.Generate(gen)}",
                    };
                    break;
                default:
                    throw new LanguageException($"Type '{fieldType.Name}' cannot be used as a field of a struct.", fieldTypeToken);
            }

            if (initializer != null)
            {
                initializer.Parse();
                if (!initializer.Result.Type.IsAssignableTo(fieldType)) throw new LanguageException($"Initialization expression is of type '{initializer.Result.Type.Name}' and can't be assigned to field '{fieldNameToken.Content}' of type '{fieldType.Name}'", initializer.Line, initializer.Column);
                type.Initializers.Add((member, fieldNameToken.Content, initializer));
            }
            
            type.Members.Add(fieldNameToken.Content, member);
        }
        
        ParentScope.CustomTypes.Add(type);
    }

    public override string Generate(GeneratorOptions options)
    {
        return "";
    }
}