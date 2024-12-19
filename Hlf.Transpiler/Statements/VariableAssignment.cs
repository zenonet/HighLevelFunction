using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class VariableAssignment : Statement
{
    public string VariableName { get; set; }
    public Statement Expression { get; set; }
    private DataId variableId;

    public override void Parse()
    {
        Expression.Parse();

        if (!ParentScope.TryGetVariable(VariableName, out DataId? variable))
            throw new LanguageException($"Trying to assign a value to the undeclared variable {VariableName}.", Line, Column, VariableName.Length);
        variableId = variable;

        if (!Expression.Result.Type.IsAssignableTo(variable.Type))
            throw new LanguageException($"Value of type {Expression.Result.Type.Name} cannot be assigned to variable {VariableName} of type {variable.Type.Name}.", Expression.Line, Expression.Column);
    }

    public override string Generate(GeneratorOptions options)
    {
        DataId dataId = Expression.Result!.ConvertImplicitly(options, variableId.Type, out string conversionCode);
        StringBuilder sb = new();

        sb.AppendCommands(ParentScope, options.Comment($"Assigning value of type {variableId.Type.Name} to variable {VariableName}{(conversionCode.Length > 0 ? $" and converting it implcitly into a {variableId.Type.Name}" : "")}\n"));
        sb.AppendCommands(ParentScope, Expression.Generate(options));
        sb.AppendCommands(ParentScope, conversionCode);
        sb.AppendCommands(ParentScope, variableId.Type.Kind switch
        {
            ValueKind.Nbt => $"data modify storage {options.StorageNamespace} {variableId.Generate(options)} set from storage {options.StorageNamespace} {dataId.Generate(options)}",
            ValueKind.Block => $"clone {dataId.Generate(options)} {dataId.Generate(options)} {variableId.Generate(options)}",
            ValueKind.EntityTag => $"tag @e[tag={dataId.Generate(options)}] add {variableId.Generate(options)}",
            _ => throw new NotImplementedException($"Variable assignment is not implemented for type {variableId.Type.Name}")
        });
        
        // Free values:
        sb.AppendCommands(ParentScope, dataId.FreeIfTemporary(options));
        sb.AppendCommands(ParentScope, Expression.Result.FreeIfTemporary(options));
        
        return sb.ToString();
    }
}