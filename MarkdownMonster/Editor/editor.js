/// <reference path="editorsettings.js"/>
/// <reference path="editorSpellcheck.js"/>

// NOTE: All method and property names have to be LOWER CASE!
//       in order for FoxPro to be able to access them here.
var te = window.textEditor = {
    mm: null, // FoxPro COM object
    editor: null, // Ace Editor instance
    settings: editorSettings,
    lastError: null,
    dic: null,
    aff: null,
    isDirty: false,
    mousePos: { column: 0, row: 0 },
    initialize: function () {
        
        // attach ace to formatted code controls if they are loaded and visible
        var $el = $("pre[lang]");
        try {
            var codeLang = $el.attr('lang');
            var aceEditorRequest = ace.edit($el[0]);
            te.editor = aceEditorRequest;
            te.configureAceEditor(aceEditorRequest, editorSettings);
            aceEditorRequest.getSession().setMode("ace/mode/" + codeLang);
        } catch (ex) {
            if (typeof console !== "undefined")
                console.log("Failed to bind syntax: " + codeLang + " - " + ex.message);
        }
        te.editor.focus();

        // explicitly call this from WPF
        //if (editorSettings.enableSpellChecking)
        //    setTimeout(spellcheck.enable, 1000);
    },
    configureAceEditor: function(editor, editorSettings) {
        if (!editor)
            editor = te.editor;
        if (!editorSettings)
            editorSettings = te.settings;

        var session = editor.getSession();

        editor.setReadOnly(false);
        editor.setHighlightActiveLine(false);
        editor.setShowPrintMargin(editorSettings.showPrintMargin);

        //te.settheme(editorSettings.theme, editorSettings.fontSize, editorSettings.wrapText);
        editor.setTheme("ace/theme/" + editorSettings.theme);
        editor.setFontSize(editorSettings.fontSize);
        // allow editor to soft wrap text
        session.setUseWrapMode(editorSettings.wrapText);
        session.setOption("indentedSoftWrap", false);

        

        editor.renderer.setShowGutter(editorSettings.showLineNumbers);
        session.setTabSize(editorSettings.tabSpaces);

        session.setNewLineMode("windows");

        // disable certain hot keys in editor so we can handle them here        
        editor.commands.bindKeys({
            //"alt-k": null,
            "ctrl-n": function() {
                // do nothing but:
                // keep ctrl-n browser behavior from happening
                // and let WPF handle the key
            },
            "f5": function() {},
            "alt-c": function() { te.specialkey("alt-c"); },
            "ctrl-o": function() { te.specialkey("ctrl-o"); },
            "ctrl-s": function() { te.specialkey("ctrl-s"); },
            "ctrl-b": function() { te.specialkey("ctrl-b"); },
            "ctrl-i": function() { te.specialkey("ctrl-i"); },
            "ctrl-l": function() { te.specialkey("ctrl-l"); },
            "ctrl-k": function() { te.specialkey("ctrl-k"); },

            // take over Zoom keys and manually zoom
            "ctrl--": function() {
                te.specialkey("ctrl--");
                return null;
            },
            "ctrl-=": function() {
                te.specialkey("ctrl-=");
                return null;
            },

            "ctrl-shift-down": function() { te.specialkey("ctrl-shift-down"); },
            "ctrl-shift-up": function() { te.specialkey("ctrl-shift-up"); },
            "ctrl-shift-c": function() { te.specialkey("ctrl-shift-c"); },
            "ctrl-shift-v": function() { te.specialkey("ctrl-shift-v"); },
            "ctrl-v": function() { te.mm.textbox.PasteOperation();  }
        
    });
        
        editor.renderer.setPadding(15);
        editor.renderer.setScrollMargin(5, 5, 0, 0); // top,bottom,left,right

        //te.editor.getSession().setMode("ace/mode/markdown" + lang);   

        
        te.editor.setOptions({
            // fill entire view
            maxLines: 0,
            minLines: 0
            //wrapBehavioursEnabled: editorSettings.wrapText                       
        });
        
        var keydownHandler = function keyDownHandler(e) {
            if (!te.isDirty) {
                if (!te.mm)
                    return;

                // any printable character
                var keycode = e.keyCode;

                var valid =
                    (e.keycode > 47 && keycode < 58) || // number keys
                        keycode == 32 ||
                        keycode == 13 || // spacebar & return key(s) 
                        (keycode > 64 && keycode < 91) || // letter keys
                        (keycode > 95 && keycode < 112) || // numpad keys  
                        (keycode > 185 && keycode < 193) || // ;=,-./` (in order) 
                        (keycode > 218 && keycode < 223); // [\]' (in order)
                // backspace, tab -> handled in key up

                if (valid) {
                    te.isDirty = true;
                    te.mm.textbox.setDirty(true);                    
                }                
            }

            if (e.ctrlKey && e.shiftKey) {                
                te.mm.textbox.PreviewMarkdownCallback();
                te.updateDocumentStats();                
            }
        };          
        $("pre[lang]").on("keydown", keydownHandler);


        var keyupHandler = debounce(function keyUpHandler(e) {
            if (!te.mm)
                return;

            var keycode = e.keyCode;
            //if (keycode == 13 ||   // cr
            //    keycode == 8 ||    // backspace
            //    keycode == 46 ||   // del                                
            //    (keycode > 185 && keycode < 193) || // ;=,-./` (in order)                        
            //    keycode == 222)   // single quote
            //          te.mm.textbox.PreviewMarkdownCallback();

            // handle tab/backspace in keyup - not working in keydown
            if (!te.isDirty) {
                if (keycode == 8 || keycode == 9 || keycode == 46) {
                    te.isDirty = true;
                    te.mm.textbox.setDirty(true);
                }
            }
            te.mm.textbox.PreviewMarkdownCallback();
            te.updateDocumentStats();            

            //if (te.isspellcheckingenabled)
            //    sc.spellCheck();

        }, 1500);
        $("pre[lang]").on("keyup", keyupHandler);

        
        // always have mouse position available when drop or paste
        te.editor.on("mousemove",function (e) {
            te.mousePos = e.getDocumentPosition();            
        });
        te.editor.on("mouseup",function() {
            te.mm.textbox.PreviewMarkdownCallback();
        });

        return editor;
    },
    initializeeditor: function() {
        te.configureAceEditor(null, null);
    },
    status: function(msg) {
        //alert(msg);
        status(msg);
    },
    getscrolltop: function () {
        return te.editor.getSession().getScrollTop();
    },
    setscrolltop: function(scrollTop) {
        return te.editor.getSession().setScrollTop(scrollTop);
    },
    getvalue: function(ignored) {
        var text = te.editor.getSession().getValue();
        return text.toString();
    },
    setvalue: function(text, pos) {
        if (!pos)
            pos = -1; // first line
        //if (pos == -2)  
        te.editor.setValue(text, pos);

        te.editor.getSession().setUndoManager(new ace.UndoManager());

        setTimeout(function () {
            te.editor.resize(true);  //force a redraw
        }, 30);
    },
    refresh: function(ignored) {
        te.editor.resize(true); //force a redraw
    },
    specialkey: function(key)   {
        te.mm.textbox.SpecialKey(key);
    },
    setfont: function (size, fontFace, weight) {
        if (size)
            te.editor.setFontSize(size);
        if (fontFace)
            te.editor.setOption('fontFamily', fontFace);
        if (weight)
            te.editor.setOption('fontWeight', weight);
    },
    getfontsize: function () {        
        var zoom = screen.deviceXDPI / screen.logicalXDPI;
        var fontsize = te.editor.getFontSize() * zoom;
        return fontsize;
    },
    setselection: function (text) {    
        var range = te.editor.getSelectionRange();
        te.editor.getSession().replace(range, text);
    },
    setselposition: function(index,count) {    	
    	var doc = te.editor.getSession().getDocument();
        var lines = doc.getAllLines();

    	 var offsetToPos = function( offset ) {
            var row = 0, col = 0;
            var pos = 0;
            while ( row < lines.length && pos + lines[row].length < offset) {
                pos += lines[row].length;
                pos++; // for the newline
                row++;
            }
            col = offset - pos;
            return {row: row, column: col};
        };             
		var start = offsetToPos( index );
        var end = offsetToPos( index + count );

    	var sel = te.editor.getSelection();
    	var range = sel.getRange();
    	range.setStart(start);
    	range.setEnd(end);
    	sel.setSelectionRange( range );
    },
    getselection: function (ignored) {
        return te.editor.getSelectedText();
    },
    getLineNumber: function(ignored) {
        var selectionRange = te.editor.getSelectionRange();
        return selectionRange.start.row;
    },
    gotfocus: function (ignored) {
        te.setfocus();
    },
    setfocus: function (ignored) {
        te.editor.resize(true);

        setTimeout(function () {
            te.editor.focus();
            setTimeout(function () {
                te.editor.focus();
            }, 400);
        }, 50);
    },
    // forces Ace to lose focus
    losefocus: function(ignored) {
        $("#losefocus").focus();
    },
    setlanguage: function (lang) {
        if (!lang)
            lang = "text";
        if (lang == "vfp")
            lang = "foxpro";
        if (lang == "c#")
            lang = "csharp";
        if (lang == "c++")
            lang == "c_cpp"

        te.editor.getSession().setMode("ace/mode/" + lang);
    },
    settheme: function (theme, fontSize, wrapText) {
        te.editor.setTheme("ace/theme/" + theme);
        
        if (fontSize) 
            te.editor.setFontSize(fontSize);

        wrapText = wrapText || false;        

        var session = te.editor.getSession();
        session.setUseWrapMode(wrapText);
        session.setOption("indentedSoftWrap", true);

        setTimeout(te.updateDocumentStats, 100);
    },
    execcommand: function(cmd,parm1,parm2) {
        te.editor.execCommand(cmd);
    },
    curStats: { wordCount: 0, lines: 0 },
    getDocumentStats: function () {
        var text = te.getvalue();

        // strip off blog post meta data at end of document
        var pos = text.indexOf("\n<!-- Post Configuration -->");
        if (pos > 0)
            text = text.substr(0, pos - 1);

        var regExWords = /\s+/gi;
        var wordCount = text.replace(regExWords, ' ').split(' ').length;                
        var lines = text.split('\n').length;
        
        te.curStats = {
            wordCount: wordCount,
            lines:lines
        }
        return te.curStats;
    },
    updateDocumentStats: function() {
        te.mm.textbox.updateDocumentStats(te.getDocumentStats());
    },
    enablespellchecking: function (disable, dictionary) {
        if (dictionary)
            editorSettings.dictionary = dictionary;
        setTimeout(function() {
                if (!disable)
                    spellcheck.enable();
                else
                    spellcheck.disable();
            },
            100);
    },
    isspellcheckingenabled: function(ignored) {
        return editorSettings.enableSpellChecking;
    },
    checkSpelling: function (word) {        
        if (!word || !editorSettings.enableSpellChecking)
            return true;

        // use typo
        if (spellcheck.dictionary) {            
            var isOk = spellcheck.dictionary.check(word);            
            return isOk;
        }

        // use COM object        
        return te.mm.textbox.CheckSpelling(word,editorSettings.dictionary,false);
    },
    suggestSpelling: function (word, maxCount) {
        if (!editorSettings.enableSpellChecking)
            return null;

        // use typo
        if (spellcheck.dictionary)
            return spellcheck.dictionary.suggest(word);
        
        // use COM object
        var words = te.mm.textbox.GetSuggestions(word, editorSettings.dictionary, false);       
        if (!words)
            return [];

        words = JSON.parse(words);
        if (words.length > maxCount)
            words = words.slice(0, maxCount);

        return words;
    },
    addWordSpelling: function (word) {        
        te.mm.textbox.AddWordToDictionary(word, editorSettings.dictionary);
    },
    onblur: function () {
        te.mm.textbox.lostfocus();
    }
}


