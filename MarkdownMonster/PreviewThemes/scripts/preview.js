var te = {
    mmEditor: null,
    isPreviewEditorSync: false,
    codeScrolled: new Date().getTime() + 2500
}
var isDebug = false;

// This function is global and called by the parent
// to pass in the form object and pass back the text
// editor instance that allows the parent to make
// calls into this component
function initializeinterop(editor) {    
    te.mmEditor = editor;    
    te.isPreviewEditorSync = te.mmEditor.IsPreviewToEditorSync();
    scroll();    
}

$(document).ready(function() {
    highlightCode();
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

var scroll = debounce(function (event) {    
    if (!te.mmEditor || !te.isPreviewEditorSync) return;

    // prevent repositioning editor scroll sync 
    // when selecting line in editor (w/ two way sync)
    var t = new Date().getTime();
    
    if (te.codeScrolled > t - 250)
        return;

    te.codeScrolled = t;
        
    var st = $(window).scrollTop();

    var winTop = st + 100;
    var $lines = $("[id*='pragma-line-']");

    if ($lines.length < 1)
        return;

    var id = null;
    for (var i = 0; i < $lines.length; i++) {
        
        if ($($lines[i]).position().top >= winTop) {
            id = $lines[i].id;
            break;
        }        
    }
    if (!id)
        return;

    id = id.replace("pragma-line-", "");
    
    te.mmEditor.GotoLine((id * 1) - 1);
},100);
window.onscroll = scroll;




function highlightCode() {
    $("pre code")
        .each(function (i, block) {
            hljs.highlightBlock(block);
        });
}

function updateDocumentContent(html) {    
    te.isPreviewEditorSync = te.mmEditor.IsPreviewToEditorSync();    
    $("#MainContent").html(html);
    highlightCode();
}

function scrollToPragmaLine(lineno) {
    if (lineno < 0) return;

    setTimeout(function () {
        try {
            var $el = $("#pragma-line-" + lineno);
            if ($el.length < 1) {
                for (var i = 0; i < 5; i++) {
                    lineno--;
                    $el = $("#pragma-line-" + lineno);
                    if ($el.length > 0)
                        return;
                }
            }

            te.codeScrolled = new Date().getTime();
            $("html").scrollTop($el.offset().top - 100);

            $el.addClass("line-highlight");
            setTimeout(function() { $el.removeClass("line-highlight"); }, 1200);
        }
        catch(ex) {  }
    },80);
}

function status(msg,append) {
    var $el = $("#statusmessage");
    if ($el.length < 1) {
        $el = $("<div id='statusmessage' style='position: fixed; opacity: 0.8; left:0; right:0; bottom: 0; padding: 5px 10px; background: #444; color: white;'></div>");
        $(document.body).append($el);
    }

    if (append) {
        var html = $el.html() +
            "<br/>" +
            msg;
        $el.html(html);
    }
    else
        $el.text(msg);

    $el.show();
    setTimeout(function() { $el.text(""); $el.fadeOut() }, 6000);
}


window.onerror = function windowError(message, filename, lineno, colno, error) {
    if (!isDebug)
        return true;
    
    var msg = "";
    if (message)
        msg = message;
    //if (filename)
    //    msg += ", " + filename;
    if (lineno)
        msg += " (" + lineno + "," + colno + ")";
    if (error)
        msg += error;

    // show error messages in a little pop overwindow
    if (isDebug)
        status(msg);

    console.log(msg);
    
    // don't let errors trigger browser window
    return true;
}

function debounce(func, wait, immediate) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        var later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow)
            func.apply(context, args);
    };
};