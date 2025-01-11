import bootsharp, {HlfTranspilerJs} from "./../bin/bootsharp/index.mjs";
bootsharp.boot();

onmessage = (e) => {
    const src = e.data[0];
    const options = e.data[1];
    const res = HlfTranspilerJs.transpileToString(src, options);
    postMessage(res);
};
