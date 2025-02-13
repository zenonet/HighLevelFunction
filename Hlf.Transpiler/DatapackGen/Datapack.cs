﻿
namespace Hlf.Transpiler.DatapackGen;

public class Datapack
{
    public string Name { get; set; }
    public string Namespace { get; set; } = "hlf";
    public int DatapackFormat { get; set; } = 14;
    public string Description { get; set; } = "Generated by Hlf";
    public List<Function> Functions { get; set; } = new();
    public Function? OnLoadFunction;
    public Function? OnTickFunction;

    public List<File> Generate()
    {
        List<File> files = new();

        string dataPath = Path.Join(Name, "data");
        
        files.Add(GenerateMcMetaFile(Name));
        
        files.AddRange(GenerateMinecraftDir(dataPath));
        files.AddRange(GenerateDatapackDir(dataPath));
        
        return files;
    }

    public File GenerateMcMetaFile(string basePath)
    {
        File file = new(Path.Join(basePath, "pack.mcmeta"), "{\"pack\": {\"pack_format\":" + DatapackFormat + ",\"description\": \"" + Description + "\"}}");
        return file;
    }

    public List<File> GenerateMinecraftDir(string basePath)
    {
        List<File> files = new();
        
        string functionsPath = Path.Join(basePath, "minecraft", "tags", "function");
        
        if (OnLoadFunction is not null)
        {
            const bool replace = false;
            string json = $"{{\"replace\": {replace.ToString().ToLower()}, \"values\": [\"{Namespace}:{OnLoadFunction!.Name}\"]}}";
            files.Add(new(Path.Join(functionsPath, "load.json"), json));
        }

        if (OnTickFunction is not null)
        {
            const bool replace = false;
            string json = $"{{\"replace\": {replace.ToString().ToLower()}, \"values\": [\"{Namespace}:{OnTickFunction!.Name}\"]}}";
            files.Add(new(Path.Join(functionsPath, "tick.json"), json));
        }

        return files;
    }

    public List<File> GenerateDatapackDir(string basePath)
    {
        List<File> files = new();
        string functionsPath = Path.Join(basePath, Namespace, "function");
        foreach (Function function in Functions)
        {
            files.Add(new(Path.Join(functionsPath, function.Name + ".mcfunction"), function.SourceCode));
        }
        return files;
    }
}