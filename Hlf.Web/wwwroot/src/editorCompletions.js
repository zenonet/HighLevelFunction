import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
import {throwSymbols, getAllBuiltinFunctionDefinitions, throwExpressionInfo} from "./hlf";

let functionDefinitions;


const statementCompletions = [
    {
        label: 'if',
        kind: monaco.languages.CompletionItemKind.Keyword,
        documentation: "An if condition",
        insertText: 'if(${1:true}){\n    $2\n}',
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
    },
    {
        label: 'while',
        kind: monaco.languages.CompletionItemKind.Keyword,
        documentation: "A while loop",
        insertText: 'while(${1:true}){\n    $2\n}',
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
    },
    {
        label: 'for',
        kind: monaco.languages.CompletionItemKind.Keyword,
        documentation: "A for loop",
        insertText: 'for(${1:int i = 0}; ${2:i < 5}; ${3:i++}){\n    $4\n}',
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
    },
    {
        label: 'val',
        kind: monaco.languages.CompletionItemKind.Keyword,
        insertText: 'val ',
    },
    {
        label: 'var',
        kind: monaco.languages.CompletionItemKind.Keyword,
        insertText: 'var ',
    },
];

const rootScopeStatementCompletions = [
    {
        label: 'void',
        kind: monaco.languages.CompletionItemKind.Keyword,
        documentation: "A function definition",
        insertText: 'void ${1:main}(){\n' +
            '    $2\n' +
            '}',
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
    },
    {
        label: 'struct',
        kind: monaco.languages.CompletionItemKind.Keyword,
        documentation: "A struct type definition",
        insertText: 'struct ${1:MyStruct}{\n    $2\n}',
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
    },
];


const expressionCompletions = [
    {
        label: 'new',
        kind: monaco.languages.CompletionItemKind.Keyword,
        insertText: 'new ',
    },
];

function getSymbolsAtPosition(model, tokens, position){
    
    var word = model.getWordUntilPosition(position);
    position = {
        lineNumber: position.lineNumber,
        column: word.startColumn,
    }

    const offset = model.getOffsetAt(position);
    const src = model.getValue();
    
    const tokensBefore = monaco.editor.tokenize(src.substring(0, offset), "hlf");
    const tokensAfter = monaco.editor.tokenize(src.substring(offset), "hlf");
    
    let startOffset = -1;
    for (let line = tokensBefore.length-1; line >= 0; line--) {
        for (let tokenI = tokensBefore[line].length-1; tokenI >= 0; tokenI--){
            const token = tokensBefore[line][tokenI];
            
            if(token !== undefined && token.type.startsWith("delimiter") && !token.type.startsWith("delimiter.dot")){
                startOffset = model.getOffsetAt({
                    column: token.offset,
                    lineNumber: line+1,
                })+2;
                console.log(`Found ${token.type} in line ${line} and column ${token.offset} which translates to offset of ${startOffset}`);
                break;
            }
        }
        if(startOffset !== -1) break;
    }
    
    let endOffset = -1;
    for (let line = 0; line < tokensAfter.length; line++) {
        for (const token of tokensAfter[line]) {

            if(token !== undefined && token.type.startsWith("delimiter.curly.closing")){
                endOffset = model.getOffsetAt(line+1, token.offset) + offset;
                break;
            }
        }
        if(endOffset !== -1) break;
    }
    
    
    const preparedSource = src.substring(0, startOffset) + "throwSymbols;" + src.substring(endOffset, src.length-1)
    console.log(preparedSource);

    //const symbolData = HlfTranspilerJs.transpileToString(preparedSource);
    return throwSymbols(preparedSource)
}

