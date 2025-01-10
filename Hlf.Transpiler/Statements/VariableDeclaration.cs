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
        if (TypeName is "var" or "val")
        {
            if (Assignment == null) throw new LanguageException($"Type of variable '{VariableName}' cannot be inferred because assignment is missing.", Line, Column, length:3);
            type = Assignment.GetExpressionType();
        }
        else
        {
            type = HlfType.Types.FirstOrDefault(x => x.Name == TypeName);
            if (type is null) throw new LanguageException($"Cannot declare a variable of type {TypeName} because that type does not exist.", Line, Column, TypeName.Length);
        }
        
        if (ParentScope.Variables.ContainsKey(VariableName))
            throw new LanguageException($"Cannot declare a variable with name '{VariableName}' because a variable with that name is already declared in the current scope.", Line, Column);

        DataId variable = ParentScope.AddVariable(VariableName, type);
        if (TypeName == "val") variable.IsImmutable = true;
        
        Assignment?.Parse();
    }

    public override string Generate(GeneratorOptions options)
    {
        string code = "";
        if (type!.Kind == ValueKind.Block) code = "";
        return $"{code}\n{Assignment?.Generate(options)}";
    }
}