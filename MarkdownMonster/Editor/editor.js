/// <reference path="editorsettings.js"/>
/// <reference path="editorSpellcheck.js"/>

// NOTE: All method and property names have to be LOWER CASE!
//       in order for FoxPro to be able to access them here.
var te = window.textEditor = {
    mm: null, // Markdown Monster MarkdownDocumentEditor COM object
    editor: null, // Ace Editor instance
    previewRefresh: 800,
    settings: editorSettings,
    lastError: null,
    dic: null,
    aff: null,
    isDirty: false,
    mousePos: { column: 0, row: 0 },
    spellcheck: null,

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
        editor.setShowInvisibles(editorSettings.showInvisibles);

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
        editor.setOption("scrollPastEnd", 0.7); // will have additional scroll  0.7% of screen height
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
            //"f1": function () {
            //    te.specialkey("f1");
            //},
            // save
            "ctrl-s": function () {
                te.mm.textbox.IsDirty(); // force document to update
                te.specialkey("ctrl-s");
            },
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

            // delete line
            "shift-del": te.deleteCurrentLine,

            // try to move between tabs
            "ctrl-tab": function () { te.specialkey("ctrl-tab"); },

            "ctrl-shift-tab": function () { te.specialkey("ctrl-shift-tab"); },

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
            "ctrl-shift-up": function () { te.specialkey("ctrl-shift-up"); },

            // Paste as Markdown/From Html
            "ctrl-shift-c": function() { te.specialkey("ctrl-shift-c"); },
            "ctrl-shift-v": function () { te.specialkey("ctrl-shift-v"); },

            // remove markdown formatting
            "ctrl-shift-z": function () { te.specialkey("ctrl-shift-z"); },

            // Capture paste operation in WPF to handle Images
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

        var updateDocument = debounce(function() {
            if (!te.mm)
                return;
            te.isDirty = te.mm.textbox.IsDirty();
            te.mm.textbox.PreviewMarkdownCallback(true);  // don't get markdown again
            te.updateDocumentStats();
        },te.previewRefresh);  
        $("pre[lang]").on("keyup", updateDocument);


        // always have mouse position available when drop or paste
        te.editor.on("mousemove",
            function(e) {
                te.mousePos = e.getDocumentPosition();                
            });
        te.editor.on("mouseup",
            function () {
                if (!te.mm)
                    return;

                te.mm.textbox.PreviewMarkdownCallback();

                // spellcheck - force recheck on next cycle
                if (sc)
                    sc.contentModified = true;                  
            });
        // used to force mouse position to whatever the existing cursor position is
        // when dragging from explorer. Without this files are always dropped at the
        // end of the document. With this it's dropped at the current cursor position
        // (better but not optimal)
        window.ondragover = function (e) {            
            te.mousePos = te.editor.getCursorPosition();            
        }
        // Let browser navigate events handle drop operations
        // in the WPF host application
        // handle file browser dragged files dropped
        // *** Don't Remove! Explorer dragging captures navigation event in WPF
        //     This captures requests from the Filebrowser
        window.ondrop =
            function (e) {
                // these don't really have any effect'
                //e.stopPropagation();
                //e.preventDefault();		    
                var file = e.dataTransfer.getData('text');

                console.log('ondrop');

                // image file names dropped from FolderBrowser
                //if (file && /(.png|.jpg|.gif|.jpeg|.bmp|.svg)$/i.test(file)) {
                if (file && /\..\w*$/.test(file)) {
                    //// IE will *ALWAYS* drop the file text but selects the drops text
                    //// delay and the collapse selection and let
                    //// WPF paste the image expansion
                    setTimeout(function () {
                        // embed the image or open the file
                        te.mm.textbox.EmbedDroppedFileAsImage(file);
                    }, 1);

                    te.setselection(''); // collapse and remove file name dragged into doc

                    return false;
                }
            };
        // this doesn't fire
        //te.editor.on("dragover",
        //    function (e) {                
        //        alert('drag over');
        //        te.mousePos = e.getDocumentPosition();
        //    });
        var changeScrollTop = debounce(function () {
                // if there is a selection don't set cursor position
                // or preview. Mouseup will scroll to position at end
                // of selection
                var sel = te.getselection();
                if (sel && sel.length > 0)
                    return;

                var firstRow = te.editor.renderer.getFirstVisibleRow();
                var lastRow = te.editor.renderer.getLastVisibleRow();
                var curRow = te.getLineNumber();            
                if (curRow < firstRow || curRow > lastRow) {
                    if (firstRow < 3)
                        te.setCursorPosition(0, 0);
                    else                        
                        te.setCursorPosition(firstRow + 3, 0);
                    
                    setTimeout(function() {                        
                        te.mm.textbox.PreviewMarkdownCallback();
                    }, 10);                    
                }
            },
            10);
        te.editor.session.on("changeScrollTop", changeScrollTop);
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
    editorSelectionOperation: function(action, text) {
        te.mm.textbox.EditorSelectionOperation(action, text);
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

        function offsetToPos(offset) {
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
        return te.editor.selection.getCursor();        
    },

    setCursorPosition: function(row, column) { // col and also be pos: { row: y, column: x }  
        var pos;        
        if (typeof row === "object")
            pos = row;
        else
            pos = { column: column, row: row };

        te.editor.gotoLine(pos.row, pos.column, true);        
    },
    setSelectionRange: function (startRow, startColumn, endRow, endColumn) {
        var sel = te.editor.getSelection();
        var range = sel.getRange();        
        range.setStart({ row: startRow, column: startColumn });
        range.setEnd({ row: endRow, column: endColumn });
        sel.setSelectionRange(range);       
    },
    deleteCurrentLine: function () {
        var sel = te.getselection();
        if (sel) {
            document.execCommand('cut');
            return;
        }

        te.editor.selection.selectLine();
        te.editor.removeLines();        
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
    getLine: function(row) {
        return te.editor.session.getLine(row);
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
    setShowInvisibles: function (showInvisibles) { 
        te.editor.renderer.setShowInvisibles(showInvisibles);  
    },
    setWordWrap: function (enable) {
        te.editor.session.setUseWrapMode(enable);
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

        // strip off front matter.
        var frontMatterExp = /^---[ \t]*$[^]+?^(---|...)[ \t]*$/m;
        var match = frontMatterExp.exec(text);
        if (match && match.index == 0)
            text = text.substr(match[0].length);

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
    showSuggestions: function (e) {
        try {
            var markers = te.editor.session.getMarkers(true);
            if (!markers || markers.length === 0)
                return;

            var pos = e && e.getDocumentPosition ? e.getDocumentPosition() : te.editor.selection.getCursor();

            var matched = null;

            // look for a misspelled marker that matches our
            // current document location
            for (var id in markers) {
                var marker = markers[id];
                if (marker.clazz != "misspelled")
                    continue;

                if (pos.row >= marker.range.start.row &&
                    pos.row <= marker.range.end.row &&
                    pos.column >= marker.range.start.column &&
                    pos.column <= marker.range.end.column) {
                    matched = marker;
                    break;
                }
            };

            var range = null;
            var misspelledWord = null;
            if (matched) {
                range = matched.range;
                misspelledWord = matched.range.misspelled;
            }

            // show suggested spellings in WPF Context Menu
            //te.suggestSpelling(misspelledWord, 8, range);
            te.mm.textbox.GetSuggestions(misspelledWord, editorSettings.dictionary, false, range);

        } catch (error) {
            alert(error.message);
        }
    },
    addWordSpelling: function (word) {        
        te.mm.textbox.AddWordToDictionary(word, editorSettings.dictionary);
        if (sc)
            sc.spellCheck(true);
    },
    replaceSpellRange: function(range, text) {
        te.editor.getSession().replace(range, text); 
        if (sc)
            sc.spellCheck(true);
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


window.onmousewheel = function(e) {
	if (e.ctrlKey) {
		e.cancelBubble = true;
		e.returnValue = false;

		if (e.wheelDelta > 0)
			te.specialkey("ctrl-=");
		if (e.wheelDelta < 0)
			te.specialkey("ctrl--");

		return false;
	}
};



//window.ondragover = function (e) {
//    te.mousePos = e.getDocumentPosition(); 
//    console.log('ondragover');
//}
 //window.ondragstart = function (e) {    
 //    e.dataTransfer.effectAllowed = 'all';  
 //    console.log('ondragstart');
 //}

// pass context popup to WPF for handling there
window.oncontextmenu = function (e) {
    e.preventDefault();
    e.cancelBubble = true;

    te.showSuggestions(e);

    return navigator.userAgent.indexOf("Trident") > -1 ? false : true;
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



/* ** Helpers  ** */

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
String.prototype.startsWith = function (sub, nocase) {
    if (!this || this.length === 0 || sub === null) return false;

    if (sub && nocase)
        return this.toLowerCase().indexOf(sub.toLowerCase()) === 0;
       
    return this.indexOf(sub) === 0;
}
String.prototype.endsWith = function (sub, nocase) {
    if (!this || this.length === 0) return false;

    var ix = 0;
    if (sub && nocase) {
        ix = this.toLowerCase().lastIndexOf(sub.toLowerCase());
        if (ix > 0 && ix + sub.length === this.length)
            return true;
        return false;
    }

    ix = this.lastIndexOf(sub);
    if (ix > 0 && ix + sub.length === this.length)
        return true;
    return false;
}
String.prototype.extract = function (startDelim, endDelim, allowMissingEndDelim, returnDelims) {
    var str = this;
    if (str.length === 0)
        return "";

    var src = str.toLowerCase();
    startDelim = startDelim.toLocaleLowerCase();
    endDelim = endDelim.toLocaleLowerCase();

    var i1 = src.indexOf(startDelim);
    if (i1 == -1)
        return "";

    var i2 = src.indexOf(endDelim, i1 + startDelim.length);

    if (!allowMissingEndDelim && i2 == -1)
        return "";

    if (allowMissingEndDelim && i2 == -1) {
        if (returnDelims)
            return str.substr(i1);

        return str.substr(i1 + startDelim.length);
    }

    if (returnDelims)
        return str.substr(i1, i2 - i1 + startDelim.length);

    return str.substr(i1 + startDelim.length, i2 - i1 - startDelim.length);
};