$(document).ready(function () {
    te.initialize();
}); 


window.onerror = function windowError(message, filename, lineno, colno, error) {
    var msg = "";
    if (message)
        msg = message;
    if (filename)
        msg += ", " + filename;
    if (lineno)
        msg += " (" + lineno + "," + colno + ")";
    if (error)
        msg += error;

    //var value = arguments.callee.caller;

    // show error messages in a little pop overwindow
    if (editorSettings.isDebug)
        status(msg);

    console.log(msg);

    if(textEditor)
        textEditor.lastError = msg; 

    // don't let errors trigger browser window
    return true;
}

window.onresize = debounce(function() {
        te.mm.textbox.resizeWindow();
    },
    200);

// prevent window from loading image/file
window.ondrop = function (event) {
    // don't allow dropping here - we can't get full file info
    event.preventDefault();
    event.stopPropagation();

    setTimeout(function () {
        te.mm.textbox.ShowMessage("To open dropped files in Markdown Monster, please drop files onto the header area of the window.","Invalid Drop Operation","Warning","Ok");
    },50);
}

//window.ondrop =
//    function (e) {
//        e.preventDefault();
//        e.stopPropagation();
        
//        debugger;

//        var dt = e.dataTransfer;
//        var files = dt.files;

//        var file = files[0];
//        console.log(file);

