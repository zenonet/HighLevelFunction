using Hlf.Transpiler.CodeGen;
using Hlf.Transpiler.DatapackGen;

namespace Hlf.Transpiler;

public abstract class Statement
{
    public int Line { get; set; }
    public int Column { get; set; }
    public int Length { get; set; }
    public bool IsReturnValueNeeded { get; set; } = true;

    public virtual bool NeedsSemicolon => true;
    public Scope ParentScope;
    public DataId Result { get; set; } = VoidDataId.Void;
    public Function[]? ExtraFunctionsToGenerate { get; set; } = null;
    public abstract void Parse();

    public abstract string Generate(GeneratorOptions options);
}