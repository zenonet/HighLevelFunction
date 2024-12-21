
using System.Collections;

namespace Hlf.Transpiler;

public class Parser
{
    /*
     Levels:
     1: Single expressions
     2: Operators
    */


    public Statement Parse(ref TokenList tokens, Scope scope)
    {
        return ParseStatementLvl2(ref tokens, scope);
    }

    public Statement ParseStatementLvl2(ref TokenList tokens, Scope scope)
    {
        var s = ParseStatementLvl1(ref tokens, scope);

        if (tokens.IsEmpty || !Token.IsOperatorToken(tokens.Peek().Type)) return s;

        List<Statement> operands = [];
        List<Token> operators = [];
        
        operands.Add(s);
        operators.Add(tokens.Pop());

        while (!tokens.IsEmpty)
        {
            operands.Add(ParseStatementLvl1(ref tokens, scope));

            if (!Token.IsOperatorToken(tokens.Peek().Type)) break;
            operators.Add(tokens.Pop());
        }

        while (operands.Count > 1)
        {
            for (int i = 0; i < operators.Count; i++)
            {
                int precedence = Token.GetOperatorPrecedence(operators[i].Type);

                if (i == operators.Count - 1 || precedence >= Token.GetOperatorPrecedence(operators[i + 1].Type))
                {
                    // Merge
                    Statement a = operands[i];
                    Statement b = operands[i + 1];

                    var op = new Operation
                    {
                        A = a,
                        B = b,
                        Op = operators[i],
                        ParentScope = scope,
                        Line = operands[i].Line,
                        Column = operands[i].Column,
                    };
                    
                    operands.RemoveAt(i+1);
                    operators.RemoveAt(i);
                    operands[i] = op;
                }
            }
        }
        
        return operands[0];
    }

