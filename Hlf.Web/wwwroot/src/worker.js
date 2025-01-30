import bootsharp, {HlfTranspilerJs} from "hlf-transpiler";
//import bootsharp, {HlfTranspilerJs} from "./index.mjs";
await bootsharp.boot();
postMessage(0)

onmessage = (e) => {
    const src = e.data[0];
    const options = e.data[1];
    const res = HlfTranspilerJs.transpileToString(src, options);
    postMessage(res);
};
