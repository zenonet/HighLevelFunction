using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class UnaryOperator : Statement
{
    public Token Operator;
    public Statement Operand;
    public override void Parse()
    {
        Operand.Parse();
        if (!Operand.Result.Type.IsAssignableTo(HlfType.Bool) || Operator.Type != TokenType.ExclamationMark) throw new LanguageException($"Unary operation '{Operator.Content}' on value of type '{Operand.Result.Type.Name}' is not supported.", Operator);
        Result = DataId.FromType(HlfType.Bool);
    }

    public override string Generate(GeneratorOptions gen)
    {
        DataId value = Operand.Result.ConvertImplicitly(gen, HlfType.Bool, out string conversion);
        StringBuilder sb = new();
        sb.SmartAppendL(conversion);
        sb.AppendLine($"execute if data storage {gen.StorageNamespace} {{{value.Generate(gen)}:1b}} run data modify storage {gen.StorageNamespace} {Result.Generate(gen)} set value 0b");
        sb.Append($"execute if data storage {gen.StorageNamespace} {{{value.Generate(gen)}:0b}} run data modify storage {gen.StorageNamespace} {Result.Generate(gen)} set value 1b");
        return sb.ToString();
    }
}