using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class PersistentComment : Statement
{
    public string Comment;
    public override bool NeedsSemicolon => false;

    public override void Parse()
    {
    }

    public override string Generate(GeneratorOptions gen) => gen.GenerateComments ? $"#{Comment}" : "";
}