//        var reader = new FileReader();
//        reader.onload = function(e) {
//            var res = e.target.result;
//            var pos = $.extend({}, te.mousePos);
//            console.log(pos);
//            var sel = te.editor.getSelection();
//            var range = sel.getRange();
//            range.setStart(pos);
//            range.setEnd(pos);
//            sel.setSelectionRange(range);

//            setTimeout(function() {
//                    te.setselection(res);
//                },
//                20);
//        }
//        try {
//            bin = reader.readAsDataURL(file); //ReadAsArrayBuffer(file);
//        } catch (ex) {
//            alert(ex.message);
//        }
//    };
window.ondragover =
    function(e) {
        e.preventDefault();
        return false;
    };

 window.onmousewheel = function (e) {     
    if (e.ctrlKey) {
        e.cancelBubble = true;
        e.returnValue = false;

        if (e.wheelDelta > 0)
            te.specialkey("ctrl-=");
        if (e.wheelDelta < 0)
            te.specialkey("ctrl--");

        return false;
    }
 }




// This function is global and called by the parent
// to pass in the form object and pass back the text
// editor instance that allows the parent to make
// calls into this component
function initializeinterop(helpBuilderForm, textbox) {

    te.mm = {};
    te.mm.helpBuilderForm = helpBuilderForm;    
    te.mm.textbox = textbox;

    return window.textEditor;
}


function status(msg) {
    var $el = $("#message");
    if (!msg)
        $el.hide();
    else {        
        var dt = new Date();
        $el.text(dt.getHours() + ":" + dt.getMinutes() + ":" +
            dt.getSeconds() + "." + dt.getMilliseconds() +
            ": " + msg);
        $el.show();
        setTimeout(function() { $("#message").fadeOut() }, 10000);
    }
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

