using System.Diagnostics;
using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class Transpiler
{
    public string Transpile(string sourceCode)
    {
        // Fix messed up line ending characters
        sourceCode = sourceCode.Replace("\r\n", "\n");
        sourceCode = sourceCode.Replace("\r", "\n");
        
        // State 1: lexical analysis
        TokenList tokens = MeasureTime("Lexical analysis", () => Lexer.Lex(sourceCode));

        Scope rootScope = new();
        
        // Stage 2: Parsing
        List<Statement> statements = MeasureTime("Parsing",() => new Parser().ParseMultiple(ref tokens, rootScope));
        MeasureTime("Validation", () => statements.ForEach(x => x.Parse()));

        // Stage 3: Code generation
        StringBuilder sb = new();
        GeneratorOptions options = new();
        
        // Clear old stuff
        sb.AppendLine($"kill @e[tag={options.MarkerTag}]");
        
        MeasureTime("Code gen", () =>
        {
            foreach (Statement s in statements)
            {
                sb.AppendCommands(rootScope, s.Generate(options));
            }
        });
        
        // Free all data in root scope
        sb.AppendCommands(rootScope, rootScope.GenerateScopeDeallocation(options));

        return sb.ToString();
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