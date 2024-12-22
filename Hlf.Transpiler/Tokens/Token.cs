namespace Hlf.Transpiler;

public class Token
{
    public string Content { get; init; }
    public TokenType Type { get; init; }
    public int Line { get; init; }
    public int Column { get; init; }

    public override string ToString() => $"{Type}: '{Content}' (line {Line}, column {Column})";

    public static bool IsOperatorToken(TokenType type) => type is TokenType.DoubleEquals or TokenType.Asterisk or TokenType.Plus or TokenType.Minus or TokenType.Slash or TokenType.Percent;

    public static int GetOperatorPrecedence(TokenType type)
    {
        return type switch
        {
            TokenType.DoubleEquals => 5,
            
            TokenType.Asterisk => 2,
            TokenType.Slash => 2,
            TokenType.Percent => 2,
            
            TokenType.Plus => 1,
            TokenType.Minus => 1,
            
            _ => throw new ArgumentException($"Token type {type} is not an operator or its precedence hasn't been set"),
        };
    }
}

public enum TokenType
{
    Identifier,
    StringLiteral,
    IntLiteral,
    FloatLiteral,
    BoolLiteral,
    OpenParenthesis,
    CloseParenthesis,
    OpenBrace,
    CloseBrace,
    OpenBracket,
    CloseBracket,
    
    Comma,
    Semicolon,
    Colon,
    Dot,
    Equals,
    DoubleEquals,
    
    Plus,
    Minus,
    Asterisk,
    Slash,
    Percent,
    PersistentComment,
}