using System.Collections;

namespace Hlf.Transpiler;

public class TokenList : IEnumerable<Token>
{
    internal List<Token> _Tokens { get; set; } = new();

    public int StartIndex { get; set; } = 0;
    public int Length { get; set; }
    public int EndIndex => StartIndex + Length;
    public bool IsEmpty => Length == 0;

    public Token Peek() => _Tokens[StartIndex];
    public Token? PeekOrNull() => IsEmpty ? null : _Tokens[StartIndex];
    public Token Peek(int offset) => _Tokens[StartIndex+offset];

    public Token Pop()
    {
        Token token = _Tokens[StartIndex++];
        Length--;
        return token;
    }

    public List<Token> ToList() => _Tokens[StartIndex..EndIndex];

    public bool StartsWith(TokenType tokenType) => Length > 0 && _Tokens[StartIndex].Type == tokenType;
    public bool StartsWithSequence(params TokenType[] tokenTypes)
    {
        if(tokenTypes.Length > Length) return false;
        for (int i = 0; i < tokenTypes.Length; i++)
        {
            if (tokenTypes[i] != _Tokens[StartIndex+i].Type) return false;
        }
        return true;
    }

    public void Push(Token token) => _Tokens.Add(token);

    public TokenList GetBetweenParentheses(TokenType openingTokenType, TokenType closingTokenType)
    {
        int opened = 0;
        int i = StartIndex;
        
        do
        {
            if (i > EndIndex || i >= _Tokens.Count)
            {
                throw new LanguageException($"Invalid {openingTokenType.ToString().Replace("Type", "")} pattern", Peek());
            }
            if(_Tokens[i].Type == openingTokenType) opened++;
            else if(_Tokens[i].Type == closingTokenType) opened--;
            i++;
        }while(opened != 0);

        i--;
        return new()
        {
            _Tokens = _Tokens,
            StartIndex = StartIndex+1,
            Length = i-StartIndex-1,
        };
    }
    
    public TokenList PopBetweenParentheses(TokenType openingTokenType, TokenType closingTokenType)
    {
        TokenList between = GetBetweenParentheses(openingTokenType, closingTokenType);
        StartIndex += between.Length+2;
        Length -= between.Length+2;
        return between;
    }
    

    public IEnumerator<Token> GetEnumerator() => _Tokens[StartIndex..EndIndex].GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _Tokens[StartIndex..EndIndex].GetEnumerator();
}