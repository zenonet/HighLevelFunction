using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class VariableAssignment : Statement
{
    public string VariableName { get; set; }
    public Statement Expression { get; set; }
    private DataId variableId;

    private bool IsExpressionParsed = false;
    public bool IsInitialization;
    public HlfType GetExpressionType()
    {
        if(!IsExpressionParsed) Expression.Parse();
        if (Expression.Result.Type is {Kind: ValueKind.Constant, ImplicitConversions.Length: > 0})
        {
            // Infer the converted non-constant type if possible
            return Expression.Result.Type.ImplicitConversions[0].ResultType;
        }
        return Expression.Result.Type;
    }
    public override void Parse()
    {
        if(!IsExpressionParsed)
        {
            Expression.Parse();
            IsExpressionParsed = true;
        }

        if (!ParentScope.TryGetVariable(VariableName, out DataId? variable))
            throw new LanguageException($"Trying to assign a value to the undeclared variable {VariableName}.", Line, Column, VariableName.Length);
        if (variable.IsImmutable && !IsInitialization) throw new LanguageException("Cannot reassign an immutable variable", Line, Column);
        variableId = variable;

        if (!Expression.Result.Type.IsAssignableTo(variable.Type))
            throw new LanguageException($"Value of type {Expression.Result.Type.Name} cannot be assigned to variable {VariableName} of type {variable.Type.Name}.", Expression.Line, Expression.Column);
    }

    public override string Generate(GeneratorOptions options)
    {
        DataId dataId = Expression.Result!.ConvertImplicitly(options, variableId.Type, out string conversionCode);
        StringBuilder sb = new();

        sb.SmartAppendL(options.Comment($"Assigning value of type {variableId.Type.Name} to variable {VariableName}{(conversionCode.Length > 0 ? $" and converting it implcitly into a {variableId.Type.Name}" : "")}"));
        sb.SmartAppendL(Expression.Generate(options));
        sb.SmartAppendL(conversionCode);
        sb.SmartAppendL(options.CopyDataId(dataId, variableId));
        
        // Free values:
        sb.AppendCommands(ParentScope, dataId.FreeIfTemporary(options));
        if(Expression.Result != dataId) sb.AppendCommands(ParentScope, Expression.Result.FreeIfTemporary(options));
        
        return sb.ToString();
    }
}