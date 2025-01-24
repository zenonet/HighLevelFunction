using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class ThrowErrorLater(LanguageException exception) : Statement
{

    public override void Parse()
    {
        throw exception;
    }

    public override string Generate(GeneratorOptions options)
    {
        throw new NotImplementedException();
    }
}