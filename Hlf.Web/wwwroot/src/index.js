// const hlf = require("./hlf");

import hlf, {onTranspilationFinished, setOnTranspilationFinishedCallback, transpile} from "hlf";
import { createApp, reactive } from "petite-vue"

let editorModel;
document.addEventListener('DOMContentLoaded', main, false);
let store;
async function main(){
    
    let hlf = await require("./hlf")
    let monacoSetup = await require("./monacoSetup");
    require("./zipping");
    let io = await require("./io");
    editorModel = monacoSetup.editorModel;
    io.setEditorModel(editorModel);
    
    editorModel.onDidChangeContent(function(){
        if(document.getElementById("transpileInRealtimeCheckbox").checked){
            monacoSetup.transpileEditorCode(true);
        }

        // if saving via file-system-access-api is enabled, don't save source code
        if(document.dirHandle === undefined){
            saveSrcCodeInLocalStorage()
        }
    })

    hlf.setOnTranspilationFinishedCallback(function (isAuto){
        switchFile();

        if(document.dirHandle !== undefined && (!isAuto || document.getElementById("saveInRealtimeCheckbox").checked)){
            io.saveDatapackLocally();
        }
    })
    
    console.log("Loading definitions...")
    hlf.loadDefinitions();
    
    //document.getElementById("transpileButton").onclick = monacoSetup.transpileEditorCode(false);
    console.log("Initializing petite vue")
    store = reactive({
        editorModel: editorModel,
        switchFile: switchFile,
        transpile: monacoSetup.transpileEditorCode,
        displayedOutput: "",
        openLocalDir: io.openLocalDir,
        copyOutputToClipboard: io.copyOutputToClipboard,
        onOptionsChanged(){
            monacoSetup.transpileEditorCode(true);
    
            // if saving via file-system-access-api is enabled, don't save source code
            if(document.dirHandle === undefined){
                saveSrcCodeInLocalStorage()
            }
        },
    })
    
    createApp({
        store: store
    }).mount()
    
    await loadExampleUrls();
}


export function switchFile(){
    let file = document.getElementById("file-view-select").value;
    console.log("Switching to file " + file);
    //let outputWindow = document.getElementById("outputTextArea");
    //outputWindow.value = document.transpiledJson[file];
    store.displayedOutput = document.transpiledJson[file];
}

function saveSrcCodeInLocalStorage(){
    const src = editorModel.getValue();
    window.localStorage.setItem("srcCode", src);
}
function loadSrcCodeFromLocalStorage(){
    const src = window.localStorage.getItem("srcCode");
    if(src !== null) editorModel.setValue(src)
}

async function loadExampleUrls(){
    const response = await fetch("https://api.github.com/repos/zenonet/HighLevelFunction/contents/Examples");
    let examples;
    if(response.ok) {
        const content = await response.json();

        examples = [];
        for (let responseElement of content) {
            examples.push({
                name: responseElement.name,
                downloadUrl: responseElement.download_url,
            })
        }
        window.localStorage.setItem("exampleCache", JSON.stringify(examples));
    }else{
        console.log("GitHub-API doesn't respond, trying to load examples from cache");
        examples = JSON.parse(window.localStorage.getItem("exampleCache"));
    }

    const list = document.getElementById("exampleList");
    const template = document.getElementById("exampleLinkTemplate");
    for (const example of examples) {
        const content = template.cloneNode(true)
        content.style.display = "unset";
        list.appendChild(content)
        content.querySelector("#link").onclick = function (event){
            loadExample(example.downloadUrl);
            event.preventDefault();
        };

        content.querySelector("#link").innerText = example.name;
        console.log("Generated link")
    }
}


async function loadExample(url){
    const response = await fetch(url);
    const text = await response.text();
    editorModel.setValue(text);
}

await main()

/*
if(document.readyState === "interactive" || document.readyState === "complete"){
}
*/