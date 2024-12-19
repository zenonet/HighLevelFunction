﻿using System.Runtime.Versioning;
using System.Web;
using Bootsharp;
using Hlf.Transpiler;
using File = Hlf.Transpiler.DatapackGen.File;

[SupportedOSPlatform("browser")]
public static partial class HlfTranspilerJs
{
    public static void Main()
    {
        Console.WriteLine("Transpiler WASM loaded");
        //OnMainInvoked($"Hello {GetFrontendName()}, .NET here!");
    }

    /*
    [JSEvent] // Used in JS as Program.onMainInvoked.subscribe(..)
    public static partial void OnMainInvoked (string message);*/

    /*
    [JSFunction] 
    public static partial string GetFrontendName ();
    */

    //[JSInvokable] // Invoked from JS as Program.GetBackendName()
    [JSInvokable]
    public static string TranspileToString(string src)
    {
        try
        {
            var dp = new Transpiler().Transpile(src, new ());
            List<File> generate = dp.Generate();
            Dictionary<string, string> files = generate.ToDictionary(x =>x.Path, y => y.Content);
            return SerializeDictionary(files);

            //return JsonSerializer.Serialize(files);
        }
        catch (LanguageException langEx)
        {
            return $"errhndl({langEx.Line};{langEx.Column};{langEx.Length}):" + langEx;
        }

    }

    private static string SerializeDictionary(Dictionary<string, string> dict)
    {
        return $"{{{string.Join(",", dict.Select(x => $"\"{HttpUtility.JavaScriptStringEncode(x.Key)}\":\"{HttpUtility.JavaScriptStringEncode(x.Value)}\""))}}}";
    }
}