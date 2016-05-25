$("pre code").each(function (i, block) {
    hljs.highlightBlock(block);
});
$(document).on("contextmenu", function () {
    // inside of WebBrowser control don't show context menu
    return navigator.userAgent.indexOf("Trident") > -1 ? false : true;
});

window.ondrop = function (event) {
    // don't allow dropping here - we can't get full file info
    event.preventDefault();
    event.stopPropagation();

    setTimeout(function () {
        alert("To open dropped files in Markdown Monster, please drop files onto the header area of the window.");
    }, 50);
}
window.ondragover = function (event) {
    event.preventDefault();
    return false;
}