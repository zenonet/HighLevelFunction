using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler.Values;

public class Member
{
    public required string Name;
    public required HlfType Type;

    public required GetterGenerator GetterGenerator;
    public SetterGenerator? SetterGenerator;
}

public delegate string GetterGenerator(GeneratorOptions gen, DataId baseValue, DataId resultId);
public delegate string SetterGenerator(GeneratorOptions gen, DataId baseValue, DataId value);