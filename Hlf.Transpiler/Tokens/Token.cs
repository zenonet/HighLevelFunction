namespace Hlf.Transpiler;

public class Token
{
    public string Content { get; init; }
    public TokenType Type { get; init; }
    public int Line { get; init; }
    public int Column { get; init; }

    public override string ToString() => $"{Type}: '{Content}' (line {Line}, column {Column})";

    public static bool IsOperatorToken(TokenType type) => type is TokenType.DoubleEquals or TokenType.Asterisk or TokenType.Plus or TokenType.Minus or TokenType.Slash or TokenType.Percent or TokenType.GreaterThan or TokenType.GreaterThanOrEqual or TokenType.LessThan or TokenType.LessThanOrEqual or TokenType.DoubleAnd or TokenType.DoublePipe;

    public static int GetOperatorPrecedence(TokenType type)
    {
        return type switch
        {
            TokenType.DoubleEquals => 2,
            TokenType.LessThan => 2,
            TokenType.GreaterThan => 2,
            TokenType.LessThanOrEqual => 2,
            TokenType.GreaterThanOrEqual => 2,
            
            TokenType.Asterisk => 5,
            TokenType.Slash => 5,
            TokenType.Percent => 5,
            
            TokenType.Plus => 3,
            TokenType.Minus => 3,
            
            TokenType.DoubleAnd => 1,
            TokenType.DoublePipe => 1,
            
            _ => throw new ArgumentException($"Token type {type} is not an operator or its precedence hasn't been set"),
        };
    }
}

public enum TokenType
{
    Identifier,
    StringLiteral,
    StartStringInterpolationLiteral, // eg.: $" bla bla {
    CenterStringInterpolationLiteral, // eg.: } and also {
    EndStringInterpolationLiteral, // eg.: } yeah"
    IntLiteral,
    FloatLiteral,
    BoolLiteral,
    OpenParenthesis,
    CloseParenthesis,
    OpenBrace,
    CloseBrace,
    OpenBracket,
    CloseBracket,
    ExclamationMark,
    Selector,
    
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
    DoubleAnd,
    DoublePipe,
    
    Plus,
    Minus,
    Asterisk,
    Slash,
    Percent,
    PersistentComment,
}