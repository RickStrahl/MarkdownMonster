highlightCode();


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

function highlightCode() {
    $("pre code")
        .each(function (i, block) {
            hljs.highlightBlock(block);
        });
}

function updateDocumentContent(html) {
    $("#MainContent").html(html);
    highlightCode();
}

function scrollToPragmaLine(lineno) {
    if (lineno < 0) return;

    setTimeout(function () {
        var $el = $("#pragma-line-" + lineno);
        if ($el.length < 1) {
            for (var i = 0; i < 5; i++) {
                lineno--;
                $el = $("#pragma-line-" + lineno);
                if ($el.length > 0)
                    return;
            }
        }

        $("html").scrollTop($el.offset().top - 100);
        
        $el.addClass("line-highlight");
        setTimeout(function() { $el.removeClass("line-highlight"); },1200);
    },200);
}

function status(msg) {
    var $el = $("#statusmessage");
    if ($el.length < 1)
        $el = $("<div id='statusmessage' style='position: fixed;  left:0; right:0; bottom: 0; padding: 10px; background: #444; color: white;'></div>");

    $(document.body).append($el);
    $el.text(msg);
}