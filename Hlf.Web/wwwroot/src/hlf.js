
// Importing compiled ES module.
import bootsharp, {HlfTranspilerJs} from "hlf-transpiler";
import monaco from 'monaco-editor/esm/vs/editor/editor.api';
//import bootsharp, {HlfTranspilerJs} from "./index.mjs";

// Initializing dotnet runtime and invoking entry point.


//const wasmWorker = new Worker("./worker.js", {type:"module"});
const wasmWorker = new Worker(new URL('./worker.js', import.meta.url));

wasmWorker.onmessage = (e) => {
    if(e.data === 0){
        // worker is ready!
        console.log("Worker is ready.")
        // transpile()
        if(document.getElementById("useWorkerCheckbox").checked) document.getElementById("transpileButton").disabled = false;
        return;
    }
    onResult(e.data)
};
let isAuto = false;

export const getFunctionData = HlfTranspilerJs.getFunctionDescription;
export const getAllBuiltinFunctionDefinitions = HlfTranspilerJs.getAllBuiltinFunctionDefinitions;
export const throwSymbols = HlfTranspilerJs.throwSymbols;
function getTargetVersion(){
    const strVal = document.getElementById("targetMcVersion").value;
    const parts = strVal.split('.');
    if(parts.length === 3){
        return {
            Major: +parts[0],
            Minor: +parts[1],
            Patch: +parts[2],
        }
    }
    return {
        Major: +parts[0],
        Minor: +parts[1],
        Patch: 0,
    }
}
let transpileResolve;
let transpileReject;
let editorModel;
export function transpile(_isAuto = false, _editorModel){
    return new Promise(function(resolve, reject){
        transpileResolve = resolve;
        transpileReject = reject;
        editorModel = _editorModel;
        isAuto = _isAuto;
        let options = {
            GenerateComments: document.getElementById("generateComments").checked,
            TargetVersion: getTargetVersion()
        }

        window.TranspileStart = performance.now()
        if(document.getElementById("useWorkerCheckbox").checked)
            wasmWorker.postMessage(
                [editorModel.getValue(), options]);
        else{
            const resp = HlfTranspilerJs.transpileToString(editorModel.getValue(), options);
            onResult(resp);
        }
    });
}

export function setOnTranspilationFinishedCallback(callback){
    console.log("setOnTranspilationFinishedCallback set");
    onTranspilationFinished = callback;
}
let onTranspilationFinished;

function onResult(resp){
    const ms = (performance.now()-window.TranspileStart);
    document.getElementById("stats").innerText = `${ms.toFixed(1)}ms (${(1/ms*1000).toFixed(0)}hz)`;

    const msgField = document.getElementById("errorMsg");
    const successField = document.getElementById("successMsg");
    if(resp.startsWith("errhndl(")){

        const metaStart = resp.indexOf('(');
        const metaEnd = resp.indexOf(')');
        const nums = resp.substring(metaStart+1, metaEnd).split(';');

        const line = parseInt(nums[0]);
        const column = parseInt(nums[1]);
        const length = parseInt(nums[2]);
        console.log("line:" + line + " column:" + column + " length:" + length)


        const err = resp.substring(metaEnd+2);
        console.log("Error detected: " + err)
        msgField.style.visibility = "visible";
        msgField.innerText = err;
        successField.style.visibility = "hidden";


        // show errors:
        monaco.editor.setModelMarkers(editorModel, "owner", [
                {
                    startLineNumber: line,
                    startColumn: column,
                    endLineNumber: line,
                    endColumn: column+length,
                    message: err,
                    severity: monaco.MarkerSeverity.Error
                }
            ]);
        transpileResolve(err)
        return;
    }
    // clear errors in editor:
    monaco.editor.setModelMarkers(editorModel, "owner", []);


    msgField.style.visibility = "hidden";
    successField.style.visibility = "visible";

    let json = JSON.parse(resp);
    //document.getElementById("outputTextArea").value = json["data/function/load.mcfunction"];
    document.transpiledJson = json;

    let oldSelectedFile = document.getElementById("file-view-select").value;
    const dropdown = document.getElementById("file-view-select");
    dropdown.innerHTML = "";
    for (let f in json){
        let option = new Option(f, f);
        option.innerText = f;
        dropdown.appendChild(option);
    }

    if(json[oldSelectedFile] !== undefined){
        dropdown.value = oldSelectedFile;
    }else{
        // By default, select the load.mcfunction file
        for(let o in json){
            if(o.endsWith("load.mcfunction")){
                dropdown.value = o;
                break;
            }
        }
    }

    onTranspilationFinished();
    transpileResolve(json);
}


await bootsharp.boot();
console.log(document.readyState)
if(!document.getElementById("useWorkerCheckbox").checked) document.getElementById("transpileButton").disabled = false;

export function loadDefinitions() {
    const definitions = HlfTranspilerJs.getAllBuiltinFunctionDefinitions()
    console.log("Found " + definitions.length + " definitions")
    const template = document.getElementById("template")
    for (const i in definitions) {
        const def = definitions[i]


        console.log(def)
        const content = template.cloneNode(true)
        document.getElementById("functionDefinitions").appendChild(content)
        content.querySelector("#description").innerHTML = def.description.replace(/\[(.*)]\((.*)\)/, '<a href="$2">$1</a>'); // we have to translate Markdown links to html here using regex rq.
        content.querySelector("#functionName").innerHTML = def.name;
        content.querySelector("#overloads").innerHTML = def.overloads.join("<br>");

        content.style.display = "unset";
    }

    document.getElementById("search").oninput = function (ev) {
        const text = document.getElementById("search").value.toLowerCase();

        for (let explanation of document.querySelectorAll("#functionDefinitions > .function-explanation")) {
            let show = false;
            const titleContains = explanation.querySelector("#functionName").innerText.toLowerCase().includes(text);
            const description = explanation.querySelector("#description").innerText.toLowerCase();

            if(text.length >= 3) show = titleContains || description.includes(text);
            else if(text.length >= 2) show = titleContains || description.includes(" " + text) || description.startsWith(text);
            else show = titleContains;

            explanation.style.display = show ? "block" : "none";
        }
    };
}

/*
module.exports = {
    transpile: transpile,
    getFunctionData: HlfTranspilerJs.getAllBuiltinFunctionDefinitions,
    test: function (){
        console.log("Test")
    }
}*/
