using System.Runtime.Versioning;
using System.Web;
using Bootsharp;
using Hlf.Transpiler;
using Hlf.Transpiler.CodeGen;
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
    public static string TranspileToString(string src, GeneratorOptions options)
    {
        try
        {
            var dp = new Transpiler().Transpile(src, options);
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
    
    [JSInvokable]
    public static FunctionMetadata GetFunctionDescription(string functionName)
    {
        var overloads = BuiltinFunctionCall.BuiltinFunctionDefinitions.Where(x => x.Name == functionName).ToArray();

        if (!overloads.Any()) return new(false, "", []);
        
        var definition = overloads.First();

        var overloadStrings = overloads.Select(ov => $"{functionName}({string.Join(", ", ov.Parameters.Select(x => x.Type.Name))})").ToArray();
        return new(true, definition.Description, overloadStrings);
    }

    private static string SerializeDictionary(Dictionary<string, string> dict)
    {
        return $"{{{string.Join(",", dict.Select(x => $"\"{HttpUtility.JavaScriptStringEncode(x.Key)}\":\"{HttpUtility.JavaScriptStringEncode(x.Value)}\""))}}}";
    }
}

public record FunctionMetadata(bool success, string? Description, string[] Overloads);