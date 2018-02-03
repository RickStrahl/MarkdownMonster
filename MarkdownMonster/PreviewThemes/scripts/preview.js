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
    if (window.dotnetProxy) {
        te.mmEditor = window.dotnetProxy;        
    } else
        te.mmEditor = editor;

    te.isPreviewEditorSync = te.mmEditor.isPreviewToEditorSync();
    scroll();    
}

$(document).ready(function() {
    highlightCode();

    // navigate all links externally in default browser
    $(document).on("click","a",
        function(e) {
            var url = this.href;
            var hash = this.hash;
            
            if (url.substr(0, 4) === "http" &&
                te.mmEditor.navigateExternalUrl(url)) {
                    e.preventDefault();
                    return false;                
            }
            if (hash) {                
                var sel = hash + "[name='" + hash.substr(1) + "'],#"+ hash.substr(1);                                          
                var $el = $(sel);                
                $("html").scrollTop($el.offset().top - 100);
                return false;
            }
        });
});

$(document).on("contextmenu",
    function() {
        te.mmEditor.previewContextMenu({ Top: 1, Left: 1 });

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
    
    te.mmEditor.gotoLine((id * 1) - 1);
},100);
window.onscroll = scroll;

function highlightCode() {
    $("pre code")
        .each(function (i, block) {
            hljs.highlightBlock(block);
        });
}

function updateDocumentContent(html) {    
    te.isPreviewEditorSync = te.mmEditor.isPreviewToEditorSync();   
    setTimeout(function () { 
        $("#MainContent").html(html);
        highlightCode();
    });
}

function scrollToPragmaLine(lineno) {
    if (typeof lineno !== "number" || lineno < 0) return;

    setTimeout(function () {
        if (lineno < 3) {
            $("html").scrollTop(0);
            return;
        }

        try {
            var $el = $("#pragma-line-" + lineno);
            if ($el.length < 1) {
                var origLine = lineno;
                for (var i = 0; i < 3; i++) {
                    lineno++;
                    $el = $("#pragma-line-" + lineno);
                    if ($el.length > 0)
                        break;
                }
                if ($el.length < 1) {
                    lineno = origLine;
                    for (var i = 0; i < 3; i++) {
                        lineno--;
                        $el = $("#pragma-line-" + lineno);
                        if ($el.length > 0)
                            break;
                    }
                }
                if ($el.length < 1)
                    return;
            }
                
            $el.addClass("line-highlight");
            setTimeout(function () { $el.removeClass("line-highlight"); }, 1800);

            te.codeScrolled = new Date().getTime();
            if (lineno > 3)
                $("html").scrollTop($el.offset().top - 150); 
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