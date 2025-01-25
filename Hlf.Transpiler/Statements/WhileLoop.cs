using System.Text;
using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.DatapackGen;

namespace Hlf.Transpiler;

public class WhileLoop : Loop
{
    public Statement Condition;

    public override bool NeedsSemicolon => false;

    public override void Parse()
    {
        Condition.Parse();
        Block.ForEach(statement => statement.Parse());
        if (!Condition.Result.Type.IsAssignableTo(HlfType.Bool))
            throw new LanguageException($"Condition expression of while-statement should be of type {HlfType.Bool.Name} but is of type {Condition.Result!.Type.Name}", Condition.Line, Condition.Column);
        base.InitializeControlFlow();
    }

    internal static int loopCounter = 0;
    public override string Generate(GeneratorOptions gen)
    {

        loopFunctionName = $"while{loopCounter++}";
        
        DataId conditionVal = Condition.Result.ConvertImplicitly(gen, HlfType.Bool, out string conditionConversionCode);
        string conditionEvalCode = Condition.Generate(gen) + "\n" + conditionConversionCode;
        string conditionalFuncCallCode = $"execute if data storage {gen.StorageNamespace} {{{conditionVal.Generate(gen)}:1b}} run function {gen.DatapackNamespace}:{loopFunctionName}";
        
        StringBuilder startupSb = new();
        startupSb.SmartAppendL(PrepareGenForControlFlow(gen));
        startupSb.SmartAppendL(conditionEvalCode);
        startupSb.SmartAppendL(conditionalFuncCallCode);
        
        StringBuilder loopSb = new();
        loopSb.AppendCommands(LoopScope, string.Join('\n', Block.Select(x => x.Generate(gen))));
        
        loopSb.AppendLine();
        
        loopSb.SmartAppendL(gen.Comment($"Evaluating condition for while-loop in line {Line}"));
        loopSb.SmartAppendL(conditionEvalCode);

        if(HasControlFlowStatements){
            loopSb.SmartAppendL(gen.Comment($"Recursively call the function again unless break-statement was hit (indicated by {ControlFlowDataId!.Generate(gen)} being 1b)"));
            loopSb.SmartAppendL(ResetControlFlowState(gen));
            loopSb.SmartAppendL($"execute unless data storage {gen.StorageNamespace} {{{ControlFlowDataId!.Generate(gen)}:1b}} run {conditionalFuncCallCode}");
        }
        else
            loopSb.AppendCommands(LoopScope, conditionalFuncCallCode); // Recursive call in loop here
        
        StringBuilder endSb = new();
        endSb.AppendLine(gen.Comment($"Cleaning up after while loop in line {Line}"));
        endSb.SmartAppendL(LoopScope.GenerateScopeDeallocation(gen));
        endSb.SmartAppendL(conditionVal.FreeIfTemporary(gen));
        if(ControlFlowDataId != null) endSb.SmartAppendL(ControlFlowDataId.Free(gen));
        
        // Finish loop execution cleanly by running cleanup if condition is no longer met
        loopSb.AppendWithPrefix($"execute unless data storage {gen.StorageNamespace} {{{conditionVal.Generate(gen)}:1b}} run", endSb.ToString());
        if(ControlFlowDataId != null)
        {
            // Also run cleanup when the loop was stopped by a break statement
            loopSb.AppendWithPrefix($"execute if data storage {gen.StorageNamespace} {{{ControlFlowDataId.Generate(gen)}:1b}} run", endSb.ToString());
        }
        
        gen.ExtraFunctionsToGenerate.Add((loopFunctionName, loopSb.ToString()));


        return startupSb.ToString();
    }
}