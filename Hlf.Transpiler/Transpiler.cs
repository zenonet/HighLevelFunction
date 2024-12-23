using System.Diagnostics;
using System.Text;
using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.DatapackGen;

namespace Hlf.Transpiler;

public class Transpiler
{
    public Datapack Transpile(string sourceCode, GeneratorOptions options)
    {
        Datapack datapack = new();
        datapack.Namespace = options.DatapackNamespace;
        
        // Fix messed up line ending characters
        sourceCode = sourceCode.Replace("\r\n", "\n");
        sourceCode = sourceCode.Replace("\r", "\n");
        
        // State 1: lexical analysis
        TokenList tokens = Lexer.Lex(sourceCode);
        
        
        Scope rootScope = new();
        
        // Stage 2: Parsing
        List<Statement> statements = new Parser().ParseMultiple(ref tokens, rootScope);
        statements.ForEach(x => x.Parse());

        // Stage 3: Code generation
        StringBuilder sb = new();
        
        // Clear old stuff
        sb.AppendLine($"kill @e[tag={options.MarkerTag}]");
        
        foreach (Statement s in statements)
        {
            sb.AppendCommands(rootScope, s.Generate(options));
        }
        // Free all data in root scope
        //sb.AppendCommands(rootScope, rootScope.GenerateScopeDeallocation(options));
        
        // Create extra functions for statements

        datapack.Functions.AddRange(options.ExtraFunctionsToGenerate);
        var loadFunction = new Function("load", sb.ToString());
        datapack.Functions.Add(loadFunction);
        datapack.OnLoadFunction = loadFunction;
        
        var tickFunction = options.ExtraFunctionsToGenerate.FirstOrDefault(x => x.Name == "tick");
        if (tickFunction != null) datapack.OnTickFunction = tickFunction;

        Console.WriteLine($"Variable translation table in root scope: {string.Join(", ", rootScope.Variables.Select(x => $"{x.Key} -> {x.Value.Generate(options)}"))}");
        
        return datapack;
    }

    private static TR MeasureTime<TR>(string regionName, Func<TR> action)
    {
        Stopwatch sw = Stopwatch.StartNew();
        TR r = action();
        sw.Stop();
        Console.WriteLine($"{regionName} took {sw.Elapsed.TotalMilliseconds}ms");
        return r;
    }
    
    private static void MeasureTime(string regionName, Action action)
    {
        Stopwatch sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        Console.WriteLine($"{regionName} took {sw.Elapsed.TotalMilliseconds}ms");
    }
}