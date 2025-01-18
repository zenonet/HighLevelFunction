using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class BuiltinFunctionDefinition(string name, HlfType returnType, CodeGenerator codeGenerator, params ParameterDefinition[] parameters)
{
    public string Name { get; init; } = name;
    public HlfType ReturnType { get; init; } = returnType;
    public ParameterDefinition[] Parameters { get; set; } = parameters;
    public CodeGenerator CodeGenerator { get; init; } = codeGenerator;
    public McVersion MinVersion = McVersion.OneDot(13);
    public string? Description;

    public BuiltinFunctionDefinition WithMinVersion(McVersion minVersion)
    {
        MinVersion = minVersion;
        return this;
    }

    public BuiltinFunctionDefinition WithDescription(string description)
    {
        Description = description;
        return this;
    }
}

public delegate string CodeGenerator(GeneratorOptions options, Dictionary<string, DataId> parameters, DataId resultId);

public class ParameterDefinition(string name, HlfType type)
{
    public string Name { get; set; } = name;
    public HlfType Type { get; set; } = type;
}