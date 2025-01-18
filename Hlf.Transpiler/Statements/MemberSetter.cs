using System.Text;
using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.Values;

namespace Hlf.Transpiler;

public class MemberSetter : Statement
{
    public string MemberName;
    public Statement Expression;
    public Statement BaseExpression;
    private Member member;
    public override void Parse()
    {
        BaseExpression.Parse();
        Expression.Parse();

        if (!BaseExpression.Result.Type.Members.TryGetValue(MemberName, out member!))
            throw new LanguageException($"Type '{BaseExpression.Result.Type.Name}' does not have a property called '{MemberName}'", Line, Column);

        if (member.SetterGenerator == null)
            throw new LanguageException($"Property '{MemberName}' in type {BaseExpression.Result.Type.Name} does not have a settter", Line, Column);
        
        if (!Expression.Result.Type.IsAssignableTo(member.Type))
            throw new LanguageException($"Value of type {Expression.Result.Type.Name} cannot be assigned to property {BaseExpression.Result.Type.Name}.{MemberName} of type {member.Type}.", Expression.Line, Expression.Column);
    }

    public override string Generate(GeneratorOptions gen)
    {
        StringBuilder sb = new();
        sb.SmartAppendL(gen.Comment($"Assigning property '{MemberName}' in type {BaseExpression.Result.Type.Name}"));
        sb.SmartAppendL($"{BaseExpression.Generate(gen)}");
        sb.SmartAppendL($"{Expression.Generate(gen)}");
        sb.SmartAppend($"{member.SetterGenerator!.Invoke(gen, BaseExpression.Result, Expression.Result)}");
        return sb.ToString();
    }
}