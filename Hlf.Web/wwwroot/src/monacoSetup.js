
//import * as monaco from 'monaco-editor';
import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
//const hlf = require("./hlf");
import { transpile, getFunctionData, loadDefinitions } from "hlf";
import {setFunctionDefinitions} from "./editorCompletions";

function getMonarchDef(){
    return {
        // Set defaultToken to invalid to see what you do not tokenize yet
        defaultToken: 'invalid',

        keywords: [
            "if", "while", "for", "struct", "new", "else", "val", "var", "break", "continue", "true", "false"
        ],

        typeKeywords: [
            "string", "int", "float", "bool", "Entity", "Vector", "BlockType", "void"
        ],

        operators: [
            "==", '*', '+', '-', '/', '&&', '||', '!', '='
        ],

        // we include these common regular expressions
        symbols:  /[><!~?:&|+\-*\/\^%]+/,

        // C# style strings
        escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,

        tokenizer: {

            root: [
                // identifiers and keywords
                [/[a-z_][\w$]*(?=\()/, { cases: {
                        '@keywords': 'keyword',
                        '@default': 'function'
                    }
                }],
                [/[a-z_][\w$]*/, { cases: {
                        '@typeKeywords': 'keyword',
                        '@keywords': 'keyword',
                        '@default': 'identifier' } }],

                // custom types
                [/[A-Z]\w*/, 'type.identifier'],  // to show class names nicely
                /*
                [/[A-Z]\w+/, {
                    cases: {'@typeKeywords': 'type.keyword', '@default': 'identifier'}
                }],
                */

                // whitespace
                { include: '@whitespace' },

                // selector
                [/@[aenprs](?:\[.*])?/, 'attribute.value'],

                // delimiters and operators
                [/}/, {
                    cases: {
                        '$S2==interpolatedstring': { token: 'string.quote', next: '@pop' },
                        '@default': '@brackets'
                    }
                }],
                [/[{()\[\]]/, '@brackets'],
                [/[<>](?!@symbols)/, '@brackets'],
                [/@symbols/, { cases: { '@operators': 'delimiter',
                        '@default'  : '' } } ],

                // numbers
                [/-?(?:(\.\d+|\d+\.\d+)[fFdD]?|(\d+)[fFdD])/, 'number.float'],
                [/-?\d+/, 'number'],

                // delimiter: after number because of .\d floats
                ['=', 'delimiter.equals'],
                [',', 'delimiter.comma'],
                [';', 'delimiter.semicolon'],
                ['\\.', {token: 'delimiter.dot', next:'@afterDot'}],

                // strings
                [/\$"/, { token: 'string.quote', next: '@interpolatedstring' }],
                [/"([^"\\]|\\.)*$/, 'string.invalid' ],  // non-teminated string
                [/"/,  { token: 'string.quote', bracket: '@open', next: '@string' } ],
            ],

            comment: [
                [/[^\/*]+/, 'comment' ],
                [/\/\*/,    'comment', '@push' ],    // nested comment
                ["\\*/",    'comment', '@pop'  ],
                [/[\/*]/,   'comment' ]
            ],

            string: [
                [/[^\\"]+/,  'string'],
                [/@escapes/, 'string.escape'],
                [/\\./,      'string.escape.invalid'],
                [/"/,        { token: 'string.quote', bracket: '@close', next: '@pop' } ]
            ],

            interpolatedstring: [
                [/[^\\"{]+/, 'string'],
                //[/@escapes/, 'string.escape'],
                [/\\./, 'string.escape.invalid'],
                //[/{{/, 'string.escape'],
                //[/}}/, 'string.escape'],
                [/{/, { token: 'string.quote', next: 'root.interpolatedstring' }],
                [/"/, { token: 'string.quote', next: '@pop' }]
            ],

            whitespace: [
                [/[ \t\r\n]+/, 'white'],
                [/\/\*/,       'comment', '@comment' ],
                [/\/\/.*$/,    'comment'],
                [/\/#.*$/,    'comment'],
            ],

            afterDot:[
                [/[A-Z]\w*/, "identifier", "@pop"],
                { include:'@root' },
            ],
        },

    };
}

export async function transpileEditorCode(isAuto = false){
    return await transpile(isAuto, editorModel)
}

//require.config({ paths: { vs: 'node_modules/monaco-editor/min/vs' } });
monaco.languages.register({ id: "hlf" });
export let editorModel = monaco.editor.createModel(`

Entity player = @p;
val radius = 4;
void tick(){
    val belowPlayer = player.Position;
    for(int x = -1*radius; x < radius;x++){
        for(int z = -1*radius; z < radius; z++){
            say($"Calculating for block ({x}; {z})");
            val v = belowPlayer+Vector(x, -1, z);
            if(x*x + z*z < 5){
                if(getBlock(v) == BlockType("water")){
                    setBlock(v, BlockType("ice"));
                }
            }
        }
    }
}
`,
    "hlf"
);
window.editorModel = editorModel;
window.monaco = monaco;
monaco.editor.defineTheme("hlf-vs-dark", {
    base: "vs-dark",
    inherit: true,
    colors: {},
    rules: [
        { token: "function", foreground: "39CC8F" },
    ],
});

let codeEditor = monaco.editor.create(document.getElementById('codeContainer'), {
    theme: 'hlf-vs-dark',
    tabSize: 4,
    directIndentation: true,
    useTabStops: true,
    model: editorModel,
    autoClosingBrackets: true,
    automaticLayout: true,
    mouseWheelZoom: true
});


codeEditor.comple
codeEditor.addAction(
    {
        id:"transpile",
        label:"Transpile to mcfunction",
        run: async function(){
            await transpile(false, editorModel)
        }
    }
);

// I add vs keybindings first and then the default vscode ones so that the vscode ones are displayed but mine are still usable
monaco.editor.addKeybindingRules([
    {
        keybinding: monaco.KeyMod.CtrlCmd | monaco.KeyMod.Alt | monaco.KeyCode.NumpadDivide,
        command: "editor.action.commentLine",
        when: "textInputFocus",
    },
    {
        keybinding: monaco.KeyMod.CtrlCmd  | monaco.KeyCode.Slash,
        command: "editor.action.commentLine",
        when: "textInputFocus",
    },
    {
        keybinding: monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.NumpadDivide,
        command: "editor.action.blockComment",
        when: "textInputFocus",
    },
    {
        keybinding: monaco.KeyMod.Shift | monaco.KeyMod.Alt | monaco.KeyCode.KeyA,
        command: "editor.action.blockComment",
        when: "textInputFocus",
    }
]);

monaco.languages.setMonarchTokensProvider("hlf", getMonarchDef());
monaco.languages.setLanguageConfiguration('hlf', {
    brackets: [
        ['{', '}'],
        ['[', ']'],
        ['(', ')']
    ],
    autoClosingPairs: [
        { open: '{', close: '}', notIn: ['string', 'comment'] },
        { open: '[', close: ']', notIn: ['string', 'comment'] },
        { open: '(', close: ')', notIn: ['string', 'comment'] },
        { open: '"', close: '"', notIn: ['string'] },
    ],
    surroundingPairs: [
        { open: '{', close: '}' },
        { open: '[', close: ']' },
        { open: '(', close: ')' },
        { open: '"', close: '"' },
    ],
    comments: {
        blockComment: ["/*", "*/"],
        lineComment: "//",
    }
});

require("./editorCompletions");

monaco.languages.registerHoverProvider("hlf", {
    provideHover: function (model, position) {
        let word = model.getWordAtPosition(position)
        if(word === null) return
        let afterWord = model.getValueInRange(new monaco.Range(position.lineNumber, word.endColumn, position.lineNumber, word.endColumn+1));
        if(afterWord !== '(') return

        let data = getFunctionData(word.word);
        if(!data.success) return

        return {
            range: new monaco.Range(
                position.lineNumber,
                word.startColumn,
                position.lineNumber,
                word.endColumn,
            ),
            contents: [
                {
                    value: data.description
                },
                {
                    value: "**Overloads:**"
                },
                {
                    value: data.overloads.map(function (str) {
                        return "- " + str + "\n"
                    }).join('\n')
                },
            ],
        };
    },
});

//console.log("Initial transpile from editor")
//await transpileEditorCode(true, editorModel);