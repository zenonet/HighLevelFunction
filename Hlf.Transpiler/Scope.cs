using System.Diagnostics.CodeAnalysis;
using System.Text;
using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class Scope
{
    public Dictionary<string, DataId> Variables { get; } = new();
    public Dictionary<string, CustomFunctionDefinition> FunctionDefinitions { get; } = new();

    public Scope? Parent
    {
        get => field;
        set
        {
            Depth = value == null ? 0 : value.Depth+1;
            field = value;
        }
    }

    public ScopeType Type = ScopeType.Default;
    public Loop? ClosestLoop;

    public int Depth { get; set; }
    
    public bool TryGetVariable(string name, [NotNullWhen(true)]out DataId? variable)
    {
        if (Variables.TryGetValue(name, out variable))
            return true;
        
        Parent?.TryGetVariable(name, out variable);
        return variable != null;
    }
    
    public bool TryGetFunction(string name, [NotNullWhen(true)]out CustomFunctionDefinition? functionDefinition)
    {
        if (FunctionDefinitions.TryGetValue(name, out functionDefinition))
            return true;
        
        Parent?.TryGetFunction(name, out functionDefinition);
        return functionDefinition != null;
    }

    public DataId? AddVariable(string variableName, HlfType hlfType)
    {
        DataId dataId = hlfType.NewDataId();
        dataId.IsVariable = true;
        Variables.Add(variableName, dataId);
        return dataId;
    }

    public string ConditionPrefix ="";
    public string ApplyScopedPrefixes(string commands)
    {
        if(ConditionPrefix.Length > 0)
            return Utils.CommandPrefixRegex().Replace(commands, $"{ConditionPrefix} $&");
        return commands;
    }

    public string GenerateScopeDeallocation(GeneratorOptions gen)
    {
        StringBuilder sb = new();
        if(Variables.Count > 0)
            sb.AppendLine(gen.Comment($"# Scope deallocation for scope at depth {Depth}"));
        foreach ((string name, DataId variable) in Variables)
        {
            sb.AppendCommands(this, variable.Free(gen));
        }
        return sb.ToString();
    }

    

    public Scope NewChildScope(ScopeType type = default)
    {
        return new() {Parent = this, Type = type, ClosestLoop = ClosestLoop};
    }
}

public enum ScopeType
{
    Default,
    Loop,
    Function,
}