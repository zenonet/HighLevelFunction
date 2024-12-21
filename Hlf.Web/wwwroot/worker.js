import bootsharp, {HlfTranspilerJs} from "./../bin/bootsharp/index.mjs";
bootsharp.boot();

onmessage = (e) => {
    const res = HlfTranspilerJs.transpileToString(e.data);
    postMessage(res);
};
