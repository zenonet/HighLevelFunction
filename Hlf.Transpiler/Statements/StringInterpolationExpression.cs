using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class StringInterpolationExpression : Statement
{
    public List<Statement> ContentParts;
    public List<Token> StringPartTokens;
    public override void Parse()
    {
        Result = DataId.FromType(HlfType.String);
        foreach (Statement contentPart in ContentParts)
        {
            contentPart.Parse();
            if (contentPart.Result.Type != HlfType.Int && contentPart.Result.Type != HlfType.Float && !contentPart.Result.Type.IsAssignableTo(HlfType.String)) throw new LanguageException($"Content part of type '{contentPart.Result.Type.Name}' in string interpolation cannot be implicitly converted into a string", contentPart.Line, contentPart.Column);
        }
    }

    internal static int InterpolationMacroFunctionCounter = 0; 
    public override string Generate(GeneratorOptions gen)
    {
        if (gen.TargetVersion < McVersion.OneDot(20, 2)) throw new LanguageException("String interpolation is not supported in target versions below 1.20.2", Line, Column, StringPartTokens[0].Content.Length);
        
        // Generate macro function
        string macro = $"$data modify storage {gen.StorageNamespace} {Result.Generate(gen)} set value \"{StringPartTokens[0].Content[2..^1]}";
        for (int i = 0; i < ContentParts.Count; i++)
            macro += $"$(p{i}){StringPartTokens[i+1].Content[1..^1]}";
        macro += "\"";
        string macroName = $"interpolation{InterpolationMacroFunctionCounter++}";
        gen.ExtraFunctionsToGenerate.Add((macroName, macro));
        
        // Evaluate and convert content parts
        StringBuilder sb = new();
        var dataIds = ContentParts.Select(part =>
        {
            sb.SmartAppendL(part.Generate(gen));
            if (part.Result.Type == HlfType.Int || part.Result.Type == HlfType.Float)
            {
                // Ints and floats can be interpolated without problems so just don't convert them.
                return part.Result;
            }
            
            var newId = part.Result.ConvertImplicitly(gen, HlfType.String, out string code);
            sb.SmartAppendL(code);
            return newId;
        }).ToArray();
        
        // copy dataids into compound for access from macro
        for (int i = 0; i < dataIds.Length; i++)
        {
            sb.SmartAppendL(gen.CopyDataToData(dataIds[i].Generate(gen),  $"interpolationCompound.p{i}"));
        }
        
        // Call macro
        sb.SmartAppendL($"function {gen.DatapackNamespace}:{macroName} with storage {gen.StorageNamespace} interpolationCompound");
        sb.Append($";fr:data remove storage {gen.StorageNamespace} interpolationCompound");
        return sb.ToString();
    }
}