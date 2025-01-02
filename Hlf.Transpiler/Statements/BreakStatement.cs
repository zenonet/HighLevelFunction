using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class BreakStatement : Statement
{
    public override void Parse()
    {
        if (ParentScope.ClosestLoop is not { } loop) throw new LanguageException("Break statement can only be used in a loop", Line, Column);
        loop.HasControlFlowStatements = true;
    }

    public override string Generate(GeneratorOptions gen)
    {
        return $"data modify storage {gen.StorageNamespace} {ParentScope.ClosestLoop!.ControlFlowDataId!.Generate(gen)} set value 1b\n" +
               $"{ParentScope.ClosestLoop!.LoopScope.GenerateScopeDeallocation(gen)}\n" +
               $"{ParentScope.ClosestLoop!.ControlFlowDataId.Free(gen)}";
    }
}