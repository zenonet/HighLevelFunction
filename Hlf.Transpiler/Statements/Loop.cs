using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public abstract class Loop : Statement
{
    public bool HasControlFlowStatements;
    public NbtDataId? ControlFlowDataId;
    public List<Statement> Block;
    public Scope LoopScope;
    protected string ControlFlowConditionalPrefix = "";

    protected string loopFunctionName;

    protected void InitializeControlFlow()
    {
        if (!HasControlFlowStatements) return; 
        
        ControlFlowDataId = (NbtDataId) DataId.FromType(HlfType.Bool);
    }

    protected string PrepareGenForControlFlow(GeneratorOptions gen)
    {
        if (ControlFlowDataId == null) return "";

        ControlFlowConditionalPrefix = $"execute if data storage {gen.StorageNamespace} {{{ControlFlowDataId.Generate(gen)}:0b}} run";
        LoopScope.ConditionPrefix = ControlFlowConditionalPrefix;
        return $"data modify storage {gen.StorageNamespace} {ControlFlowDataId.Generate(gen)} set value 0b";
    }
    
    protected string ResetControlFlowState(GeneratorOptions gen)
    {
        if (ControlFlowDataId == null) return "";
        return $"execute unless data storage {gen.StorageNamespace} {{{ControlFlowDataId.Generate(gen)}:1b}} run data modify storage {gen.StorageNamespace} {ControlFlowDataId!.Generate(gen)} set value 0b";
    }
}