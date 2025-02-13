﻿
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
        Token? unaryOperator = null;
        if (tokens.StartsWith(TokenType.ExclamationMark))
        {
            unaryOperator = tokens.Pop();
        }
        
        Statement statement = ParseStatementLvl1(ref tokens, scope);

        if (unaryOperator != null)
        {
            statement = new UnaryOperator
            {
                Line = statement.Line,
                Column = statement.Column,
                Operand = statement,
                ParentScope = scope,
                Operator = unaryOperator,
            };
        }
        
        if(!tokens.StartsWith(TokenType.Dot)) return statement;

        // Resolve type member path
        tokens.Pop();
        
        // Find end of path
        int startIndex = tokens.StartIndex;
        while(true)
        {
            if (!tokens.StartsWith(TokenType.Identifier)) throw new LanguageException("Expected identifier after dot", tokens.Peek(-1));
            tokens.Pop();

            if (tokens.StartsWith(TokenType.OpenParenthesis))
            {
                tokens.PopBetweenParentheses(TokenType.OpenParenthesis, TokenType.CloseParenthesis);
            }
            
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

            if (ThrowDefinedSymbolsStatement.AllowSymbolThrows && identifier.Content == "throwMeta")
            {
                return new ThrowExpressionInfoStatement
                {
                    Line = identifier.Line,
                    Column = identifier.Column,
                    ParentScope = scope,
                    Expression = statement,
                };
            }
            
            if (path.IsEmpty && tokens.StartsWith(TokenType.Equals))
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
                    ParentScope = scope,
                };
            }
            
            else if (path.StartsWith(TokenType.OpenParenthesis))
            {
                TokenList parameterListTokens = path.PopBetweenParentheses(TokenType.OpenParenthesis, TokenType.CloseParenthesis);

                // Parse parameters
                List<Statement> parameterStatements = [];
                while (parameterListTokens.Length > 0)
                {
                    Statement param = Parse(parameterListTokens, scope);
                    parameterStatements.Add(param);
                    if (parameterListTokens.StartsWith(TokenType.Comma))
                    {
                        parameterListTokens.Pop();
                        continue;
                    }
                }
                
                statement = new MethodCall
                {
                    BaseExpression = statement,
                    MethodName = identifier.Content,
                    Line = identifier.Line,
                    Column = identifier.Column,
                    ParentScope = scope,
                    Parameters = parameterStatements,
                };

            }else
            {
                statement = new MemberGetter
                {
                    BaseExpression = statement,
                    MemberName = identifier.Content,
                    Line = identifier.Line,
                    Column = identifier.Column,
                    ParentScope = scope,
                };
            }
            
            if(!path.IsEmpty)
            {
                if (!path.StartsWith(TokenType.Dot)) throw new LanguageException("Uh, what is happening? There is a member path parsing problem! Pls contact the developer!", statement.Line, statement.Column);
                path.Pop();
            }

        }

        if(statement is MemberGetter getter)
        if (tokens.StartsWithSequence(TokenType.Plus, TokenType.Plus) || tokens.StartsWithSequence(TokenType.Minus, TokenType.Minus))
        {
            getter.Increment = (sbyte) (tokens.Pop().Type == TokenType.Plus ? 1 : -1);
            tokens.Pop();
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
            if (tokens.Peek().Content == "new")
            {
                tokens.Pop();
                StructInstantiationStatement structInstantiation = new();
                InitStatement(structInstantiation);
                structInstantiation.StructName = tokens.Pop();
                tokens.Pop();
                if(!tokens.StartsWith(TokenType.CloseParenthesis)) throw new LanguageException("new-constructors with parameters are currently not supported", tokens.Peek(-1));
                tokens.Pop();
                return structInstantiation;
            }
            if (tokens.Peek().Content == "void") // TODO: Actually allow for return types
            {
                // Function definition
                tokens.Pop();
                string functionName = tokens.Pop().Content;
                TokenList parameterTokens = tokens.PopBetweenParentheses(TokenType.OpenParenthesis, TokenType.CloseParenthesis);
                
                List<(string, string)> parameters = new();
                while (!parameterTokens.IsEmpty)
                {
                    if (!parameterTokens.StartsWithSequence(TokenType.Identifier, TokenType.Identifier)) throw new LanguageException("Invalid syntax in parameter declaration", parameterTokens.Peek());
                    string type = parameterTokens.Pop().Content;
                    string name = parameterTokens.Pop().Content;
                    parameters.Add((name, type));
                    if (!parameterTokens.IsEmpty)
                    {
                        if (!parameterTokens.StartsWith(TokenType.Comma)) throw new LanguageException("Expected comma between parameter declarations", tokens.Peek());
                        parameterTokens.Pop();
                    } 
                }
                
                if(!tokens.StartsWith(TokenType.OpenBrace)) throw new LanguageException("Expected code block for function definition", tokens.Peek(-1));

                TokenList blockTokens = tokens.PopBetweenParentheses(TokenType.OpenBrace, TokenType.CloseBrace);
                Scope functionScope = scope.NewChildScope();
                List<Statement> block = ParseMultiple(ref blockTokens, functionScope);

                FunctionDefinitionStatement statement = new()
                {
                    Name = functionName,
                    Block = block,
                    FunctionScope = functionScope,
                    ParameterDefinitions = parameters,
                };
                InitStatement(statement);
                return statement;
            }
            
        }

        if (tokens.StartsWithSequence(TokenType.Identifier, TokenType.Identifier, TokenType.OpenBrace))
        {
            if (tokens.Peek().Content == "struct")
            {
                tokens.Pop();
                StructDefinitionStatement structDefinition = new();
                InitStatement(structDefinition);
                
                structDefinition.StructName = tokens.Pop().Content;
                structDefinition.Fields = [];
                TokenList betweenBraces = tokens.PopBetweenParentheses(TokenType.OpenBrace, TokenType.CloseBrace);
                while (!betweenBraces.IsEmpty)
                {
                    if (!betweenBraces.StartsWithSequence(TokenType.Identifier, TokenType.Identifier)) throw new LanguageException("Invalid syntax in struct definition", tokens.Peek());
                    
                    Token type = betweenBraces.Pop();
                    Token name = betweenBraces.Pop();
                    Statement? fieldInitializer = null;
                    if (betweenBraces.StartsWith(TokenType.Equals))
                    {
                        betweenBraces.Pop();
                        fieldInitializer = Parse(betweenBraces, scope);
                    }

                    if (!betweenBraces.StartsWith(TokenType.Semicolon)) throw new LanguageException("Expected semicolon after field declaration in struct definition", tokens.Peek(-1));
                    betweenBraces.Pop();
                    structDefinition.Fields.Add((name, type, fieldInitializer));
                }

                return structDefinition;
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
                        whileLoop.LoopScope.ClosestLoop = whileLoop;
                        TokenList blockTokens = tokens.PopBetweenParentheses(TokenType.OpenBrace, TokenType.CloseBrace);
                        whileLoop.Block = ParseMultiple(ref blockTokens, whileLoop.LoopScope);
                        return whileLoop;
                    case "for":
                        if (betweenParentheses.IsEmpty) throw new LanguageException("Expected condition for for statement", tokens.Peek());

                        var headerSections = betweenParentheses.Split(TokenType.Semicolon);
                        ForLoop forLoop = new();
                        forLoop.HeaderScope = scope.NewChildScope();
                        forLoop.LoopScope = forLoop.HeaderScope.NewChildScope(ScopeType.Loop);
                        forLoop.LoopScope.ClosestLoop = forLoop;
                        InitStatement(forLoop);

                        if(!headerSections[0].IsEmpty) forLoop.InitStatement = Parse(headerSections[0], forLoop.HeaderScope);
                        if(!headerSections[1].IsEmpty) forLoop.Condition = Parse(headerSections[1], forLoop.HeaderScope);
                        if(!headerSections[2].IsEmpty) forLoop.Increment = Parse(headerSections[2], forLoop.HeaderScope);

                        blockTokens = tokens.PopBetweenParentheses(TokenType.OpenBrace, TokenType.CloseBrace);
                        forLoop.Block = ParseMultiple(ref blockTokens, forLoop.LoopScope);
                        return forLoop;
                }
            }
            else
            {
                // Function call
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
                assignment.IsInitialization = true;
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
                tokens.Pop();
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

        if (tokens.StartsWith(TokenType.StartStringInterpolationLiteral))
        {
            List<Token> stringParts = [tokens.Pop(),];
            List<Statement> contentParts = [];

            while (true)
            {
                Statement content = ParseStatementLvl3(ref tokens, scope);
                contentParts.Add(content);

                if (tokens.StartsWith(TokenType.EndStringInterpolationLiteral))
                {
                    stringParts.Add(tokens.Pop());
                    break;
                }
                if (tokens.StartsWith(TokenType.CenterStringInterpolationLiteral))
                {
                    stringParts.Add(tokens.Pop());
                    continue;
                }
                throw new LanguageException("Expected string part after expression in string interpolation.", line, column);
            }

            var interpolation = new StringInterpolationExpression
            {
                ContentParts = contentParts,
                StringPartTokens = stringParts,
            };
            InitStatement(interpolation);
            return interpolation;
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

        if (tokens.StartsWith(TokenType.BoolLiteral))
        {
            var lit = new LiteralExpression(HlfType.Bool, tokens.Pop().Content == "true" ? "1b" : "0b");
            InitStatement(lit);
            return lit;
        }

        #endregion
        
        // Selector
        if (tokens.StartsWith(TokenType.Selector))
        {
            Token token = tokens.Pop();
            SelectorExpression selector = new();
            InitStatement(selector);
            selector.Selector = token.Content;
            return selector;
        }

        // Variable accessor
        if (tokens.StartsWith(TokenType.Identifier))
        {
            if (tokens.Peek().Content is "break" or "continue")
            {
                BreakStatement breakStatement = new()
                {
                    Type = tokens.Pop().Content == "break" ? ControlFlowStatementType.Break : ControlFlowStatementType.Continue,
                };
                InitStatement(breakStatement);
                return breakStatement;
            }
            
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
            if (ThrowDefinedSymbolsStatement.AllowSymbolThrows && tokens.Peek().Content is "throwSymbols")
            {
                Console.WriteLine("Parsing symbolThrow");
                statements.Add(new ThrowDefinedSymbolsStatement
                {
                    Line = tokens.Peek().Line,
                    Column = tokens.Peek().Column,
                    ParentScope = scope,
                });
                break;
            }

            Statement statement = Parse(tokens, scope);
            statement.IsReturnValueNeeded = false;
            statements.Add(statement);
            if (statement.NeedsSemicolon)
            {
                if (!tokens.StartsWith(TokenType.Semicolon))
                {
                    // Throw the error later so that the dev gets more useful exceptions first.
                    statements.Add(new ThrowErrorLater(new("Expected semicolon after statement.", tokens.Peek(-1))));
                    break;
                }
                tokens.Pop(); // semicolon
            }
        }

        return statements;
    }
}