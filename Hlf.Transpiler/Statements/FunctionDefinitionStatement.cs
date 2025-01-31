using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class FunctionDefinitionStatement : Statement
{
    public string Name;
    public List<Statement> Block = new();
    public Scope FunctionScope = new();
    public override bool NeedsSemicolon => false;

    public override void Parse()
    {
        if (ParentScope.Parent != null) throw new LanguageException("Functions can only be defined in root scope", Line, Column);
        
        Block.ForEach(statement => statement.Parse());

        CustomFunctionDefinition def = new()
        {
            Name = Name,
            Scope = FunctionScope,
        };
        ParentScope.FunctionDefinitions.Add(Name, def);
    }

    public override string Generate(GeneratorOptions gen)
    {
        StringBuilder sb = new();
        Block.ForEach(x => sb.AppendCommands(FunctionScope, x.Generate(gen)));
        sb.AppendLine(); // add missing linebreak
        sb.Append(FunctionScope.GenerateScopeDeallocation(gen));
        gen.ExtraFunctionsToGenerate.Add(new(Name, sb.ToString()));
        return "";
    }
}

public class CustomFunctionDefinition
{
    public string Name;
    public Scope Scope;
}