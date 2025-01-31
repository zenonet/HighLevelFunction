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
            Dictionary<string, string> files = generate.ToDictionary(x => x.Path, y => y.Content);
            return SerializeDictionary(files);

            //return JsonSerializer.Serialize(files);
        }
        catch (LanguageException langEx)
        {
            return $"errhndl({langEx.Line};{langEx.Column};{langEx.Length}):" + langEx;
        }
    }
    
    
    [JSInvokable]
    public static SymbolThrowData ThrowSymbols(string src)
    {
        ThrowDefinedSymbolsStatement.AllowSymbolThrows = true;
        try
        {
            new Transpiler().Transpile(src, new());
        }
        catch (SymbolThrow e)
        {
            return e.SymbolData;
        }
        catch (LanguageException langEx)
        {
        }
        finally
        {
            ThrowDefinedSymbolsStatement.AllowSymbolThrows = false;
        }
        return new(
            Success: false,
            Error: "nothrow",
            [],
            [],
            []
        );
    }
    
    [JSInvokable]
    public static ExpressionMetadata ThrowExpressionInfo(string src)
    {
        ThrowDefinedSymbolsStatement.AllowSymbolThrows = true;
        try
        {
            new Transpiler().Transpile(src, new());

        }
        catch (ExpressionInfoThrow e)
        {
            return e.Metadata;
        }
        catch (LanguageException _)
        {
            
        }
        finally
        {
            ThrowDefinedSymbolsStatement.AllowSymbolThrows = false;
        }
        return new(
            Success: false,
            Type:"",
            Members:[]
        );
    }

    [JSInvokable]
    public static FunctionMetadata GetFunctionDescription(string functionName)
    {
        var overloads = BuiltinFunctionCall.BuiltinFunctionDefinitions.Where(x => x.Name == functionName).ToArray();

        if (!overloads.Any()) return new(false, "", "", []);

        var definition = overloads.First();

        var overloadStrings = overloads.Select(ov => $"{ov.ReturnType.Name} {functionName}({string.Join(", ", ov.Parameters.Select(x => $"{x.Type.Name} {x.Name}"))})").ToArray();
        return new(true, definition.Name, definition.Description ?? "", overloadStrings);
    }

    [JSInvokable]
    public static FunctionMetadata[] GetAllBuiltinFunctionDefinitions() =>
        BuiltinFunctionCall.BuiltinFunctionDefinitions.GroupBy(x => x.Name).ToArray().Select(overloads =>
        {
            if (!overloads.Any()) return new FunctionMetadata(false, "", "", []);

            var definition = overloads.First();

            var overloadStrings = overloads.Select(ov => $"{ov.ReturnType.Name} {definition.Name}({string.Join(", ", ov.Parameters.Select(x => $"{x.Type.Name} {x.Name}"))})").ToArray();
            return new(true, definition.Name, definition.Description ?? "", overloadStrings);
        }).ToArray();

    private static string SerializeDictionary(Dictionary<string, string> dict)
    {
        return $"{{{string.Join(",", dict.Select(x => $"\"{HttpUtility.JavaScriptStringEncode(x.Key)}\":\"{HttpUtility.JavaScriptStringEncode(x.Value)}\""))}}}";
    }
}

public record FunctionMetadata(bool success, string Name, string Description, string[] Overloads);

public record TypeMemberMetadata(string Name, string Type);