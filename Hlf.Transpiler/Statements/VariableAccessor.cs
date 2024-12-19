using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class VariableAccessor : Statement
{
    public string VariableName;
    public override void Parse()
    {
        if (ParentScope.TryGetVariable(VariableName, out DataId? dataId))
        {
            Result = dataId;
        }
        else
        {
            throw new LanguageException($"Variable '{VariableName}' does not exist in the current scope", Line, Column, VariableName.Length);
        }
    }

    public override string Generate(GeneratorOptions options)
    {
        return "";
    }
}