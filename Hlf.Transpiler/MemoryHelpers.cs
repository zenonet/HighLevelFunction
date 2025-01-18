using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public static class MemoryHelpers
{
    public static string CopyDataToScoreboard(this GeneratorOptions gen, string dataPath, string scoreboardName, string scale = "1")
    {
        return $"execute store result score {scoreboardName} {gen.Scoreboard} run data get storage {gen.StorageNamespace} {dataPath} {scale}";
    }
    public static string CopyDataToData(this GeneratorOptions gen, string dataPath, string destDataPath)
    {
        return $"data modify storage {gen.StorageNamespace} {destDataPath} set from storage {gen.StorageNamespace} {dataPath}";
    }    
    
    public static string CopyScoreToScore(this GeneratorOptions gen, string src, string dest)
    {
        return $"scoreboard players operation {dest} {gen.Scoreboard} = {src} {gen.Scoreboard}";
    }
    
    public static string CopyScoreToData(this GeneratorOptions gen, string scoreboardName, string dataPath, string scale = "1", string type = "int")
    {
        return $"execute store result storage {gen.StorageNamespace} {dataPath} {type} {scale} run scoreboard players get {scoreboardName} {gen.Scoreboard}";
    }

    public static string ScoreboardOpIntoA(this GeneratorOptions gen, string a, string b, string @operator)
    {
        return $"scoreboard players operation {a} {gen.Scoreboard} {@operator} {b} {gen.Scoreboard}";
    }

    public static string Convert(this GeneratorOptions gen, string dataIn, string dataOut, string type)
    {
        return $"execute store result storage {gen.StorageNamespace} {dataOut} {type} 1 run data get storage {gen.StorageNamespace} {dataIn}";
    }
}