function generateSuggestionFromFunctionOverload(overload, addSemicolonOnInsert = false){
    const regex = /(?<=^(\w+) (.*)\(.*,?)(?:(\w+) (\w+)|(?<=\()\))/g;

    const matches = Array.from(overload.matchAll(regex));
    if(matches === null || matches[0] === undefined) {
        console.log("Couldnt parse: " + overload);
        return null;
    }
    
    const parameterCount = matches[0][0] === ")" ? 0 : matches.length;
    const returnType = matches[0][1];
    const functionName = matches[0][2];
    const parameterNames = matches.map(x => x[4]).filter(x => x !== undefined);
    console.log(parameterNames);
    let i = 1;
    let insert = functionName + "(" +  parameterNames.map(x => "${" + i++ + ":" + x + "}").join(", ") + ")" + ((addSemicolonOnInsert && returnType === "void") ? ";" : "");

    let overloadWithoutReturnType = overload.substring(overload.indexOf(" ")+1);

    return {
        label: overloadWithoutReturnType,
        kind: monaco.languages.CompletionItemKind.Function,
        sortText: "9",
        insertText: insert,
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
    };
}

let symbolData;
monaco.languages.registerCompletionItemProvider("hlf", {
    triggerCharacters: ['.'],
    provideCompletionItems: function (model, position) {
        
        const _tokens = monaco.editor.tokenize(model.getValue(), "hlf").flat();
        let d = getSymbolsAtPosition(model, _tokens, position);
        if(d.success) symbolData = d;
        
        var word = model.getWordUntilPosition(position);
        console.log(word.word);
        var codeBefore = model.getValueInRange({
            startLineNumber: 1,
            startColumn: 1,
            endLineNumber: position.lineNumber,
            endColumn: word.startColumn,
        });
        // Tokenize using monarch tokenizer and add line number to tokens
        const lines = monaco.editor.tokenize(codeBefore, "hlf")
        for (const line in lines) {
            lines[line].forEach(token => {
                token.line = Number(line)
            })
        }
        const allTokens = lines.flat();
        
        const tokens = allTokens.filter((token) => token.type !== "white.hlf" && token.type !== "");
        console.log(tokens.at(-1))
        var range = {
            startLineNumber: position.lineNumber,
            endLineNumber: position.lineNumber,
            startColumn: word.startColumn,
            endColumn: word.endColumn,
        };


        let suggestions = [];

        const lastToken = tokens.at(-1);
        // Statements and expressions
        if(lastToken === undefined || (lastToken.type.startsWith("delimiter") && !lastToken.type.startsWith("delimiter.dot"))){
            
            if(functionDefinitions === undefined) {
                functionDefinitions = getAllBuiltinFunctionDefinitions();
            }
            
            // If statement is valid
            let isStatement = false;
            if(lastToken === undefined ||lastToken.type.startsWith("delimiter.semicolon") || lastToken.type.startsWith("delimiter.curly")){
                isStatement = true;
                statementCompletions.forEach(completion => {
                    completion.range = range;
                    completion.sortText = "4";
                    suggestions.push(completion);
                })

                rootScopeStatementCompletions.forEach(completion => {
                    completion.range = range;
                    completion.sortText = "3";
                    suggestions.push(completion);
                })
                
                // Add types
                symbolData?.types.forEach(type => {
                    suggestions.push({
                        label: type,
                        kind: monaco.languages.CompletionItemKind.Class,
                        insertText: type + " ",
                        range: range,
                        sortText: "2",
                    })
                })
            }
            
            
            if(!isStatement){
                expressionCompletions.forEach(completion => {
                    completion.range = range;
                    suggestions.push(completion);
                })
            }

            // Add custom functions
            symbolData?.functions.forEach(func => {
                const suggestion = generateSuggestionFromFunctionOverload(func, isStatement);
                suggestion.range = range;
                suggestions.push(suggestion);
            })

            // Add variables
            symbolData?.variables.forEach(variable => {
                const parts = variable.split(":");
                const name = parts[0];
                suggestions.push({
                    label: name,
                    sortText: "1",
                    kind: monaco.languages.CompletionItemKind.Variable,
                    insertText: name,
                    range: range,
                })
            })

            // Add builtin function definitions
            for (let def of functionDefinitions) {
                const suggestion = generateSuggestionFromFunctionOverload(def.overloads[0], isStatement);
                suggestion.description = def.description;
                suggestion.range = range;
                suggestions.push(suggestion);
            }

        }
        if(lastToken !== undefined && lastToken.type.startsWith("delimiter.dot")){
            // run expression analysis to suggest type members
            const offset = model.getOffsetAt({
                lineNumber: lastToken.line+1,
                column: lastToken.offset+1,
            })+1;
            const src = model.getValue();
            const preparedSource = [src.slice(0, offset), "throwMeta", src.slice(offset)].join('');
            console.log(preparedSource)
            const expressionInfo = throwExpressionInfo(preparedSource);
            
            expressionInfo.members.forEach(member => {
                suggestions.push({
                    label: member,
                    kind: monaco.languages.CompletionItemKind.Property,
                    insertText: member,
                },)
            })

            expressionInfo.methods.forEach(method => {
                const suggestion = generateSuggestionFromFunctionOverload(method, );
                //suggestion.description = def.description;
                suggestion.kind = monaco.languages.CompletionItemKind.Method;
                suggestion.range = range;
                suggestions.push(suggestion);
            })
        }
        
        console.log(`Generated ${suggestions.length} suggestions`);
        return {
            suggestions: suggestions,
        }
    },
})