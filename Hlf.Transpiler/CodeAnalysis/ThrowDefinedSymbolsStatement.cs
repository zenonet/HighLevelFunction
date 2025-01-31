using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class ThrowDefinedSymbolsStatement : Statement
{
    public static bool AllowSymbolThrows = false;
    public override void Parse()
    {
        Console.WriteLine("Hit ThrowDefinedSymbolsStatement");
        throw new SymbolThrow(new(
            Success: true,
            Error: null,
            Variables: ParentScope.GetAllAvailableVariables().Select(x => $"{x.Key}:{x.Value.Type.Name!}").ToList(),
            Types:ParentScope.Types.Select(x => x.Name!).ToList(),
            Functions:ParentScope.GetAllFunctionDefinitions().Select(x => x.GetSignature()).ToList()
        ));
    }

    public override string Generate(GeneratorOptions options)
    {
        throw new();
    }
}