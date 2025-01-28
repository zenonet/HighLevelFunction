using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class BuiltinFunctionCall : Statement
{
    public static BuiltinFunctionDefinition[] BuiltinFunctionDefinitions { get; } =
    [
        new BuiltinFunctionDefinition("say", HlfType.Void, (gen, parameters, _) => $"tellraw @a \"{parameters["content"].Generate(gen)}\"",
            [
                new("content", HlfType.ConstString),
            ]
        ).WithDescription("Writes a value to the chat"),
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
        new BuiltinFunctionDefinition("Vector", HlfType.Vector, (gen, parameters, resultId) => 
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
        ).WithDescription("Constructs a 3d vector from individual values"),
        
        new BuiltinFunctionDefinition("int", HlfType.Int, (gen, parameters, resultId) => 
                $"{gen.Convert(parameters["x"].Generate(gen), resultId.Generate(gen), "int")}",
            [
                new("x", HlfType.Float),
            ]
        ).WithDescription("Converts a float into an integer"),
        new BuiltinFunctionDefinition("string", HlfType.String, (gen, parameters, resultId) => 
                gen.CopyDataToData(parameters["x"].Generate(gen), resultId.Generate(gen)), // basically trust me bro
            [
                new("x", HlfType.Int),
            ]
        ).WithDescription("Converts a value to a string"),
        new("string", HlfType.String, (gen, parameters, resultId) => 
                gen.CopyDataToData(parameters["x"].Generate(gen), resultId.Generate(gen)), // basically trust me bro
            [
                new("x", HlfType.Float),
            ]
        ),
        
        new BuiltinFunctionDefinition("setBlock", HlfType.Void, (gen, parameters, _) => 
                gen.Comment("Placing a block\n") +
                $"summon marker 0 0 0 {{Tags:[\"{gen.MarkerTag}\", \"hlf_setblock\"]}}\n" +
                $"data modify entity @e[type=marker, tag={gen.MarkerTag}, tag=hlf_setblock, limit=1] Pos set from storage {gen.StorageNamespace} {parameters["position"].Generate(gen)}\n" +
                $"execute at @e[type=marker, tag={gen.MarkerTag}, tag=hlf_setblock, limit=1] run clone {parameters["block"].Generate(gen)} {parameters["block"].Generate(gen)} ~ ~ ~\n" +
                $"kill @e[type=marker, tag={gen.MarkerTag}, tag=hlf_setblock, limit=1]",
            [
                new("position", HlfType.Vector),
                new("block", HlfType.BlockType),
            ]
        ),
        new BuiltinFunctionDefinition("setBlock", HlfType.Void, (gen, parameters, _) => 
                gen.Comment("Placing a block\n") +
                $"summon marker 0 0 0 {{Tags:[\"{gen.MarkerTag}\", \"hlf_setblock\"]}}\n" +
                $"data modify entity @e[type=marker, tag={gen.MarkerTag}, tag=hlf_setblock, limit=1] Pos set from storage {gen.StorageNamespace} {parameters["position"].Generate(gen)}\n" +
                $"execute at @e[type=marker, tag={gen.MarkerTag}, tag=hlf_setblock, limit=1] run clone {parameters["block"].Generate(gen)} {parameters["block"].Generate(gen)} ~ ~ ~\n" +
                $"kill @e[type=marker, tag={gen.MarkerTag}, tag=hlf_setblock, limit=1]",
            [
                new("position", HlfType.Vector),
                new("block", HlfType.BlockType),
            ]
        ).WithDescription("Places a block of a specified type at a specified position"),
        
        new BuiltinFunctionDefinition("getBlock", HlfType.BlockType, (gen, parameters, resultId) => 
                gen.Comment("Copying block into memory\n") +
                $"summon marker 0 0 0 {{Tags:[\"{gen.MarkerTag}\", \"hlf_getblock\"]}}\n" +
                $"data modify entity @e[type=marker, tag={gen.MarkerTag}, tag=hlf_getblock, limit=1] Pos set from storage {gen.StorageNamespace} {parameters["position"].Generate(gen)}\n" +
                $"execute at @e[type=marker, tag={gen.MarkerTag}, tag=hlf_getblock, limit=1] run clone ~ ~ ~ ~ ~ ~ {resultId.Generate(gen)}\n" +
                $"kill @e[type=marker, tag={gen.MarkerTag}, tag=hlf_getblock]",
            [
                new("position", HlfType.Vector),
            ]
        ).WithDescription("Gets the type of a block at a specified position"),
        
        new BuiltinFunctionDefinition("BlockType", HlfType.BlockType, (gen, parameters, resultId) => 
                $"setblock {resultId.Generate(gen)} {parameters["blockType"].Generate(gen)}",
            [
                new("blockType", HlfType.ConstString),
            ]
        ).WithDescription("Returns the BlockType corresponding to the given string"),
        
        new BuiltinFunctionDefinition("summon", HlfType.Entity, (gen, parameters, resultId) =>
        {
            string tag = resultId.Generate(gen);
            return $"tag @e remove {tag}\n" +
                   $"summon {parameters["type"].Generate(gen)} 0 0 0 {{Tags:[\"{tag}\", \"{gen.OwnedEntityTag}\"]}}";
        }, [
            new("type", HlfType.ConstString),
        ]).WithDescription("Creates an instance of the specified entity type at Position 0 0 0."),
        
        new BuiltinFunctionDefinition("kill", HlfType.Void, (gen, parameters, _) =>
        {
            return $"kill @e[tag={parameters["entity"].Generate(gen)}]";
        }, [
            new("entity", HlfType.Entity),
        ]).WithDescription("Kills the specified entity"),
        
        new BuiltinFunctionDefinition("killOwned", HlfType.Void, (gen, _, _) =>
        {
            return $"execute as @e[tag={gen.OwnedEntityTag}] run data modify entity @s Health set value 0";
        }).WithDescription("Kills all entities owned (e.g. created) by the datapack"),
            
        new BuiltinFunctionDefinition("glow", HlfType.Void, (gen, parameters, _) =>
        {
            return $"effect give @e[tag={parameters["target"].Generate(gen)}] glowing 10 1 true";
        },
            [
            new("target", HlfType.Entity)]).WithDescription("Gives the supplied entity the glowing effect for 10 seconds"),
        
        
        new BuiltinFunctionDefinition("raycast", HlfType.Vector, (gen, parameters, resultId) =>
        {
            return $"summon marker 0.0 0.0 0.0 {{Tags:[\"{gen.MarkerTag}\",\"hlf_raycast\"]}}\n" +
                   $"summon marker 0 0 0 {{Tags:[\"{gen.MarkerTag}\",\"hlf_raycast_dir\"]}}\n" +
                   $"data modify entity @e[type=marker,tag=hlf_raycast_dir,limit=1] Pos set from storage {gen.StorageNamespace} {parameters["direction"].Generate(gen)}\n" +
                   $"execute as @e[type=marker,tag=hlf_raycast] at @s run tp @s ~ ~ ~ facing entity @e[type=marker,tag=hlf_raycast_dir,limit=1] eyes\n" +
                   $"data modify entity @e[type=marker,tag=hlf_raycast,limit=1] Pos set from storage {gen.StorageNamespace} {parameters["origin"].Generate(gen)}\n" +
                   $"kill @e[type=marker,tag=hlf_raycast_dir,limit=1]\n" +
                   $"{gen.CopyDataToScoreboard(parameters["maxSteps"].Generate(gen), "ray_steps")}\n" +
                   $"function {gen.DatapackNamespace}:hlf_raycast_block\n" +
                   $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from entity @e[type=marker,tag=hlf_raycast,limit=1] Pos\n" +
                   $"kill @e[type=marker,tag=hlf_raycast,limit=1]"
                   ;
        }, [
            new ("origin", HlfType.Vector),
            new("direction", HlfType.Vector),
            new ("maxSteps", HlfType.Int),
        ]).WithDescription("Raycasts through the world. The ray is blocked by blocks only. Returns the position where the ray hit a block")
        .WithDependencies(ResourceFunctions.RecursiveBlockRaycast),        
        new BuiltinFunctionDefinition("raycastForEntity", HlfType.Entity, (gen, parameters, resultId) =>
        {
            return $"summon marker 0.0 0.0 0.0 {{Tags:[\"{gen.MarkerTag}\",\"hlf_raycast\"]}}\n" +
                   $"summon marker 0 0 0 {{Tags:[\"{gen.MarkerTag}\",\"hlf_raycast_dir\"]}}\n" +
                   $"data modify entity @e[type=marker,tag=hlf_raycast_dir,limit=1] Pos set from storage {gen.StorageNamespace} {parameters["direction"].Generate(gen)}\n" +
                   $"execute as @e[type=marker,tag=hlf_raycast] at @s run tp @s ~ ~ ~ facing entity @e[type=marker,tag=hlf_raycast_dir,limit=1] eyes\n" +
                   $"data modify entity @e[type=marker,tag=hlf_raycast,limit=1] Pos set from storage {gen.StorageNamespace} {parameters["origin"].Generate(gen)}\n" +
                   $"kill @e[type=marker,tag=hlf_raycast_dir,limit=1]\n" +
                   $"{gen.CopyDataToScoreboard(parameters["maxSteps"].Generate(gen), "ray_steps")}\n" +
                   $"function {gen.DatapackNamespace}:hlf_raycast_entity\n" +
                   $"tag @e[tag={resultId.Generate(gen)}] remove {resultId.Generate(gen)}\n" +
                   $"execute as @e[type=marker,tag=hlf_raycast] at @s run tag @e[distance=..1,type=!marker,limit=1,sort=nearest] add {resultId.Generate(gen)}\n" +
                   $"kill @e[type=marker,tag=hlf_raycast,limit=1]";
        }, [
            new ("origin", HlfType.Vector),
            new("direction", HlfType.Vector),
            new ("maxSteps", HlfType.Int),
        ]).WithDescription("Raycasts through the world. The ray is blocked by entities only. Returns the the entity that has been hit.")
        .WithDependencies(ResourceFunctions.RecursiveEntityRaycast),

        #region Mathematics

                
        new BuiltinFunctionDefinition("sin", HlfType.Float, (gen, parameters, resultId) => 
                $"{CalculateSinAndCos(gen, parameters["x"].Generate(gen))}\n" +
                $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from entity @e[tag=hlf_sin_calc, limit=1] Pos[0]\n" +
                $"kill @e[type=armor_stand, tag={gen.MarkerTag}, tag=hlf_sin_calc]",
            [
                new("x", HlfType.Float),
            ]
        ).WithDescription("Calculates the sine of the supplied value in degrees"),
        new BuiltinFunctionDefinition("cos", HlfType.Float, (gen, parameters, resultId) => 
                $"{CalculateSinAndCos(gen, parameters["x"].Generate(gen))}\n" +
                $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set from entity @e[tag=hlf_sin_calc, limit=1] Pos[2]\n" +
                $"kill @e[type=armor_stand, tag={gen.MarkerTag}, tag=hlf_sin_calc]",
            [
                new("x", HlfType.Float),
            ]
        ).WithDescription("Calculates the cosine of the supplied value in degrees"),
        new BuiltinFunctionDefinition("tan", HlfType.Float, (gen, parameters, resultId) => 
                $"{CalculateSinAndCos(gen, parameters["x"].Generate(gen))}\n" +
                $"execute store result score sin {gen.Scoreboard} run data get entity @e[tag=hlf_sin_calc, limit=1] Pos[0] 1000000\n" +
                $"execute store result score cos {gen.Scoreboard} run data get entity @e[tag=hlf_sin_calc, limit=1] Pos[2] 1000\n" +
                $"kill @e[type=armor_stand, tag={gen.MarkerTag}, tag=hlf_sin_calc]\n" +
                $"{gen.ScoreboardOpIntoA("sin", "cos", "/=")}\n" +
                $"{gen.CopyScoreToData("sin", resultId.Generate(gen), "0.001", "double")}",
            [
                new("x", HlfType.Float),
            ]
        ).WithDescription("Calculates the tangent of the supplied value in degrees"),
        new BuiltinFunctionDefinition("dot", HlfType.Float, (gen, parameters, resultId) => 
                $"{gen.CopyDataToScoreboard($"{parameters["a"].Generate(gen)}[0]", "ax", "1000")}\n"+
                $"{gen.CopyDataToScoreboard($"{parameters["a"].Generate(gen)}[1]", "ay", "1000")}\n"+
                $"{gen.CopyDataToScoreboard($"{parameters["a"].Generate(gen)}[2]", "az", "1000")}\n"+
                $"{gen.CopyDataToScoreboard($"{parameters["b"].Generate(gen)}[0]", "bx", "1000")}\n"+
                $"{gen.CopyDataToScoreboard($"{parameters["b"].Generate(gen)}[1]", "by", "1000")}\n"+
                $"{gen.CopyDataToScoreboard($"{parameters["b"].Generate(gen)}[2]", "bz", "1000")}\n" +
                $"{gen.ScoreboardOpIntoA("ax", "bx", "*=")}\n" +
                $"{gen.ScoreboardOpIntoA("ay", "by", "*=")}\n" +
                $"{gen.ScoreboardOpIntoA("az", "bz", "*=")}\n" +
                $"{gen.ScoreboardOpIntoA("ax", "ay", "+=")}\n" +
                $"{gen.ScoreboardOpIntoA("ax", "az", "+=")}\n" +
                $"{gen.CopyScoreToData("ax", resultId.Generate(gen), "0.000001", "float")}",
            [
                new("a", HlfType.Vector),
                new("b", HlfType.Vector),
            ]
        ).WithDescription("Calculates the [dot product](https://en.wikipedia.org/wiki/Dot_product) of the 2 supplied vectors"),
        new BuiltinFunctionDefinition("cross", HlfType.Vector, (gen, parameters, resultId) => 
                $"{gen.CopyDataToScoreboard($"{parameters["a"].Generate(gen)}[0]", "ax", "1000")}\n"+
                $"{gen.CopyDataToScoreboard($"{parameters["a"].Generate(gen)}[1]", "ay", "1000")}\n"+
                $"{gen.CopyDataToScoreboard($"{parameters["a"].Generate(gen)}[2]", "az", "1000")}\n"+
                $"{gen.CopyDataToScoreboard($"{parameters["b"].Generate(gen)}[0]", "bx", "1000")}\n"+
                $"{gen.CopyDataToScoreboard($"{parameters["b"].Generate(gen)}[1]", "by", "1000")}\n"+
                $"{gen.CopyDataToScoreboard($"{parameters["b"].Generate(gen)}[2]", "bz", "1000")}\n" +
                
                // Calculate x-value
                $"{gen.CopyScoreToScore("ay", "x")}\n" +
                $"{gen.ScoreboardOpIntoA("x", "bz", "*=")}\n" +
                $"{gen.CopyScoreToScore("az", "x2")}\n" +
                $"{gen.ScoreboardOpIntoA("x2", "by", "*=")}\n" +
                $"{gen.ScoreboardOpIntoA("x", "x2", "-=")}\n" +
                
                // Calculate y-value
                $"{gen.CopyScoreToScore("az", "y")}\n" +
                $"{gen.ScoreboardOpIntoA("y", "bx", "*=")}\n" +
                $"{gen.CopyScoreToScore("ax", "y2")}\n" +
                $"{gen.ScoreboardOpIntoA("y2", "bz", "*=")}\n" +
                $"{gen.ScoreboardOpIntoA("y", "y2", "-=")}\n" +
                
                // Calculate z-value
                $"{gen.CopyScoreToScore("ax", "z")}\n" +
                $"{gen.ScoreboardOpIntoA("z", "by", "*=")}\n" +
                $"{gen.CopyScoreToScore("ay", "z2")}\n" +
                $"{gen.ScoreboardOpIntoA("z2", "bx", "*=")}\n" +
                $"{gen.ScoreboardOpIntoA("z", "z2", "-=")}\n" +
                
                $"data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set value [0d,0d,0d]\n" +
                $"execute store result storage {gen.StorageNamespace} {resultId.Generate(gen)}[0] double 0.000001 run scoreboard players get x {gen.Scoreboard}\n" +
                $"execute store result storage {gen.StorageNamespace} {resultId.Generate(gen)}[1] double 0.000001 run scoreboard players get y {gen.Scoreboard}\n" +
                $"execute store result storage {gen.StorageNamespace} {resultId.Generate(gen)}[2] double 0.000001 run scoreboard players get z {gen.Scoreboard}\n",
            [
                new("a", HlfType.Vector),
                new("b", HlfType.Vector),
            ]
        ).WithDescription("Calculates the [cross product](https://en.wikipedia.org/wiki/Cross_product) of the 2 supplied vectors"),

        
        new BuiltinFunctionDefinition("abs", HlfType.Float, (gen, parameters, resultId) =>
            $"{gen.CopyDataToScoreboard(parameters["x"].Generate(gen), "a", OperationImplementations.FixedPointScale)}\n" +
            $"execute if score a {gen.Scoreboard} matches ..0 store result storage {gen.StorageNamespace} {resultId.Generate(gen)} float -{OperationImplementations.InverseFixedPointScale} run scoreboard players get a {gen.Scoreboard}\n" +
            $"execute unless score a {gen.Scoreboard} matches ..0 run {gen.CopyDataToData(parameters["x"].Generate(gen), resultId.Generate(gen))}",
            [
                new("x", HlfType.Float)
            ]
        ).WithDescription("Calculates the absolute value of the supplied number"),
        new BuiltinFunctionDefinition("pow", HlfType.Int, (gen, parameters, resultId) =>
            $"{gen.CopyDataToScoreboard(parameters["base"].Generate(gen), "a")}\n" +
            $"{gen.CopyDataToScoreboard(parameters["exponent"].Generate(gen), "b")}\n" +
            $"scoreboard players set pow {gen.Scoreboard} 1\n" +
            $"function {gen.DatapackNamespace}:hlf_res_pow\n" +
            $"{gen.CopyScoreToData("pow", resultId.Generate(gen))}\n" +
            $"{gen.CopyDataToScoreboard(parameters["exponent"].Generate(gen), "b")}\n" +
            $"execute if score b hlf matches ..0 run data modify storage {gen.StorageNamespace} {resultId.Generate(gen)} set value 0\n",
            [
                new("base", HlfType.Int),
                new("exponent", HlfType.Int),
            ]
        )
        .WithDescription("Calculates base to the power of exponent")
        .WithDependencies(ResourceFunctions.Pow),
        #endregion
    ];
    
    private static string CalculateSinAndCos(GeneratorOptions gen, string inputVariable) => $"{gen.CopyDataToScoreboard(inputVariable, "dtf", "-100000")}\n" +
                                                                                            $"{gen.CopyScoreToData("dtf", "sin_input", "0.00001", "float")}\n" +
                                                                                            $"summon armor_stand 0 63 0 {{Tags:[\"{gen.MarkerTag}\", \"hlf_sin_calc\"]}}\n" +
                                                                                            $"data modify entity @e[type=armor_stand, tag=hlf_sin_calc, limit=1] Pos set value [0d,0d,0d]\n" +
                                                                                            $"data modify entity @e[type=armor_stand, tag=hlf_sin_calc, limit=1] Rotation[0] set from storage {gen.StorageNamespace} sin_input\n" +
                                                                                            $"execute as @e[type=armor_stand, tag=hlf_sin_calc] at @s run tp @s ^ ^ ^1";

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

        if (definition != null && options.TargetVersion < definition.MinVersion)
        {
            throw new LanguageException($"The {definition.Name}() function requires a target version of at least {definition.MinVersion}", Line, Column, FunctionName.Length);
        }
        
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
        
        // Add dependency to required resource functions
        definition?.Dependencies.ForEach(x => options.DependencyResources.Add(x));
        return sb.ToString().TrimEnd('\n');
    }

    public List<Statement> Parameters;
    public string FunctionName { get; set; }
}