using Hlf.Transpiler.Values;

namespace Hlf.Transpiler;

public class StructType(string? name, ValueKind kind, Conversion[]? implicitConversions = null, OperationDefinition[]? operations = null) : HlfType(name, kind, implicitConversions, operations)
{
    public List<(Member member, string memberName, Statement intializer)> Initializers = [];
}