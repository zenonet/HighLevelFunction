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
}
public delegate (string, string) ResourceFunctionGenerator(GeneratorOptions gen);