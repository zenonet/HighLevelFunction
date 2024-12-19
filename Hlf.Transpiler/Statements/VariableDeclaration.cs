using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class VariableDeclaration : Statement
{
    public string TypeName { get; set; }
    public string VariableName { get; set; }
    public VariableAssignment? Assignment;
    HlfType? type;
    public override void Parse()
    {
        type = HlfType.Types.FirstOrDefault(x => x.Name == TypeName);
        if (type is null) throw new LanguageException($"Cannot declare a variable of type {TypeName} because that type does not exist.", Line, Column, TypeName.Length);
        if (ParentScope.Variables.ContainsKey(VariableName))
            throw new LanguageException($"Cannot declare a variable with name '{VariableName}' because a variable with that name is already declared in the current scope.", Line, Column);

        ParentScope.AddVariable(VariableName, type);

        Assignment?.Parse();
    }

    public override string Generate(GeneratorOptions options)
    {
        string code = "";
        if (type.Kind == ValueKind.Block) code = "";
        return $"{code}\n{Assignment?.Generate(options)}";
    }
}