// https://stackoverflow.com/a/30810322
function fallbackCopyTextToClipboard(text) {
    var textArea = document.createElement("textarea");
    textArea.value = text;

    // Avoid scrolling to bottom
    textArea.style.top = "0";
    textArea.style.left = "0";
    textArea.style.position = "fixed";

    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
        var successful = document.execCommand('copy');
        var msg = successful ? 'successful' : 'unsuccessful';
        console.log('Fallback: Copying text command was ' + msg);
    } catch (err) {
        console.error('Fallback: Oops, unable to copy', err);
    }

    document.body.removeChild(textArea);
}
export function copyOutputToClipboard() {
    const text = document.getElementById("outputTextArea").value;
    console.log("copying: " + text);
    if (!navigator.clipboard) {
        fallbackCopyTextToClipboard(text);
        return;
    }
    navigator.clipboard.writeText(text).then(function() {
        console.log('Async: Copying to clipboard was successful!');
    }, function(err) {
        console.error('Async: Could not copy text: ', err);
    });
}

let editorModel;
export function setEditorModel(_editorModel) {
    editorModel = _editorModel;
}

export async function openLocalDir(){
    if(!('showOpenFilePicker' in window)){
        alert("Your browser currently does not support FileSystemAccessAPI which is needed for modifying local files. Try using a chromium-based browser");
        return;
    }

    document.dirHandle = await window.showDirectoryPicker({mode:"readwrite"});
    lastJson = undefined;

    const dir = document.dirHandle;

    if(document.dirHandle === undefined) return;

    console.log("got file handle: " + document.dirHandle);

    const srcFileHandle = await dir.getFileHandle("source.hlf", {create: false});
    if(srcFileHandle !== undefined && window.confirm("The opened datapack contains hlf code. Do you want to load it? (Your current code will be gone)")){
        console.log("Loading hlf code from opened datapack...");
        // existing hlf datapack has been opened, load source code:
        const src = await (await srcFileHandle.getFile()).text()

        editorModel.setValue(src);
    }

    document.getElementById("saveInRealtimeCheckbox").disabled = false;
    document.getElementById("workInLocalDirButton").innerText = "Change working directory";
}


let lastJson = undefined;
export async function saveDatapackLocally(){
    let json = document.transpiledJson;

    document.getElementById("savingLocallySpan").style.display = "unset";

    for(const o in json){

        if(lastJson !== undefined && lastJson.hasOwnProperty(o)){
            if(json[o] === lastJson[o]){
                console.log("File didn't change, not writing!");
                continue;
            }
        }

        let subdirs = o.split("/");
        let fileName = subdirs.pop()
        console.log("Creating subdirs:" + subdirs)
        // Iterate over the subdirectory names and create them
        let currentDir = document.dirHandle;
        for (const subdir of subdirs) {
            currentDir = await currentDir.getDirectoryHandle(subdir, { create: true });
        }
        const fileHandle = await currentDir.getFileHandle(fileName, { create: true });
        const writable = await fileHandle.createWritable();
        await writable.write(json[o]);
        await writable.close();
    }
    console.log("build files written")

    // write the source code to an additional file:
    const fileHandle = await document.dirHandle.getFileHandle("source.hlf", { create: true });
    const writable = await fileHandle.createWritable();
    await writable.write(editorModel.getValue());
    await writable.close();
    document.getElementById("savingLocallySpan").style.display = "none";

    lastJson = json;
}