using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Hlf.Transpiler;

public static class Lexer
{
    private static ITokenDefinition[] TokenDefinitions =
    [
        new RawTokenDefinition(TokenType.OpenParenthesis, "("),
        new RawTokenDefinition(TokenType.CloseParenthesis, ")"),
        new RawTokenDefinition(TokenType.Comma, ","),
        new RawTokenDefinition(TokenType.Dot, "."),
        new RawTokenDefinition(TokenType.DoubleEquals, "=="),
        new RawTokenDefinition(TokenType.GreaterThan, ">"),
        new RawTokenDefinition(TokenType.LessThan, "<"),
        new RawTokenDefinition(TokenType.LessThanOrEqual, "<="),
        new RawTokenDefinition(TokenType.GreaterThanOrEqual, ">="),
        new RawTokenDefinition(TokenType.Equals, "="),
        new RawTokenDefinition(TokenType.Plus, "+"),
        new RawTokenDefinition(TokenType.Asterisk, "*"),
        new RawTokenDefinition(TokenType.Slash, "/"),
        
        new RawTokenDefinition(TokenType.Colon, ":"),
        new RawTokenDefinition(TokenType.Semicolon, ";"),
        
        new RegexTokenDefinition(TokenType.StringLiteral, """
                                                          ^(?:".*?(?:[^\\](?:\\\\)+"|[^\\]")|\$?""|\$"(?:[^{]|[^\\](?:\\\\)*\\{)*?(?:[^\\](?:\\\\)+"|(?:[^{\\]|[^\\](?:\\\\)*\\{)"))
                                                          """),
        new RegexTokenDefinition(TokenType.StartStringInterpolationLiteral, """
                                                                            ^(?:\$".*?(?:[^\\](?:\\\\)+{|[^\\]{)|\$"{)
                                                                            """),
        new RegexTokenDefinition(TokenType.CenterStringInterpolationLiteral, """
                                                                             ^(?:}.*?(?:[^\\](?:\\\\)+{|[^\\]{))
                                                                             """),
        
        new RegexTokenDefinition(TokenType.EndStringInterpolationLiteral, """
                                                                          ^(?:}.*?(?:[^\\](?:\\\\)+"|[^\\]")|}")
                                                                          """),
        new RawTokenDefinition(TokenType.OpenBrace, "{"),
        new RawTokenDefinition(TokenType.CloseBrace, "}"),
        
        new SelectorTokenDefinition(),
        
        new RegexTokenDefinition(TokenType.FloatLiteral, @"(?:-?(?:(\.\d+|\d+\.\d+)[fFdD]?|(\d+)[fFdD]))"),
        new RegexTokenDefinition(TokenType.IntLiteral, @"-?\d+"),
        new RegexTokenDefinition(TokenType.Identifier, @"\w+"),
        new RegexTokenDefinition(TokenType.PersistentComment, @"\/#(.*)"),
        
        new RawTokenDefinition(TokenType.Minus, "-"),
    ];

    public static TokenList Lex(ReadOnlySpan<char> src)
    {
        TokenList tokens = new();

        LexerState state = new();

        while (src.Length > 0)
        {
            // skip whitespace
            int skipped = 0;
            bool isSingleLineComment = false;
            int multiLineCommentDepth = 0;
            while (skipped < src.Length && (isSingleLineComment || multiLineCommentDepth > 0 || char.IsWhiteSpace(src[skipped]) || (src[skipped] == '/' && src[skipped + 1] is '/' or '*') || (src[skipped] is '*' && src[skipped+1] is '/')))
            {

                state.Column++;
                char c = src[skipped];
                skipped++;
                if (c == '/')
                {
                    if(src[skipped] is '*')
                    {
                        multiLineCommentDepth++;
                        skipped++;
                        state.Column++;
                    }
                    else isSingleLineComment = true;
                }

                if (multiLineCommentDepth > 0 && c == '*' && src[skipped] == '/')
                {
                    multiLineCommentDepth--;
                    skipped++;
                    state.Column++;
                }
                
                if (c == '\n')
                {
                    state.Line++;
                    state.Column = 1;
                    isSingleLineComment = false;
                }
            }
            src = src[skipped..];
            if (src.IsEmpty) break;

            state.Source = src.ToString();
            foreach (ITokenDefinition def in TokenDefinitions)
            {
                if (def.TryInterpret(ref state, out Token? token))
                {
                    tokens.Push(token);
                    goto success;
                }
            }
            // failure
            throw new LanguageException($"Unknown Token", state.Line, state.Column);
            
            success:
            src = state.Source;
        }
        
        tokens.Length = tokens._Tokens.Count;
        return tokens;
    }


