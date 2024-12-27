﻿using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class BuiltinFunctionCall : Statement
{
    public static BuiltinFunctionDefinition[] BuiltinFunctionDefinitions { get; } =
    [
        new("say", HlfType.Void, (gen, parameters, _) => $"tellraw @a \"{parameters["content"].Generate(gen)}\"",
            [
                new("content", HlfType.ConstString),
            ]
        ),
        new("say", HlfType.Void, (gen, parameters, _) => $"tellraw @a {{\"storage\":\"{gen.StorageNamespace}\", \"nbt\":\"{parameters["content"].Generate(gen)}\"}}",
            [
                new("content", HlfType.String),
            ]
        ),
        new("say", HlfType.Void, (gen, parameters, _) => $"tellraw @a {{\"storage\":\"{gen.StorageNamespace}\", \"nbt\":\"{parameters["content"].Generate(gen)}\"}}",
            [
                new("content", HlfType.Int),
            ]
        ),
        new("say", HlfType.Void, (gen, parameters, _) => $"tellraw @a {{\"storage\":\"{gen.StorageNamespace}\", \"nbt\":\"{parameters["content"].Generate(gen)}\"}}",
            [
                new("content", HlfType.Float),
            ]
        ),
        new("say", HlfType.Void, (gen, parameters, _) => $"tellraw @a {{\"storage\":\"{gen.StorageNamespace}\", \"nbt\":\"{parameters["content"].Generate(gen)}\"}}",
            [
                new("content", HlfType.Vector),
            ]
        ),
        new("Vector", HlfType.Vector, (gen, parameters, resultId) => 
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
        
        new("int", HlfType.Int, (gen, parameters, resultId) => 
                $"{gen.Convert(parameters["x"].Generate(gen), resultId.Generate(gen), "int")}",
            [
                new("x", HlfType.Float),
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
        
        new("BlockType", HlfType.BlockType, (gen, parameters, resultId) => 
                $"setblock {resultId.Generate(gen)} {parameters["blockType"].Generate(gen)}",
            [
                new("blockType", HlfType.ConstString),
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
        
        new("summon", HlfType.Entity, (gen, parameters, resultId) =>
        {
            string tag = resultId.Generate(gen);
            return $"summon {parameters["type"].Generate(gen)} 0 0 0 {{Tags:[\"{tag}\", \"{gen.OwnedEntityTag}\"]}}";
        }, [
            new("type", HlfType.ConstString),
        ]),
        
        new("kill", HlfType.Void, (gen, parameters, _) =>
        {
            return $"kill @e[tag={parameters["entity"].Generate(gen)}]";
        }, [
            new("entity", HlfType.Entity),
        ]),
        
        new("killOwned", HlfType.Void, (gen, _, _) =>
        {
            return $"execute as @e[tag={gen.OwnedEntityTag}] run data modify entity @s Health set value 0";
        }),

        #region Mathematics

                
        new("sin", HlfType.Float, (gen, parameters, resultId) => 
                $"{CalculateSinAndCos(gen, parameters["x"].Generate(gen))}\n" +
                $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from entity @e[tag=hlf_sin_calc, limit=1] Pos[0]\n" +
                $"kill @e[tag={gen.MarkerTag}, tag=hlf_sin_calc]",
            [
                new("x", HlfType.Int),
            ]
        ),
        new("cos", HlfType.Float, (gen, parameters, resultId) => 
                $"{CalculateSinAndCos(gen, parameters["x"].Generate(gen))}\n" +
                $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from entity @e[tag=hlf_sin_calc, limit=1] Pos[2]\n" +
                $"kill @e[tag={gen.MarkerTag}, tag=hlf_sin_calc]",
            [
                new("x", HlfType.Int),
            ]
        ),

        #endregion
    ];
    
    private static string CalculateSinAndCos(GeneratorOptions gen, string inputVariable) => $"{gen.CopyDataToScoreboard(inputVariable, "dtf", "-10000000")}\n" +
                                                                                            $"{gen.CopyScoreToData("dtf", "sin_input", "0.0000001", "float")}\n" +
                                                                                            $"summon armor_stand 0 63 0 {{Tags:[\"{gen.MarkerTag}\", \"hlf_sin_calc\"]}}\n" +
                                                                                            $"data modify entity @e[tag=hlf_sin_calc, limit=1] Pos set value [0d,0d,0d]\n" +
                                                                                            $"data modify entity @e[tag=hlf_sin_calc, limit=1] Rotation[0] set from storage {gen.StorageNamespace} sin_input\n" +
                                                                                            $"execute as @e[tag=hlf_sin_calc] at @s run tp @s ^ ^ ^1";

    private BuiltinFunctionDefinition? definition;
    private CustomFunctionDefinition? customFunctionDefinition;
    public override void Parse()
    {
        Parameters.ForEach(x => x.Parse());

        var overloads = BuiltinFunctionDefinitions.Where(x => x.Name == FunctionName).ToArray();

        if (overloads.Length != 0)
        {
            BuiltinFunctionDefinition? o = overloads
                .Where(x => x.Parameters.Length == Parameters.Count)
                .FirstOrDefault(x => !x.Parameters.Where((t, i) => !Parameters[i].Result.Type.IsAssignableTo(t.Type)).Any());

            if (o != null)
                definition = o;
            else
            {
                throw new LanguageException($"Overload for function with signature {FunctionName}({string.Join(", ", Parameters.Select(x => x.Result.Type.Name))}) could not be found.\n" +
                                            $"Available overloads of {FunctionName}:\n" +
                                            $"{string.Join('\n', overloads.Select(ov => $"    {FunctionName}({string.Join(", ", ov.Parameters.Select(x => x.Type.Name))})"))}", Line, Column, FunctionName.Length);
            }


            if (definition.ReturnType.Kind != ValueKind.Void)
                Result = definition.ReturnType.NewDataId();
        }
        else
        {
            if (!ParentScope.TryGetFunction(FunctionName, out var function))
                throw new LanguageException($"No function called {FunctionName} was found.", Line, Column, FunctionName.Length);
            
            // User defined function

            if (Parameters.Count > 0) throw new LanguageException($"Function {FunctionName} does not receive parameters but your are supplying {Parameters.Count}", Line, Column, FunctionName.Length);

            customFunctionDefinition = function;

        }
    }

    public override string Generate(GeneratorOptions options)
    {
        if (definition == null && customFunctionDefinition == null) throw new ArgumentNullException();

        StringBuilder sb = new();
        Dictionary<string, DataId> parameterDataIds = new();
        for (int i = 0; i < Parameters.Count; i++)
        {
            sb.AppendCommands(ParentScope, Parameters[i].Generate(options));
            DataId parameterId = Parameters[i].Result!.ConvertImplicitly(options, definition.Parameters[i].Type, out string code);
            sb.AppendCommands(ParentScope, code);
            parameterDataIds.Add(definition.Parameters[i].Name, parameterId);
        }
        
        // Actual function code
        if(definition != null)
            sb.AppendCommands(ParentScope, definition.CodeGenerator.Invoke(options, parameterDataIds, Result!));
        else
            sb.AppendCommands(ParentScope, $"function {options.DatapackNamespace}:{customFunctionDefinition!.Name}");
    
        // Free parameter values
        sb.AppendCommands(ParentScope, options.Comment("Freeing parameter values for function call.\n"));
        sb.AppendCommands(ParentScope, string.Join("\n", parameterDataIds.Values.Union(Parameters.Select(x => x.Result)).Select(x => x.FreeIfTemporary(options))));
        
        return sb.ToString().TrimEnd('\n');
    }

    public List<Statement> Parameters;
    public string FunctionName { get; set; }
}