using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class VariableAccessor : Statement
{
    public string VariableName;
    public sbyte Increment = 0;
    private DataId dataId;

    public override void Parse()
    {
        if (ParentScope.TryGetVariable(VariableName, out DataId? dataId))
        {
            if (Increment != 0 && dataId.IsImmutable) throw new LanguageException($"Immutable variable '{VariableName}' cannot be {(Increment > 0 ? "in" : "de")}cremented", Line, Column);
            if (Increment != 0 && dataId.Type != HlfType.Int) throw new LanguageException("Increment/Decrement expression can only be used on variables of type int", Line, Column, VariableName.Length);

            this.dataId = dataId;
            Result = Increment == 0 || !IsReturnValueNeeded ? dataId : DataId.FromType(HlfType.Int);
        }
        else
        {
            throw new LanguageException($"Variable '{VariableName}' does not exist in the current scope", Line, Column, VariableName.Length);
        }
    }

    public override string Generate(GeneratorOptions gen)
    {
        return Increment == 0
            ? ""
            : $"{gen.CopyDataToData(dataId.Generate(gen), Result.Generate(gen))}\n" + // Copy value to result dataid for return value
              $"{gen.CopyDataToScoreboard(dataId.Generate(gen), "a")}\n" + // (?:In/De)crement
              $"scoreboard players {(Increment == -1 ? "remove" : "add")} a {gen.Scoreboard} 1\n" +
              $"{gen.CopyScoreToData("a", dataId.Generate(gen))}";
    }
}