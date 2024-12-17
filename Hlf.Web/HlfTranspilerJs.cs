using System.Runtime.Versioning;
using Bootsharp;
using Hlf.Transpiler;

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
            string mcFunction = new Transpiler().Transpile(src);
            return mcFunction;
        }
        catch (LanguageException langEx)
        {
            return "errhndl:" + langEx.ToString();
        }

    }
}