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

        FindOperationDefinition();
        
        Result = definition!.ResultType.NewDataId();
    }
    
    private void FindOperationDefinition()
    {

        // Basically, when != is used, we use the definition for == and invert the result afterward
        TokenType operatorForDefinition = Op.Type == TokenType.NotEquals ? TokenType.DoubleEquals : Op.Type;
        if ((A.Result.Type == HlfType.Float && B.Result.Type == HlfType.Int) || (A.Result.Type == HlfType.Int && B.Result.Type == HlfType.Float))
        {
            // Allow float operations between any operands of type int and float
            definition = HlfType.Float.Operations.FirstOrDefault(x => x.Operator == operatorForDefinition && x.OtherType == HlfType.Float);
            if (definition != null) return;
            
            // let the normal logic throw the normal error
        }
        
        definition = A.Result.Type.Operations.FirstOrDefault(x => x.OtherType == B.Result.Type && x.Operator == operatorForDefinition);
        if (definition == null) throw new LanguageException($"Type {A.Result.Type.Name} does not implement the {Op.Content} Operator with a right side of type {B.Result.Type.Name}", Op);
    }
    
    public override string Generate(GeneratorOptions gen)
    {
        StringBuilder sb = new();
        sb.SmartAppendL(A.Generate(gen));
        sb.SmartAppendL(B.Generate(gen));
        sb.SmartAppendL(definition!.Generator.Invoke(gen, A.Result, B.Result, Result));
        if (Op.Type == TokenType.NotEquals)
        {
            // Invert the result if the operator is '!='
            sb.SmartAppendL($"execute store success storage {gen.StorageNamespace} {Result.Generate(gen)} byte 1 if data storage {gen.StorageNamespace} {{{Result.Generate(gen)}:0b}}");
        }
        sb.SmartAppendL(A.Result.FreeIfTemporary(gen));
        sb.SmartAppendL(B.Result.FreeIfTemporary(gen));
        return sb.ToString();
    }
}