    private interface ITokenDefinition
    {
        bool TryInterpret(ref LexerState state, [NotNullWhen(true)] out Token? token);
    }

    private class RawTokenDefinition(TokenType type, string pattern) : ITokenDefinition
    {
        public bool TryInterpret(ref LexerState state, [NotNullWhen(true)] out Token? token)
        {
            if (state.Source.StartsWith(pattern, StringComparison.Ordinal))
            {
                token = new()
                {
                    Type = type,
                    Content = pattern,
                    Column = state.Column,
                    Line = state.Line,
                };
                state.Source = state.Source[pattern.Length..];
                state.Column += pattern.Length;
                return true;
            }

            token = null;
            return false;
        }
    }

    private class RegexTokenDefinition : ITokenDefinition
    {
        private Regex _regex;
        private TokenType _type;

        public RegexTokenDefinition(TokenType type, string pattern)
        {
            if (!pattern.StartsWith('^')) pattern = $"^{pattern}";
            _regex = new(pattern);
            _type = type;
        }

        public bool TryInterpret(ref LexerState state, [NotNullWhen(true)] out Token? token)
        {
            Match match = _regex.Match(state.Source);

            if (!match.Success)
            {
                token = null;
                return false;
            }
            
            string content = match.Groups.Count > 1 ? match.Groups.Values.Skip(1).First(x => x.Success).Value : match.Value;

            token = new()
            {
                Content = content,
                Column = state.Column,
                Line = state.Line,
                Type = _type,
            };

            state.Source = state.Source[match.Length..];
            state.Column += match.Length;
            return true;
        }
    }

    private class SelectorTokenDefinition : ITokenDefinition
    {
        public bool TryInterpret(ref LexerState state, [NotNullWhen(true)] out Token? token)
        {
            if (!state.Source.StartsWith('@'))
            {
                token = null;
                return false;
            }
            if(!"aenprs".Contains(state.Source[1])) throw new LanguageException("Invalid selector: Expected a, e, n, p, r or s after @", state.Line, state.Column);
            int offset = 2;
            if (state.Source[2] == '[')
            {
                offset++;
                int opened = 1;
                for (;offset < state.Source.Length && opened != 0;offset++)
                {
                    if (state.Source[offset] == '\n') throw new LanguageException("Invalid syntax at selector", state.Line, state.Column);
                    if(state.Source[offset] == '[') opened++;
                    if(state.Source[offset] == ']') opened--;
                }

                if (opened == 0)
                {
                    token = new()
                    {
                        Line = state.Line,
                        Column = state.Column,
                        Type = TokenType.Selector,
                        Content = state.Source[..(offset)],
                    };
                    state.Column += offset;
                    state.Source = state.Source[offset..];
                    return true;
                }

                throw new LanguageException("Invalid syntax at selector", state.Line, state.Column);
            }

            token = new()
            {
                Line = state.Line,
                Column = state.Column,
                Type = TokenType.Selector,
                Content = state.Source[..offset],
            };
            state.Column += offset;
            state.Source = state.Source[offset..];
            return true;
        }
    }

    private ref struct LexerState()
    {
        public string Source;
        public int Line = 1;
        public int Column = 1;
    }
}