using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class MethodCall : Statement
{
    public Statement BaseExpression;
    public string MethodName;
    public List<Statement> Parameters;

    private BuiltinFunctionDefinition methodDefinition;

    public override void Parse()
    {
        BaseExpression.Parse();
        HlfType type = BaseExpression.Result.Type;

        Parameters.ForEach(x => x.Parse());

        var overloads = type.Methods.Where(x => x.Name == MethodName).ToArray();

        if (overloads.Length < 1) throw new LanguageException($"Method {type.Name}.{MethodName} does not exist", Line, Column);

        BuiltinFunctionDefinition? o = overloads
            .Where(x => x.Parameters.Length-1 == Parameters.Count)
            .FirstOrDefault(x => !Parameters.Where((p, i) => p.Result.Type.IsAssignableTo(x.Parameters[i+1].Type)).Any());
            //.FirstOrDefault(x => !x.Parameters.Where((t, i) => !Parameters[i].Result.Type.IsAssignableTo(t.Type)).Any());

        if (o != null)
            methodDefinition = o;
        else
        {
            throw new LanguageException($"Overload for method with signature {type.Name}.{MethodName}({string.Join(", ", Parameters.Select(x => x.Result.Type.Name))}) could not be found.\n" +
                                        $"Available overloads of {type.Name}.{MethodName}:\n" +
                                        $"{string.Join('\n', overloads.Select(ov => $"    {type.Name}.{MethodName}({string.Join(", ", ov.Parameters.Select(x => x.Type.Name))})"))}", Line, Column, MethodName.Length);
        }


        if (methodDefinition.ReturnType.Kind != ValueKind.Void)
            Result = methodDefinition.ReturnType.NewDataId();
    }

    public override string Generate(GeneratorOptions gen)
    {
        if (methodDefinition == null) throw new ArgumentNullException();

        if (gen.TargetVersion < methodDefinition.MinVersion)
        {
            throw new LanguageException($"The {BaseExpression.Result.Type.Name}.{MethodName}() method requires a target version of at least {methodDefinition.MinVersion}", Line, Column, MethodName.Length);
        }
        
        StringBuilder sb = new();
        Dictionary<string, DataId> parameterDataIds = new();
        
        sb.SmartAppendL(BaseExpression.Generate(gen));
        parameterDataIds.Add("self", BaseExpression.Result);
        
        for (int i = 0; i < Parameters.Count; i++)
        {
            sb.AppendCommands(ParentScope, Parameters[i].Generate(gen));
            DataId parameterId = Parameters[i].Result.ConvertImplicitly(gen, methodDefinition.Parameters[i].Type, out string code);
            sb.AppendCommands(ParentScope, code);
            parameterDataIds.Add(methodDefinition.Parameters[i].Name, parameterId);
        }
        
        // Actual function code
        sb.SmartAppendL(methodDefinition.CodeGenerator.Invoke(gen, parameterDataIds, Result));
    
        // Free parameter values
        sb.SmartAppendL(gen.Comment("Freeing parameter values for function call.\n"));
        sb.SmartAppendL(string.Join("\n", parameterDataIds.Values.Union(Parameters.Select(x => x.Result)).Select(x => x.FreeIfTemporary(gen))));
        
        // Add dependency to required resource functions
        methodDefinition.Dependencies.ForEach(x => gen.DependencyResources.Add(x));
        return sb.ToString().TrimEnd('\n');
    }
}