
//console.log("webWorker loadding");

onmessage = function (event) {    
    importScripts(event.data.script);

    var result;
    if (!event.data.lang)
        result = hljs.highlightAuto(event.data.code);
    else
        result = hljs.highlight(event.data.lang, event.data.code);
    
    postMessage(result);
    //console.log('result from worker: ' + result);
    close(); // close worker
};