namespace Hlf.Transpiler;

public class SymbolThrow(SymbolThrowData symbolData) : Exception
{
    public SymbolThrowData SymbolData = symbolData;
}

public record SymbolThrowData(
    bool Success,
    string? Error,
    List<string> Variables,
    List<string> Functions,
    List<string> Types
);