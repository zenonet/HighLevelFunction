using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class ThrowExpressionInfoStatement: Statement
{
    public Statement Expression;
    public override void Parse()
    {
        Expression.Parse();
        throw new ExpressionInfoThrow(new(
            Success:true,
            Type: Expression.Result.Type.Name!,
            Members: (Expression.Result.Type.Members ?? []).Keys.ToList()
        ));
    }

    public override string Generate(GeneratorOptions options)
    {
        throw new NotImplementedException();
    }
}

public class ExpressionInfoThrow(ExpressionMetadata meta) : Exception
{
    public ExpressionMetadata Metadata = meta;
}

public record ExpressionMetadata(
    bool Success,
    string Type,
    List<string> Members
);