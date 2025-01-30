import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
import bootsharp, {Hlf, HlfTranspilerJs} from "hlf-transpiler";

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



monaco.languages.registerCompletionItemProvider("hlf", {
    triggerCharacters: [],
    provideCompletionItems: function (model, position) {
        
        var word = model.getWordUntilPosition(position);
        console.log(word.word);
        var codeBefore = model.getValueInRange({
            startLineNumber: 1,
            startColumn: 1,
            endLineNumber: position.lineNumber,
            endColumn: word.startColumn,
        });
        // Tokenize using monarch tokenizer
        const allTokens = monaco.editor.tokenize(codeBefore, "hlf").flat();
        const tokens = allTokens.filter((token) => token.type !== "white.hlf" && token.type !== "");

        var range = {
            startLineNumber: position.lineNumber,
            endLineNumber: position.lineNumber,
            startColumn: word.startColumn,
            endColumn: word.endColumn,
        };


        let suggestions = [];

        const lastToken = tokens.at(-1);
        // Statements and expressions
        if(lastToken.type.startsWith("delimiter") && !lastToken.type.startsWith("delimiter.dot")){
            
            if(functionDefinitions === undefined) {
                functionDefinitions = HlfTranspilerJs.getAllBuiltinFunctionDefinitions();
            }
            
            // If statement is valid
            let isStatement = false;
            if(lastToken.type.startsWith("delimiter.semicolon") || lastToken.type.startsWith("delimiter.curly")){
                isStatement = true;
                statementCompletions.forEach(completion => {
                    completion.range = range;
                    suggestions.push(completion);
                })

                rootScopeStatementCompletions.forEach(completion => {
                    completion.range = range;
                    suggestions.push(completion);
                })
            }
            
            
            if(!isStatement){
                expressionCompletions.forEach(completion => {
                    completion.range = range;
                    suggestions.push(completion);
                })
            }
            

            for (let def of functionDefinitions) {
                const overload = def.overloads[0];
                const regex = /(?<=^(\w+) (.*)\(.*,?)(?:(\w+) (\w+)|(?<=\()\))/g;
                
                const matches = Array.from(overload.matchAll(regex));
                if(matches === null) {
                    console.log("Couldnt parse: " + overload);
                    continue;
                }
                const parameterCount = matches[0][0] === ")" ? 0 : matches.length;
                const returnType = matches[0][1];
                const functionName = matches[0][2];
                const parameterNames = matches.map(x => x[4])
                console.log(parameterNames);

                let i = 0;
                let insert = functionName + "(" +  parameterNames.map(x => "${" + i++ + ":" + x + "}").join(", ") + ")" + (isStatement ? ";" : "");
                
                let overloadWithoutReturnType = def.overloads[0].substring(def.overloads[0].indexOf(" "));
                
                suggestions.push({
                    label: overloadWithoutReturnType,
                    documentation: def.description,
                    kind: monaco.languages.CompletionItemKind.Function,
                    insertText: insert,
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    range: range,
                });
            }
            

        }



        return {
            suggestions: suggestions,
        }
    },
})