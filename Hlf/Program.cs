﻿
using System.Diagnostics;
using System.Text.Json;
using Hlf.Transpiler;
using Hlf.Transpiler.DatapackGen;
using Newtonsoft.Json;
using Directory = System.IO.Directory;
using File = System.IO.File;

string src = File.ReadAllText(@"C:\Users\zeno\RiderProjects\Hlf.Transpiler\Hlf\first.hlf");

Transpiler transpiler = new();

const string dataPackPath = @"C:\Users\zeno\MultiMC\instances\1.21.3 HLF\.minecraft\saves\HflTests\datapacks\first_hlf\";
try
{
    Stopwatch sw = Stopwatch.StartNew();
    Datapack datapack = transpiler.Transpile(src);
    datapack.Name = "first_hlf";
    datapack.Namespace = "first_hlf";
    Hlf.Transpiler.DatapackGen.Directory gen = datapack.Generate();
    new DefaultDirectoryGenerator().GenerateDirectoryStructure(@"C:\Users\zeno\MultiMC\instances\1.21.3 HLF\.minecraft\saves\HflTests\datapacks\", gen);
    /*CreateDataPackStructure(dataPackPath);
    File.WriteAllText(Path.Join(dataPackPath, @"data", "first_hlf", "function", "load.mcfunction"), mcFunction);
    sw.Stop();*/
    Console.WriteLine($"Successfully generated datapack in {sw.Elapsed.TotalMilliseconds}ms");
}
catch (LanguageException l)
{
    //if(Debugger.IsAttached) Debugger.Break();
    //Console.WriteLine($"Error in line {l.Line}: {l.CustomErrorMessage}");
    Console.WriteLine(l.ToString());
}

return;

Console.WriteLine("Generating datapack...");
Datapack dp = new()
{
    Name = "deez",
    Functions =
    [
        new Function("myload", "tellraw @a \"yooo\""),
    ],
};
dp.OnLoadFunction = dp.Functions[0];
dp.Namespace = "mynamespace";
Hlf.Transpiler.DatapackGen.Directory directory = dp.Generate();

string serialize = JsonConvert.SerializeObject(directory);
Console.WriteLine(serialize);
new DefaultDirectoryGenerator().GenerateDirectoryStructure(@"C:\Users\zeno\Desktop\datapack", directory);


void CreateDataPackStructure(string dataPackPath)
{
    string dataPackName = Path.GetFileName(Path.GetDirectoryName(dataPackPath)!);
    Directory.CreateDirectory(dataPackPath);
    Directory.CreateDirectory(Path.Join(dataPackPath, "data", "minecraft", "tags", "function"));
    Directory.CreateDirectory(Path.Join(dataPackPath, "data", dataPackName, "function"));
    File.WriteAllText(Path.Join(dataPackPath, "data", "minecraft", "tags", "function", "load.json"), $"{{\"replace\": false, \"values\": [\"{dataPackName}:load\"]}}");
    File.WriteAllText(Path.Join(dataPackPath, "pack.mcmeta"), "{\"pack\": {\"pack_format\":14,\"description\": \"Generated by HLF\"}}");
}