using System.Text;
using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.DatapackGen;

namespace Hlf.Transpiler;

public class ForLoop : Loop
{
    public Statement? InitStatement;
    public Statement? Condition;
    public Statement? Increment;
    public Scope HeaderScope;
    
    public override bool NeedsSemicolon => false;

    public override void Parse()
    {
        InitStatement?.Parse();
        Block.ForEach(x => x.Parse());
        Condition?.Parse();
        Increment?.Parse();
        
        if (Condition != null && !Condition.Result.Type.IsAssignableTo(HlfType.Bool))
            throw new LanguageException($"Condition expression of for-loop should be of type {HlfType.Bool.Name} but is of type {Condition.Result.Type.Name}", Condition.Line, Condition.Column);
    }

    private static int loopCounter = 0;
    public override string Generate(GeneratorOptions gen)
    {
        StringBuilder sb = new();
        string funcName = $"for{loopCounter++}";
        
        // TODO: Make condition optional
        DataId conditionVal = Condition.Result.ConvertImplicitly(gen, HlfType.Bool, out string conditionConversionCode);
        string conditionEvalCode = Condition.Generate(gen) + "\n" + conditionConversionCode;
        string conditionalFuncCallCode = $"execute if data storage {gen.StorageNamespace} {{{conditionVal.Generate(gen)}:1b}} run function {gen.DatapackNamespace}:{funcName}";
        
        sb.AppendCommands(ParentScope, PrepareGenForControlFlow(gen));
        sb.AppendCommands(LoopScope, string.Join('\n', Block.Select(x => x.Generate(gen))));
        if (Increment != null)
        {
            sb.AppendLine('\n' + gen.Comment("Increment statement in forloop:"));
            sb.AppendCommands(HeaderScope, Increment.Generate(gen));
        }
        sb.AppendLine('\n' + gen.Comment("Evaluating condition for forloop:"));
        sb.AppendCommands(LoopScope, conditionEvalCode);
        sb.AppendCommands(LoopScope, conditionalFuncCallCode); // Recursive call in loop here

        gen.ExtraFunctionsToGenerate.Add((funcName, sb.ToString()));

        sb = new();
        if(InitStatement != null) sb.AppendCommands(HeaderScope, InitStatement.Generate(gen));
        sb.AppendCommands(HeaderScope, conditionEvalCode);
        sb.AppendCommands(HeaderScope, conditionalFuncCallCode);
        return sb.ToString();
    }
}