using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.Values;

namespace Hlf.Transpiler;

public class MemberGetter : Statement
{
    private Member Member;
    public string MemberName;
    public Statement BaseExpression;
    public sbyte Increment = 0;
    private DataId tempIncrId;
    public override void Parse()
    {
        BaseExpression.Parse();
        
        // Resolve member
        if (!BaseExpression.Result.Type.Members.TryGetValue(MemberName, out Member!))
            throw new LanguageException($"Type '{BaseExpression.Result.Type.Name}' does not have a property called '{MemberName}'", Line, Column);

        if (Increment != 0)
        {
            if (Member.Type != HlfType.Int) throw new LanguageException("Increment/Decrement expression can only be used on variables of type int", Line, Column, MemberName.Length + 2);
            if (Member.SetterGenerator == null) throw new LanguageException($"{BaseExpression.Result.Type.Name}.{MemberName} cannot be incremented because the member does not have a setter", Line, Column, MemberName.Length+2);
            tempIncrId = DataId.FromType(Member.Type);
        }
        
        Result = DataId.FromType(Member.Type);
    }

    public override string Generate(GeneratorOptions gen)
    {
        return $"{BaseExpression.Generate(gen)}\n" +
               $"{Member.GetterGenerator.Invoke(gen, BaseExpression.Result, Result)}" + 
               (Increment == 0 
                   ? "" 
                   : $"\n{gen.CopyDataToScoreboard(Result.Generate(gen), "a")}\n" +
                     $"scoreboard players {(Increment == -1 ? "remove" : "add")} a {gen.Scoreboard} 1\n" +
                     $"{gen.CopyScoreToData("a", tempIncrId.Generate(gen))}\n" +
                     $"{Member.SetterGenerator!.Invoke(gen, BaseExpression.Result, tempIncrId)}"
               );
    }
}