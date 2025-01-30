//import { downloadZip } from "./node_modules/client-zip/index.js"
import { downloadZip } from "client-zip"

//import { transpile } from 'hlf';
import { transpileEditorCode as transpile } from "./monacoSetup";
 
document.downloadDatapack = async function () {
    console.log("Downloading...")
    if(document.transpiledJson === undefined) return

    // define what we want in the ZIP
    const now = new Date();
    const files = []

    for (const name in document.transpiledJson) {
        const content = document.transpiledJson[name];
        files.push({
            name: name,
            input: content,
            lastModified: now
        });
    }

    // get the ZIP stream in a Blob
    const blob = await downloadZip(files).blob()

    // make and click a temporary link to download the Blob
    const link = document.createElement("a")
    link.href = URL.createObjectURL(blob)
    link.download = "datapack.zip"
    link.click()
    link.remove()

    // in real life, don't forget to revoke your Blob URLs if you use them
}
console.log("Added download function")
document.getElementById("downloadZipButton").onclick = async function (){
    await transpile(false);
    await document.downloadDatapack();
}
document.getElementById("downloadZipButton").disabled = false;