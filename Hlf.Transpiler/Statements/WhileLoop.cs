using System.Text;
using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.DatapackGen;

namespace Hlf.Transpiler;

public class WhileLoop : Statement
{
    public Statement Condition;
    public List<Statement> Body;
    public Scope LoopScope;

    public override bool NeedsSemicolon => false;

    public override void Parse()
    {
        Condition.Parse();
        Body.ForEach(statement => statement.Parse());
        if (!Condition.Result!.Type.IsAssignableTo(HlfType.Bool))
            throw new LanguageException($"Condition expression of while-statement should be of type {HlfType.Bool.Name} but is of type {Condition.Result!.Type.Name}", Condition.Line, Condition.Column);

    }

    private static int loopCounter = 0;
    public override string Generate(GeneratorOptions gen)
    {
        StringBuilder sb = new();

        string funcName = $"while{loopCounter++}";
        
        DataId conditionVal = Condition.Result!.ConvertImplicitly(gen, HlfType.Bool, out string conditionConversionCode);
        string conditionEvalCode = Condition.Generate(gen) + "\n" + conditionConversionCode;
        string conditionalFuncCallCode = $"execute if data storage {gen.StorageNamespace} {{{conditionVal.Generate(gen)}:1b}} run function {gen.DatapackNamespace}:{funcName}";

        sb.AppendCommands(LoopScope, string.Join('\n', Body.Select(x => x.Generate(gen))));
        sb.AppendCommands(LoopScope, conditionEvalCode);
        sb.AppendCommands(LoopScope, conditionalFuncCallCode); // Recursive call in loop here

        Function loopFunction = new()
        {
            Name = funcName,
            SourceCode = sb.ToString(),
        };
        
        base.ExtraFunctionsToGenerate = [
            loopFunction,
        ];

        sb = new();
        sb.AppendCommands(ParentScope, conditionEvalCode);
        sb.AppendCommands(ParentScope, conditionalFuncCallCode);
        return sb.ToString();
    }
}