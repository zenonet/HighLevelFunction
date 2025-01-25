using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class BreakStatement : Statement
{
    public ControlFlowStatementType Type;
    public override void Parse()
    {
        if (ParentScope.ClosestLoop is not { } loop) throw new LanguageException("Break statement can only be used in a loop", Line, Column, length:5);
        loop.HasControlFlowStatements = true;
    }

    public override string Generate(GeneratorOptions gen)
    {
        return Type switch
        {
            ControlFlowStatementType.Break => 
                gen.Comment($"Break from this loop (break-statement in line {Line}):\n") + 
                $"data modify storage {gen.StorageNamespace} {ParentScope.ClosestLoop!.ControlFlowDataId!.Generate(gen)} set value 1b",
            ControlFlowStatementType.Continue =>
                gen.Comment($"Continue with next loop iteration (continue-statement in line {Line}):\n") + 
                $"data modify storage {gen.StorageNamespace} {ParentScope.ClosestLoop!.ControlFlowDataId!.Generate(gen)} set value 2b",
            _ => throw new NotImplementedException($"Control flow statement of type {Type} is not implemented")
        };
    }
}

public enum ControlFlowStatementType
{
    Break, 
    Continue,
}