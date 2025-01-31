using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class FunctionDefinitionStatement : Statement
{
    public string Name;
    public List<Statement> Block = new();
    public Scope FunctionScope = new();
    public List<(string, string)> ParameterDefinitions;
    public override bool NeedsSemicolon => false;

    public override void Parse()
    {
        if (ParentScope.Parent != null) throw new LanguageException("Functions can only be defined in root scope", Line, Column);
        
        List<CustomParameterDefinition> parameters = new();
        foreach ((string? name, string? typeName) in ParameterDefinitions)
        {
            HlfType parameterType = ParentScope.Types.FirstOrDefault(x => x.Name == typeName) ?? throw Utils.TypeDoesNotExistError(typeName, Line, Column);
            parameters.Add(new(name, parameterType, DataId.FromType(parameterType)));
        }
        
        foreach (var paramDef in parameters)
        {
            FunctionScope.Variables.Add(paramDef.Name, paramDef.DataId);
        }
        
        Block.ForEach(statement => statement.Parse());

        
        CustomFunctionDefinition def = new()
        {
            Name = Name,
            McFunctionName = Name.ToLower(),
            Scope = FunctionScope,
            Parameters = parameters,
        };
        ParentScope.FunctionDefinitions.Add(Name, def);
    }

    public override string Generate(GeneratorOptions gen)
    {
        StringBuilder sb = new();
        Block.ForEach(x => sb.AppendCommands(FunctionScope, x.Generate(gen)));
        sb.AppendLine(); // add missing linebreak
        sb.Append(FunctionScope.GenerateScopeDeallocation(gen));
        gen.ExtraFunctionsToGenerate.Add(new(Name, sb.ToString()));
        return "";
    }
}

public class CustomFunctionDefinition
{
    public string Name;
    public string McFunctionName;
    public Scope Scope;
    public List<CustomParameterDefinition> Parameters = new();
    public string GetSignature() => $"void {Name}({string.Join(", ", Parameters.Select(x => $"{x.Type.Name} {x.Name}"))})";
}