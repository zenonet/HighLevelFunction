using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class Operation : Statement
{
    public Statement A;
    public Statement B;
    public Token Op;
    OperationDefinition? definition;
    public override void Parse()
    {
        A.Parse();
        B.Parse();

        definition = A.Result.Type.Operations.FirstOrDefault(x => x.OtherType == B.Result.Type && x.Operator == Op.Type);
        if (definition == null)
            throw new LanguageException($"Type {A.Result.Type.Name} does not implement the {Op.Content} Operator with a right side of type {B.Result!.Type.Name}", Op);
        Result = definition.ResultType.NewDataId();
    }
    
    public override string Generate(GeneratorOptions gen)
    {
        StringBuilder sb = new();
        sb.AppendCommands(ParentScope, A.Generate(gen));
        sb.AppendCommands(ParentScope, B.Generate(gen));

        sb.Append(definition!.Generator.Invoke(gen, A.Result!, B.Result!, Result!));
        return sb.ToString();
    }
}