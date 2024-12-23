
using System.Collections;

namespace Hlf.Transpiler;

public class Parser
{
    /*
     Levels:
     1: Single expressions
     2: Resolving member accessors
     3: Operators
    */


    public Statement Parse(TokenList tokens, Scope scope)
    {
        return ParseStatementLvl3(ref tokens, scope);
    }

    public Statement ParseStatementLvl3(ref TokenList tokens, Scope scope)
    {
        var s = ParseStatementLvl2(ref tokens, scope);

        if (tokens.IsEmpty || !Token.IsOperatorToken(tokens.Peek().Type)) return s;

        List<Statement> operands = [];
        List<Token> operators = [];
        
        operands.Add(s);
        operators.Add(tokens.Pop());

        while (!tokens.IsEmpty)
        {
            operands.Add(ParseStatementLvl2(ref tokens, scope));

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

    public Statement ParseStatementLvl2(ref TokenList tokens, Scope scope)
    {
        Statement statement = ParseStatementLvl1(ref tokens, scope);
        if(!tokens.StartsWith(TokenType.Dot)) return statement;

        // Resolve type member path
        tokens.Pop();
        
        // Find end of path
        int startIndex = tokens.StartIndex;
        while(true)
        {
            if (!tokens.StartsWith(TokenType.Identifier)) throw new LanguageException("Expected identifier after dot", tokens.Peek(-1));
            tokens.Pop();
            if(!tokens.StartsWith(TokenType.Dot)) break;
            tokens.Pop();
        }

        TokenList path = new()
        {
            _Tokens = tokens._Tokens, // Yep, we give that new tokenlist a reference to tokens of the original one.
                                      // This works because token lists are supposed to be immutable as soon as lexical analysis is over
            StartIndex = startIndex,
            Length = tokens.StartIndex - startIndex,
        };

        while (!path.IsEmpty)
        {
            Token identifier = path.Pop();
            
            if (tokens.StartsWith(TokenType.Equals))
            {
                tokens.Pop();
                Statement value = ParseStatementLvl3(ref tokens, scope);
                statement = new MemberSetter
                {
                    MemberName = identifier.Content,
                    BaseExpression = statement,
                    Expression = value,
                    Line = identifier.Line,
                    Column = identifier.Column,
                };
            }
            else
            {
                statement = new MemberGetter
                {
                    BaseExpression = statement,
                    MemberName = identifier.Content,
                    Line = identifier.Line,
                    Column = identifier.Column,
                };
            }
            
            if(!path.IsEmpty)
            {
                if (!path.StartsWith(TokenType.Dot)) throw new LanguageException("Uh, what is happening? There is a member path parsing problem! Pls contact the developer!", statement.Line, statement.Column);
                path.Pop();
            }

        }


        
        
        return statement;

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

        if (tokens.StartsWithSequence(TokenType.Identifier, TokenType.Identifier, TokenType.OpenParenthesis))
        {
            if (tokens.Peek().Content == "void") // TODO: Actually allow for return types
            {
                // Function definition
                tokens.Pop();
                string functionName = tokens.Pop().Content;
                TokenList parameterTokens = tokens.PopBetweenParentheses(TokenType.OpenParenthesis, TokenType.CloseParenthesis);
                
                if(!tokens.StartsWith(TokenType.OpenBrace)) throw new LanguageException("Expected code block for function definition", tokens.Peek(-1));

                TokenList blockTokens = tokens.PopBetweenParentheses(TokenType.OpenBrace, TokenType.CloseBrace);
                Scope functionScope = scope.NewChildScope();
                List<Statement> block = ParseMultiple(ref blockTokens, functionScope);

                FunctionDefinitionStatement statement = new()
                {
                    Name = functionName,
                    Block = block,
                    FunctionScope = functionScope,
                };
                InitStatement(statement);
                return statement;
            }
            
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
                        ifStatement.Condition = Parse(betweenParentheses, scope);
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
                        whileLoop.Condition = Parse(betweenParentheses, scope);
                        whileLoop.LoopScope = scope.NewChildScope(ScopeType.Loop);
                        TokenList blockTokens = tokens.PopBetweenParentheses(TokenType.OpenBrace, TokenType.CloseBrace);
                        whileLoop.Body = ParseMultiple(ref blockTokens, whileLoop.LoopScope);
                        return whileLoop;
                    case "for":
                        if (betweenParentheses.IsEmpty) throw new LanguageException("Expected condition for for statement", tokens.Peek());

                        var headerSections = betweenParentheses.Split(TokenType.Semicolon);
                        ForLoop forLoop = new();
                        forLoop.HeaderScope = scope.NewChildScope();
                        forLoop.LoopScope = forLoop.HeaderScope.NewChildScope(ScopeType.Loop);
                        InitStatement(forLoop);

                        if(!headerSections[0].IsEmpty) forLoop.InitStatement = Parse(headerSections[0], scope);
                        if(!headerSections[1].IsEmpty) forLoop.Condition = Parse(headerSections[1], scope);
                        if(!headerSections[2].IsEmpty) forLoop.Increment = Parse(headerSections[2], scope);

                        blockTokens = tokens.PopBetweenParentheses(TokenType.OpenBrace, TokenType.CloseBrace);
                        forLoop.Block = ParseMultiple(ref blockTokens, forLoop.LoopScope);
                        return forLoop;
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
                    Statement param = Parse(betweenParentheses, scope);
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
                assignment.Expression = Parse(tokens, scope);
                assignment.VariableName = declaration.VariableName;
                declaration.Assignment = assignment;
            }

            return declaration;
        }

        // Variable assignment
        if (tokens.StartsWith(TokenType.Identifier) && tokens.Length > 1)
        {
            if (tokens.Peek(1).Type == TokenType.Equals)
            {
                VariableAssignment assignment = new();
                InitStatement(assignment);
                assignment.VariableName = tokens.Pop().Content;
                tokens.Pop(); // Equals
                assignment.Expression = Parse(tokens, scope);
                return assignment;
            }

            if (tokens.Length > 2 && Token.IsOperatorToken(tokens.Peek(1).Type) && tokens.Peek(2).Type == TokenType.Equals)
            {
                VariableAssignment assignment = new();
                InitStatement(assignment);
                assignment.VariableName = tokens.Pop().Content;
                Token operatorToken = tokens.Pop();
                tokens.Pop(); // Equals
                
                VariableAccessor accessor = new();
                InitStatement(accessor);
                accessor.VariableName = assignment.VariableName;
                
                Statement expression = Parse(tokens, scope);
                Operation operation = new()
                {
                    A = accessor,
                    B = expression,
                    Op = operatorToken,
                };
                InitStatement(operation);
                assignment.Expression = operation;
                return assignment;
            }

            if (tokens.StartsWithSequence(TokenType.Identifier, TokenType.Plus, TokenType.Plus) || tokens.StartsWithSequence(TokenType.Identifier, TokenType.Minus, TokenType.Minus))
            {
                // Increment stuff
                VariableAccessor accessor = new();
                InitStatement(accessor);
                accessor.VariableName = tokens.Pop().Content;
                accessor.Increment = (sbyte) (tokens.Pop().Type == TokenType.Plus ? 1 : -1);
                return accessor;
            }
        }

        if (tokens.StartsWith(TokenType.OpenParenthesis))
        {
            TokenList betweenParentheses = tokens.PopBetweenParentheses(TokenType.OpenParenthesis, TokenType.CloseParenthesis);
            return Parse(betweenParentheses, scope);
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

        if (tokens.StartsWith(TokenType.PersistentComment))
        {
            PersistentComment comment = new()
            {
                Comment = tokens.Pop().Content,
            };
            InitStatement(comment);
            return comment;
        }

        throw new LanguageException("Invalid syntax.", tokens.Peek());
    }

    public List<Statement> ParseMultiple(ref TokenList tokens, Scope scope)
    {
        List<Statement> statements = new();
        while (tokens.Length > 0)
        {
            Token t = tokens.Peek();
            Statement statement = Parse(tokens, scope);
            statement.IsReturnValueNeeded = false;
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