using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.Values;

namespace Hlf.Transpiler;

public class MemberGetter : Statement
{
    private Member Member;
    public string MemberName;
    public Statement BaseExpression;
    public override void Parse()
    {
        BaseExpression.Parse();
        
        // Resolve member
        if (!BaseExpression.Result.Type.Members.TryGetValue(MemberName, out Member!))
            throw new LanguageException($"Type '{BaseExpression.Result.Type.Name}' does not have a property called '{MemberName}'", Line, Column);
        
        Result = DataId.FromType(Member.Type);
    }

    public override string Generate(GeneratorOptions gen)
    {
        return $"{BaseExpression.Generate(gen)}\n" +
               $"{Member.GetterGenerator.Invoke(gen, BaseExpression.Result, Result)}";
    }
}