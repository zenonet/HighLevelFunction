using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class BuiltinFunctionCall : Statement
{
    public static BuiltinFunctionDefinition[] BuiltinFunctionDefinitions { get; } =
    [
        new("say", HlfType.Void, (gen, parameters, _) => $"tellraw @a {{\"storage\":\"{gen.StorageNamespace}\", \"nbt\":\"{parameters["content"].Generate(gen)}\"}}",
            [
                new("content", HlfType.String),
            ]
        ),
        // TODO: Add function overloading for this purpose
        new("constSay", HlfType.Void, (gen, parameters, _) => $"tellraw @a \"{parameters["content"].Generate(gen)}\"",
            [
                new("content", HlfType.ConstString),
            ]
        ),
        
        new("vec", HlfType.Vector, (gen, parameters, resultId) => 
                gen.Comment("Constructing a 3d vector from individual values\n") + 
                $"data remove storage {gen.StorageNamespace} {resultId.Generate(gen)}\n" +
                $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} append from storage {gen.StorageNamespace} {parameters["x"].Generate(gen)}\n" +
                $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} append from storage {gen.StorageNamespace} {parameters["y"].Generate(gen)}\n" +
                $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} append from storage {gen.StorageNamespace} {parameters["z"].Generate(gen)}",
            [
                new("x", HlfType.Float),
                new("y", HlfType.Float),
                new("z", HlfType.Float),
            ]
        ),
        
        new("setBlock", HlfType.Void, (gen, parameters, _) => 
                gen.Comment("Placing a block\n") +
                $"summon marker 0 0 0 {{Tags:[\"{gen.MarkerTag}\", \"hlf_setblock\"]}}\n" +
                $"data modify entity @e[tag={gen.MarkerTag}, tag=hlf_setblock, limit=1] Pos set from storage {gen.StorageNamespace} {parameters["position"].Generate(gen)}\n" +
                $"execute at @e[tag={gen.MarkerTag}, tag=hlf_setblock, limit=1] run clone {parameters["block"].Generate(gen)} {parameters["block"].Generate(gen)} ~ ~ ~\n" +
                $"kill @e[tag={gen.MarkerTag}, tag=hlf_setblock]",
            [
                new("position", HlfType.Vector),
                new("block", HlfType.BlockType),
            ]
        ),
        
        new("getBlock", HlfType.BlockType, (gen, parameters, resultId) => 
                gen.Comment("Copying block into memory\n") +
                $"summon marker 0 0 0 {{Tags:[\"{gen.MarkerTag}\", \"hlf_getblock\"]}}\n" +
                $"data modify entity @e[tag={gen.MarkerTag}, tag=hlf_getblock, limit=1] Pos set from storage {gen.StorageNamespace} {parameters["position"].Generate(gen)}\n" +
                $"execute at @e[tag={gen.MarkerTag}, tag=hlf_getblock, limit=1] run clone ~ ~ ~ ~ ~ ~ {resultId.Generate(gen)}\n" +
                $"kill @e[tag={gen.MarkerTag}, tag=hlf_getblock]",
            [
                new("position", HlfType.Vector),
            ]
        ),

        new("getEntityWithTag", HlfType.Entity, (gen, parameters, resultId) =>
        {
            string tag = parameters["tag"].Generate(gen);
            string newTag = resultId.Generate(gen);
            return $"execute as @e[tag={tag}, limit=1] run tag @s add {newTag}";
        }, [
            new("tag", HlfType.ConstString),
        ]),
        
        new("kill", HlfType.Void, (gen, parameters, _) =>
        {
            return $"kill @e[tag={parameters["entity"].Generate(gen)}]";
        }, [
            new("entity", HlfType.Entity),
        ]),
    ];

    private BuiltinFunctionDefinition? definition;
    public override void Parse()
    {
        Parameters.ForEach(x => x.Parse());
        definition = BuiltinFunctionDefinitions.FirstOrDefault(x => x.Name == FunctionName);
        if (definition == null) throw new LanguageException($"No function called {FunctionName} was found.", Line, Column);

        if (definition.Parameters.Length != Parameters.Count) throw new LanguageException($"Function {FunctionName} expects {definition.Parameters.Length} parameters but received {Parameters.Count}.", Line, Column);
        for (int i = 0; i < definition.Parameters.Length; i++)
        {
            if (!Parameters[i].Result!.Type.IsAssignableTo(definition.Parameters[i].Type))
                throw new LanguageException($"Function {FunctionName} expects a {definition.Parameters[i].Type.Name} as its {(i + 1).GetOrdinal()} parameter but received a {Parameters[i].Result!.Type.Name}.", Line, Column);
        }

        if(definition.ReturnType.Kind != ValueKind.Void)
            Result = definition.ReturnType.NewDataId();
    }

    public override string Generate(GeneratorOptions options)
    {
        if (definition == null) throw new ArgumentNullException();

        StringBuilder sb = new();
        Dictionary<string, DataId> parameterDataIds = new();
        for (int i = 0; i < Parameters.Count; i++)
        {
            sb.AppendCommands(ParentScope, Parameters[i].Generate(options));
            DataId parameterId = Parameters[i].Result!.ConvertImplicitly(options, definition.Parameters[i].Type, out string code);
            sb.AppendCommands(ParentScope, code);
            parameterDataIds.Add(definition.Parameters[i].Name, parameterId);
        }
        sb.AppendCommands(ParentScope, definition.CodeGenerator.Invoke(options, parameterDataIds, Result!));
        
        // Free parameter values
        sb.AppendCommands(ParentScope, options.Comment("Freeing parameter values for function call.\n"));
        sb.AppendCommands(ParentScope, string.Join("\n", parameterDataIds.Values.Union(Parameters.Select(x => x.Result)).Select(x => x.FreeIfTemporary(options))));
        
        return sb.ToString().TrimEnd('\n');
    }

    public List<Statement> Parameters;
    public string FunctionName { get; set; }
}