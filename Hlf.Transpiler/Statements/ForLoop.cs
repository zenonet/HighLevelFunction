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
        
        base.InitializeControlFlow();
    }

    internal static int loopCounter = 0;
    public override string Generate(GeneratorOptions gen)
    {
        loopFunctionName = $"for{loopCounter++}";
        
        // TODO: Make condition optional
        DataId conditionVal = Condition.Result.ConvertImplicitly(gen, HlfType.Bool, out string conditionConversionCode);
        string conditionEvalCode = Condition.Generate(gen) + "\n" + conditionConversionCode;
        string conditionalFuncCallCode = $"execute if data storage {gen.StorageNamespace} {{{conditionVal.Generate(gen)}:1b}} run function {gen.DatapackNamespace}:{loopFunctionName}";
        
        StringBuilder initSb = new();
        initSb.SmartAppendL(PrepareGenForControlFlow(gen));
        if(InitStatement != null) initSb.AppendCommands(HeaderScope, InitStatement.Generate(gen));
        initSb.AppendCommands(HeaderScope, conditionEvalCode);
        initSb.AppendCommands(HeaderScope, conditionalFuncCallCode);
        
        
        StringBuilder loopSb = new();
        loopSb.AppendCommands(LoopScope, string.Join('\n', Block.Select(x => x.Generate(gen))));
        if (Increment != null)
        {
            loopSb.AppendLine('\n' + gen.Comment("Increment statement in forloop:"));
            loopSb.AppendCommands(HeaderScope, Increment.Generate(gen));
        }
        loopSb.AppendLine('\n' + gen.Comment("Evaluating condition for forloop:"));
        
        loopSb.SmartAppendL(conditionEvalCode);
        if (HasControlFlowStatements)
        {
            loopSb.SmartAppendL(gen.Comment($"Recursively call the function again unless break-statement was hit (indicated by {ControlFlowDataId!.Generate(gen)} being 1b)"));
            loopSb.SmartAppendL(ResetControlFlowState(gen));
            loopSb.SmartAppendL($"execute unless data storage {gen.StorageNamespace} {{{ControlFlowDataId!.Generate(gen)}:1b}} run {conditionalFuncCallCode}");
        }
        else
            loopSb.AppendCommands(LoopScope, conditionalFuncCallCode); // Recursive call in loop here

        StringBuilder endSb = new();
        endSb.AppendLine(gen.Comment($"Cleaning up after for loop in line {Line}"));
        endSb.SmartAppendL(LoopScope.GenerateScopeDeallocation(gen));
        endSb.SmartAppendL(conditionVal.FreeIfTemporary(gen));
        if(ControlFlowDataId != null) endSb.SmartAppendL(ControlFlowDataId.Free(gen));
        
        
        loopSb.AppendWithPrefix($"execute unless data storage {gen.StorageNamespace} {{{conditionVal.Generate(gen)}:1b}} run", endSb.ToString());
        if(ControlFlowDataId != null)
        {
            // Also run cleanup when the loop was stopped by a break statement
            loopSb.AppendWithPrefix($"execute if data storage {gen.StorageNamespace} {{{ControlFlowDataId.Generate(gen)}:1b}} run", endSb.ToString());
        }
        
        gen.ExtraFunctionsToGenerate.Add((loopFunctionName, loopSb.ToString()));
        
        return initSb.ToString();
    }
}