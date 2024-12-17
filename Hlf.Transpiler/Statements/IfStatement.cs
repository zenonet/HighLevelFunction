using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class IfStatement : Statement
{
    public override bool NeedsSemicolon => false;
    public List<Statement> Block;
    public List<Statement>? ElseBlock;
    public Statement Condition;
    public Scope IfClauseScope;
    public Scope? ElseClauseScope;
    public override void Parse()
    {
        Condition.Parse();
        Block.ForEach(statement => statement.Parse());
        ElseBlock?.ForEach(statement => statement.Parse());
        if (!Condition.Result!.Type.IsAssignableTo(HlfType.Bool))
            throw new LanguageException($"Condition expression of if-statement should be of type {HlfType.Bool.Name} but is of type {Condition.Result!.Type.Name}", Line, Column);
    }

    public override string Generate(GeneratorOptions gen)
    {
        StringBuilder sb = new();

        sb.AppendCommands(ParentScope, gen.Comment($"Evaluating condition of if-statement in line {Line}:\n"));
        DataId condition = Condition.Result!.ConvertImplicitly(gen, HlfType.Bool, out string conversionCode);
        sb.AppendCommands(ParentScope, $"{Condition.Generate(gen)}\n" +
                                       $"{conversionCode}");
        sb.AppendCommands(ParentScope, gen.Comment($"If block code for if-statement in line {Line} (And entering scope at depth {IfClauseScope.Depth}):\n"));
        IfClauseScope.ConditionPrefix = $"execute if data storage {gen.StorageNamespace} {{{condition.Generate(gen)}:1b}} run";
        Block.ForEach(x => sb.AppendCommands(IfClauseScope, x.Generate(gen)));
        sb.AppendCommands(ParentScope, IfClauseScope.GenerateScopeDeallocation(gen));

        if (ElseBlock != null)
        {
            sb.AppendCommands(ParentScope, gen.Comment($"Else block code for if-statement in line {Line} (And entering scope at depth {ElseClauseScope!.Depth}):\n"));
            ElseClauseScope!.ConditionPrefix = $"execute unless data storage {gen.StorageNamespace} {{{condition.Generate(gen)}:1b}} run";
            ElseBlock.ForEach(x => sb.AppendCommands(ElseClauseScope, x.Generate(gen)));
            sb.AppendCommands(ParentScope, ElseClauseScope.GenerateScopeDeallocation(gen));
        }
        sb.AppendCommands(ParentScope, condition.FreeIfTemporary(gen));
        

        return sb.ToString();
    }
}