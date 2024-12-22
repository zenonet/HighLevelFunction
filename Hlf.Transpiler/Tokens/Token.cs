namespace Hlf.Transpiler;

public class Token
{
    public string Content { get; init; }
    public TokenType Type { get; init; }
    public int Line { get; init; }
    public int Column { get; init; }

    public override string ToString() => $"{Type}: '{Content}' (line {Line}, column {Column})";

    public static bool IsOperatorToken(TokenType type) => type is TokenType.DoubleEquals or TokenType.Asterisk or TokenType.Plus or TokenType.Minus or TokenType.Slash or TokenType.Percent or TokenType.GreaterThan or TokenType.GreaterThanOrEqual or TokenType.LessThan or TokenType.LessThanOrEqual;

    public static int GetOperatorPrecedence(TokenType type)
    {
        return type switch
        {
            TokenType.DoubleEquals => 1,
            TokenType.LessThan => 1,
            TokenType.GreaterThan => 1,
            TokenType.LessThanOrEqual => 1,
            TokenType.GreaterThanOrEqual => 1,
            
            TokenType.Asterisk => 5,
            TokenType.Slash => 5,
            TokenType.Percent => 5,
            
            TokenType.Plus => 2,
            TokenType.Minus => 2,
            
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
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    
    Plus,
    Minus,
    Asterisk,
    Slash,
    Percent,
    PersistentComment,
}