    public Statement ParseStatementLvl1(ref TokenList tokens, Scope scope)
    {
        if (tokens.IsEmpty)
            throw new LanguageException("Expected expression", tokens._Tokens.Last());
        
        int line = tokens.Peek().Line;
        int column = tokens.Peek().Column;

        void InitStatement(Statement stmt)
        {
            stmt.Line = line;
            stmt.Column = column;
            stmt.ParentScope = scope;
        }

        if (tokens.StartsWithSequence(TokenType.Identifier, TokenType.OpenParenthesis))
        {
            string identifier = tokens.Pop().Content; // Pop identifier
            TokenList betweenParentheses = tokens.PopBetweenParentheses(TokenType.OpenParenthesis, TokenType.CloseParenthesis);

            if (tokens.StartsWith(TokenType.OpenBrace))
            {
                // Some block statement
                switch (identifier)
                {
                    case "if":
                        if (betweenParentheses.IsEmpty) throw new LanguageException("Expected condition for if statement", tokens.Peek());
                        IfStatement ifStatement = new();
                        InitStatement(ifStatement);
                        ifStatement.Condition = Parse(ref betweenParentheses, scope);
                        ifStatement.IfClauseScope = scope.NewChildScope();
                        TokenList codeBlockTokens = tokens.PopBetweenParentheses(TokenType.OpenBrace, TokenType.CloseBrace);
                        ifStatement.Block = ParseMultiple(ref codeBlockTokens, ifStatement.IfClauseScope);

                        if (tokens.PeekOrNull() is {Type: TokenType.Identifier, Content: "else"})
                        {
                            tokens.Pop();
                            if (!tokens.StartsWith(TokenType.OpenBrace))
                                throw new LanguageException("Expected block after else keyword", line, column);
                            TokenList elseBlockTokens = tokens.PopBetweenParentheses(TokenType.OpenBrace, TokenType.CloseBrace);
                            ifStatement.ElseClauseScope = scope.NewChildScope();
                            ifStatement.ElseBlock = ParseMultiple(ref elseBlockTokens, ifStatement.ElseClauseScope);
                        }
                        
                        return ifStatement;
                    case "while":
                        if (betweenParentheses.IsEmpty) throw new LanguageException("Expected condition for while statement", tokens.Peek());
                        WhileLoop whileLoop = new();
                        InitStatement(whileLoop);
                        whileLoop.Condition = Parse(ref betweenParentheses, scope);
                        whileLoop.LoopScope = scope.NewChildScope();
                        TokenList blockTokens = tokens.PopBetweenParentheses(TokenType.OpenBrace, TokenType.CloseBrace);
                        whileLoop.Body = ParseMultiple(ref blockTokens, whileLoop.LoopScope);
                        return whileLoop;
                }
            }
            else
            {
                // Function
                BuiltinFunctionCall functionCall = new();

                // Parse parameters
                List<Statement> parameterStatements = new();
                while (betweenParentheses.Length > 0)
                {
                    Statement param = Parse(ref betweenParentheses, scope);
                    parameterStatements.Add(param);
                    if (betweenParentheses.StartsWith(TokenType.Comma))
                    {
                        betweenParentheses.Pop();
                        continue;
                    }
                }

                functionCall.FunctionName = identifier;
                functionCall.Parameters = parameterStatements;
                InitStatement(functionCall);
                return functionCall;
            }
        }

        // Variable declaration
        if (tokens.StartsWithSequence(TokenType.Identifier, TokenType.Identifier))
        {
            VariableDeclaration declaration = new();
            InitStatement(declaration);
            declaration.TypeName = tokens.Pop().Content;
            declaration.VariableName = tokens.Pop().Content;

            // Combined declaration and assignment
            if (tokens.StartsWith(TokenType.Equals))
            {
                tokens.Pop();
                VariableAssignment assignment = new();
                InitStatement(assignment);
                assignment.Expression = Parse(ref tokens, scope);
                assignment.VariableName = declaration.VariableName;
                declaration.Assignment = assignment;
            }

            return declaration;
        }

        // Variable assignment
        if (tokens.StartsWithSequence(TokenType.Identifier, TokenType.Equals))
        {
            VariableAssignment assignment = new();
            InitStatement(assignment);
            assignment.VariableName = tokens.Pop().Content;
            tokens.Pop(); // Equals
            assignment.Expression = Parse(ref tokens, scope);
            return assignment;
        }


        #region Value literals

        if (tokens.StartsWith(TokenType.StringLiteral))
        {
            var lit = new LiteralExpression(new ConstDataId(HlfType.ConstString, tokens.Pop().Content[1..^1]));
            InitStatement(lit);
            return lit;
        }

        if (tokens.StartsWith(TokenType.IntLiteral))
        {
            var lit = new LiteralExpression(HlfType.Int, tokens.Pop().Content);
            InitStatement(lit);
            return lit;
        }

        if (tokens.StartsWith(TokenType.FloatLiteral))
        {
            var lit = new LiteralExpression(HlfType.Float, tokens.Pop().Content + 'd');
            InitStatement(lit);
            return lit;
        }

        #endregion

        // Variable accessor
        if (tokens.StartsWith(TokenType.Identifier))
        {
            VariableAccessor accessor = new();
            InitStatement(accessor);
            accessor.VariableName = tokens.Pop().Content;
            return accessor;
        }


        throw new LanguageException("Invalid syntax.", tokens.Peek());
    }

    public List<Statement> ParseMultiple(ref TokenList tokens, Scope scope)
    {
        List<Statement> statements = new();
        while (tokens.Length > 0)
        {
            Token t = tokens.Peek();
            Statement statement = Parse(ref tokens, scope);
            statements.Add(statement);
            if (statement.NeedsSemicolon)
            {
                if (!tokens.StartsWith(TokenType.Semicolon))
                    throw new LanguageException("Expected semicolon after statement.", tokens.Peek(-1));
                tokens.Pop(); // semicolon
            }
        }

        return statements;
    }
}