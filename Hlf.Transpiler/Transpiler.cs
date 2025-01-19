using System.Diagnostics;
using System.Text;
using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.DatapackGen;

namespace Hlf.Transpiler;

public class Transpiler
{
    public Datapack Transpile(string sourceCode, GeneratorOptions options)
    {
        ResetAllocationCounters();
        
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
        sb.AppendLine("forceload add 0 0");

        List<VariableAssignment> globalVarInitializers = [];
        foreach (Statement s in statements)
        {
            if (s is not VariableDeclaration and not FunctionDefinitionStatement)
            {
                throw new LanguageException("Only variable declarations and function definitions are allowed as top-level statements!", s.Line, s.Column);
            }

            if (s is VariableDeclaration {Assignment: {} assignment} declaration)
            {
                // The declaration shouldn't generate that here
                declaration.Assignment = null;
                globalVarInitializers.Add(assignment);
            }
            sb.AppendCommands(rootScope, s.Generate(options));
        }
        // Free all data in root scope
        //sb.AppendCommands(rootScope, rootScope.GenerateScopeDeallocation(options));
        
        // Create extra functions for statements

        List<(string, string)> extraFunctions = options.ExtraFunctionsToGenerate;
        foreach ((string name, string code) in extraFunctions)
        {
            string processedCode = PostProcessCode(code, options);
            
            if(name is "main" or "load")
            {
                // Generate initializers for global variables here
                sb.SmartAppendL(options.Comment("Initializing global variables declared in root scope:"));
                globalVarInitializers.ForEach(x => sb.SmartAppendL(PostProcessCode(x.Generate(options), options)));
                // Create obsidian block around block storage
                if(BlockTypeDataId.counter > 0)
                    sb.SmartAppendL($"fill {BlockTypeDataId.GetReservedAreaBox(options)} obsidian");
                
                if(options.GenerateComments) sb.AppendLine(); // Actually an intentional line break here
                sb.Append('\n' + processedCode);
                continue;
            }
            
            Function function = new(name, processedCode);
            if (name is "tick") datapack.OnTickFunction = function;
            
            datapack.Functions.Add(function);
        }
        
        var combinedLoadFunction = new Function("load", sb.ToString());
        datapack.Functions.Add(combinedLoadFunction);
        datapack.OnLoadFunction = combinedLoadFunction;

        Console.WriteLine($"Variable translation table in root scope: {string.Join(", ", rootScope.Variables.Select(x => $"{x.Key} -> {x.Value.Generate(options)}"))}");
        
        return datapack;
    }

    private static string PostProcessCode(string code, GeneratorOptions options)
    {
        return code.Replace(";fr:", "");
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

    public static void ResetAllocationCounters()
    {
        NbtDataId.counter = 0;
        EntityDataId.counter = 0;
        BlockTypeDataId.counter = 0;
        WhileLoop.loopCounter = 0;
        ForLoop.loopCounter = 0;
        StringInterpolationExpression.InterpolationMacroFunctionCounter = 0;
    }
}