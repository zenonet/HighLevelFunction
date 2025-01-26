using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public static class ResourceFunctions
{
    // TODO: Add custom namespace for hlf res functions
    public static ResourceFunctionGenerator Pow =
        gen =>
            ("hlf_res_pow",
                $"scoreboard players operation pow {gen.Scoreboard} *= a {gen.Scoreboard}\n" +
                $"scoreboard players remove b {gen.Scoreboard} 1\n" +
                $"execute unless score b {gen.Scoreboard} matches ..1 run function {gen.DatapackNamespace}:hlf_res_pow"
            );
    public static ResourceFunctionGenerator RecursiveBlockRaycast =
        gen =>
            ("hlf_raycast_block",
                $"execute as @e[type=marker,tag=hlf_raycast,limit=1] at @s run tp @s ^ ^ ^0.1\n" +
                (gen.VisualizeRaycasts ? $"execute as @e[type=marker,tag=hlf_raycast,limit=1] at @s run particle minecraft:crit ~ ~ ~ .2 .2 .2 0 10\n" : "") +
                $"scoreboard players remove ray_steps {gen.Scoreboard} 1\n" +
                $"execute unless score ray_steps {gen.Scoreboard} matches ..0 as @e[type=marker,tag=hlf_raycast,limit=1] at @s if block ~ ~ ~ minecraft:air run function {gen.DatapackNamespace}:hlf_raycast_block"
            );
}
public delegate (string, string) ResourceFunctionGenerator(GeneratorOptions gen);