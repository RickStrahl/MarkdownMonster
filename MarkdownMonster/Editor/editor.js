/// <reference path="editorsettings.js"/>
/// <reference path="editorSpellcheck.js"/>

// NOTE: All method and property names have to be LOWER CASE!
//       in order for FoxPro to be able to access them here.
var te = window.textEditor = {
    mm: null, // FoxPro COM object
    editor: null, // Ace Editor instance
    previewRefresh: 800,
    settings: editorSettings,
    lastError: null,
    dic: null,
    aff: null,
    isDirty: false,
    mousePos: { column: 0, row: 0 },

    initialize: function() {

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
        editor.setHighlightActiveLine(editorSettings.highlightActiveLine);
        editor.setShowPrintMargin(editorSettings.showPrintMargin);

        //te.settheme(editorSettings.theme, editorSettings.fontSize, editorSettings.wrapText);
        editor.setTheme("ace/theme/" + editorSettings.theme);
        editor.setOptions({
            fontFamily: editorSettings.font,
            fontSize: editorSettings.fontSize
        });

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
                te.specialkey("ctrl-n");
                // do nothing but:
                // keep ctrl-n browser behavior from happening
                // and let WPF handle the key
            },
            "f5": function() {
                // avoid page refresh
            },
            // save
            "ctrl-s": function() { te.specialkey("ctrl-s"); },
            // Open document
            "ctrl-o": function() {
                te.editor.blur(); // HACK: avoid letter o insertion into document IE bug
                te.specialkey("ctrl-o");
                setTimeout(function() { te.editor.focus(); }, 20);
            },

            // link
            "ctrl-k": function() { te.specialkey("ctrl-k"); },
            // print
            "ctrl-p": function() { te.specialkey("ctrl-p") },
            // turn lines into list
            "ctrl-l": function () { te.specialkey("ctrl-l"); },
            // Emoji
            "ctrl-j": function () { te.specialkey("ctrl-j") },

            // Image emedding
            "alt-i": function() { te.specialkey("alt-i"); },

            // find again redirect
            "f3": function () { te.editor.execCommand("findnext") },
            // embed code
            "alt-c": function () { te.specialkey("alt-c"); },
            // inline code 
            "ctrl-`": function () { te.specialkey("ctrl-`"); },
            
            "ctrl-b": function() { te.specialkey("ctrl-b"); },
            "ctrl-i": function () { te.specialkey("ctrl-i"); },
            
            
            
            

            // take over Zoom keys and manually zoom
            "ctrl--": function() {
                te.specialkey("ctrl--");
                return null;
            },
            "ctrl-=": function() {
                te.specialkey("ctrl-=");
                return null;
            },
            //"alt-shift-enter": function() { te.specialkey("alt-shift-enter")},
            "ctrl-shift-down": function() { te.specialkey("ctrl-shift-down"); },
            "ctrl-shift-up": function() { te.specialkey("ctrl-shift-up"); },
            "ctrl-shift-c": function() { te.specialkey("ctrl-shift-c"); },
            "ctrl-shift-v": function() { te.specialkey("ctrl-shift-v"); },
            "ctrl-v": function() { te.mm.textbox.PasteOperation(); }
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

        // var keydownHandler = function keyDownHandler(e) {
        //     if (e.ctrlKey && e.shiftKey) {
        //         te.mm.textbox.PreviewMarkdownCallback();
        //         te.updateDocumentStats();
        //     }
        // };
        //$("pre[lang]").on("keydown", keydownHandler);


        var updateDocument = debounce(function() {
                te.isDirty = te.mm.textbox.IsDirty();
                te.mm.textbox.PreviewMarkdownCallback();
                te.updateDocumentStats();
            },
            te.previewRefresh);

        var keyupHandler = function keyUpHandler(e) {
            if (!te.mm)
                return;
            updateDocument();
        }
        $("pre[lang]").on("keyup", keyupHandler);


        // always have mouse position available when drop or paste
        te.editor.on("mousemove",
            function(e) {
                te.mousePos = e.getDocumentPosition();
            });
        te.editor.on("mouseup",
            function() {
                te.mm.textbox.PreviewMarkdownCallback();
                if (sc)
                    sc.contentModified = true;  // force recheck next cycle                
            });
      
        
        //if (window.EmojiCompleter) {
        //    // auto complete
        //    var langTools = ace.require("ace/ext/language_tools");
        //    editor.setOptions({
        //        enableBasicAutocompletion: true,
        //        enableSnippets: false,
        //        enableLiveAutocompletion: true
        //    });
        //    langTools.setCompleters([window.EmojiCompleter]);         
        //}

        return editor;
    },
    initializeeditor: function() {
        te.configureAceEditor(null, null);
    },
    status: function(msg) {
        //alert(msg);
        status(msg);
    },
    getscrolltop: function (ignored) {        
        var st = te.editor.getSession().getScrollTop();        
        return st;
    },
    setscrolltop: function (scrollTop) {
        setTimeout(function() {
                return te.editor.getSession().setScrollTop(scrollTop);
            },
            100);
    },
    getvalue: function(ignored) {
        var text = te.editor.getSession().getValue();
        return text.toString();
    },
    setvalue: function(text, pos) {
        if (!pos)
            pos = -1; // first line

        var offset = 0; // get cursor offset
        if (pos === -2) {
            try {
                offset = te.editor.session.doc.positionToIndex(te.editor.selection.getCursor());
            } catch (ex) {
                pos = -1; // go to top
            }
            if (offset == 0) // if 0 go to top
                pos = -1;
        }

        te.editor.setValue(text, pos);

        if (offset > 0) {
            te.setselposition(offset, 0);
        }

        te.editor.getSession().setUndoManager(new ace.UndoManager());

        setTimeout(function() {
                te.editor.resize(true); //force a redraw
            },
            30);
    },
    refresh: function(ignored) {
        te.editor.resize(true); //force a redraw
    },
    specialkey: function(key) {
        te.mm.textbox.SpecialKey(key);
    },
    setfont: function(size, fontFace, weight) {
        if (size)
            te.editor.setFontSize(size);
        if (fontFace)
            te.editor.setOption('fontFamily', fontFace);
        if (weight)
            te.editor.setOption('fontWeight', weight);
    },
    getfontsize: function() {
        var zoom = screen.deviceXDPI / screen.logicalXDPI;
        var fontsize = te.editor.getFontSize() * zoom;
        return fontsize;
    },

    gotoLine: function (line) {
        setTimeout(function() {
                te.editor.scrollToLine(line);
                var sel = te.editor.getSelection();
                var range = sel.getRange();
                range.setStart({ row: line, column: 0 });
                range.setEnd({ row: line, column: 0 });
                sel.setSelectionRange(range);

                setTimeout(te.refreshPreview, 10);
        },100);
    },
    gotoBottom: function (ignored) {
        setTimeout(function() {
                var row = te.editor.session.getLength() - 1;
                var column = te.editor.session.getLine(row).length; // or simply Infinity
                te.editor.selection.moveTo(row, column);
                setTimeout(te.refreshPreview, 10);                
            },
            70);
    },
    refreshPreview: function(ignored) {
        te.mm.textbox.PreviewMarkdownCallback();
    },
    setselection: function(text) {
        var range = te.editor.getSelectionRange();
        te.editor.session.replace(range, text);
    },
    getselection: function(ignored) {
        return te.editor.getSelectedText();
    },
    setselposition: function(index, count) {
        var doc = te.editor.session.getDocument();
        var lines = doc.getAllLines();

        var offsetToPos = function(offset) {
            var row = 0, col = 0;
            var pos = 0;
            while (row < lines.length && pos + lines[row].length < offset) {
                pos += lines[row].length;
                pos++; // for the newline
                row++;
            }
            col = offset - pos;
            return { row: row, column: col };
        };

        var start = offsetToPos(index);
        var end = offsetToPos(index + count);

        var sel = te.editor.getSelection();
        var range = sel.getRange();
        range.setStart(start);
        range.setEnd(end);
        sel.setSelectionRange(range);
    },
    setselpositionfrommouse: function(pos) {
        if (!pos)
            pos = $.extend({}, te.mousePos);

        var sel = te.editor.getSelection();
        var range = sel.getRange();
        range.setStart(pos);
        range.setEnd(pos);
        sel.setSelectionRange(range);
    },
    getCursorPosition: function (ignored) { // returns {row: y, column: x}        
        return te.editor.getCursorPosition();
    },

    setCursorPosition: function(row, column) { // col and also be pos: { row: y, column: x }  
        var pos;        
        if (typeof row === "object")
            pos = row;
        else
            pos = { column: column, row: row };
        
         te.editor.selection.moveTo(pos.row, pos.column);
    },
    moveCursorLeft: function (count) {
        if (!count)
            count = 1;
        var sel = te.editor.getSelection();
        
        for (var i = 0; i < count; i++) {
           sel.moveCursorLeft();    
        }        
    },
    moveCursorRight: function (count) {
        if (!count)
            count = 1;
        var sel = te.editor.getSelection();
        for (var i = 0; i < count; i++) {
            sel.moveCursorRight();
        }
        
    },
    getLineNumber: function(ignored) {
        var selectionRange = te.editor.getSelectionRange();
        if (!selectionRange) {
            status("no selection range...");
            return -1;
        }
        return Math.floor(selectionRange.start.row);
    },
    getCurrentLine: function(ignored) {
        var selectionRange = te.editor.getSelectionRange();
        var startLine = selectionRange.start.row;
        return te.editor.session.getLine(startLine);
    },
    findAndReplaceText: function(search, replace) {
        var range = te.editor.find(search,
        {
            wrap: true,
            caseSensitive: true,
            wholeWord: true,
            regExp: false,
            preventScroll: false // do not change selection
        });
        if (!range)
            return;

        range.start.column = 0;        
        range.end.column = 2000;
        
        te.editor.session.replace(range, replace);        
    },
    findAndReplaceTextInCurrentLine: function (search, replace) {
        var range = te.editor.getSelectionRange();
        var startLine = range.start.row;
        var lineText = te.editor.session.getLine(startLine);

        var i = lineText.indexOf(search);
        if (i === -1)
            return;
        
        range.start.column = i;
        range.end.column = i + search.length;

        te.editor.session.replace(range, replace);
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
            lang = "txt";
        if (lang == "vfp")
            lang = "foxpro";
        if (lang == "c#")
            lang = "csharp";
        if (lang == "c++" || lang == "cpp")
            lang = "c_cpp";
        if (lang == "txt" || lang == "text" || lang == "none" || lang == "plain")
            lang = "txt";

        te.editor.getSession().setMode("ace/mode/" + lang);
    },
    settheme: function (theme, font, fontSize, wrapText, highlightActiveLine,keyboardHandler) {
        te.editor.setTheme("ace/theme/" + theme);

        te.editor.setOptions({
            fontFamily: font,
            fontSize: fontSize
        });
        
        wrapText = wrapText || false;        

        var session = te.editor.getSession();
        session.setUseWrapMode(wrapText);
        session.setOption("indentedSoftWrap", true);        

        te.editor.setHighlightActiveLine(highlightActiveLine);        

        keyboardHandler = keyboardHandler.toLowerCase();
        if (!keyboardHandler || keyboardHandler == "default" || keyboardHandler == "ace")
            te.editor.setKeyboardHandler("");
        else
            te.editor.setKeyboardHandler("ace/keyboard/" + keyboardHandler);

        setTimeout(te.updateDocumentStats, 100);
    },
    setShowLineNumbers: function(showLineNumbers) { 
        te.editor.renderer.setShowGutter(showLineNumbers);  
    },
    execcommand: function(cmd,parm1,parm2) {
        te.editor.execCommand(cmd);
    },
    curStats: { wordCount: 0, lines: 0, characters: 0 },
    getDocumentStats: function () {
        var text = te.getvalue();

        // strip off blog post meta data at end of document
        var pos = text.indexOf("\n<!-- Post Configuration -->");
        if (pos > 0)
            text = text.substr(0, pos - 1);

        // strip of front matter
        if (text.substr(0, 4) === "---\r" || text.substr(0, 4) === "---\n") {
            pos = text.indexOf("\n---\n");
            if (pos < 0)
                pos = text.indexOf("\n---\r\n");
            if (pos > -1) {
                pos += 6;
                text = text.substr(pos);
            }
        }

        var regExWords = /\s+/gi;
        var wordCount = text.replace(regExWords, ' ').split(' ').length;                
        var lines = text.split('\n').length;
        var chars = text.length;

        te.curStats = {
            wordCount: wordCount,
            lines: lines,
            characters: chars
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

    if (textEditor)
        textEditor.lastError = msg; 

    console.log(msg,filename,lineno,colno,error);

    // don't let errors trigger browser window
    return true;
}

window.onresize = debounce(function() {
        te.mm.textbox.resizeWindow();
    },
    200);

// prevent window from loading image/file
//window.ondrop = function (e) {
//    // don't allow dropping here - we can't get full file info
//    event.preventDefault();
//    event.stopPropagation();

//    te.mm.textbox.fileDropOperation();


//    setTimeout(function () {
//        te.mm.textbox.ShowMessage("To open or embed dropped files in Markdown Monster, please drop files onto the header area of the window.\r\n\r\n" +
//            "You can drop text files to open and edit, or images to embed at the cursor position in the open document.",
//            "Invalid Drop Operation", "Warning", "Ok");
//    },50);
//}

//window.ondrop =
//    function (e) {
//        // don't let files be dropped or the document 
//        // is replaced.
//        e.preventDefault();
//        e.stopPropagation();

//        var dt = e.dataTransfer;
//        var files = dt.files;

//        var file = files[0];        
        
//        var reader = new FileReader();
//        reader.onload = function(e) {
//            var res = e.target.result;

//            var pos = $.extend({}, te.mousePos);

//            var sel = te.editor.getSelection();
//            var range = sel.getRange();
//            range.setStart(pos);
//            range.setEnd(pos);
//            sel.setSelectionRange(range);

//            te.mm.textbox.FileDropOperation(res, file.name);
//        }
//        try {
//            bin = reader.readAsDataURL(file); //ReadAsArrayBuffer(file);
//        } catch (ex) {
//            status("Drag and drop error: " + ex.message);
//        }
//    };
//window.ondragover =
//    function(e) {
//        e.preventDefault();
//        return false;
//    };

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
function initializeinterop(textbox) {
    te.mm = {};    
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
        setTimeout(function() { $("#message").fadeOut() }, 5000);
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
