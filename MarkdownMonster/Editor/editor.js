/// <reference path="editorsettings.js"/>
/// <reference path="editorSpellcheck.js"/>

// NOTE: All method and property names have to be LOWER CASE!
//       in order for FoxPro to be able to access them here.
var te = window.textEditor = {
    fox: null, // FoxPro COM object
    editor: null, // Ace Editor instance
    settings: editorSettings,
    lastError: null,
    dic: null,
    aff: null,
    isDirty: false,
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
            "ctrl-n": function() { te.specialkey("ctrl-n"); }, // let WPF
            "alt-c":  function() { te.specialkey("alt-c"); },
            "ctrl-o": function() { te.specialkey("ctrl-o"); },
            "ctrl-s": function() { te.specialkey("ctrl-s"); },
            "ctrl-b": function() { te.specialkey("ctrl-b"); },
            "ctrl-i": function() { te.specialkey("ctrl-i"); },
            "ctrl-l": function() { te.specialkey("ctrl-l"); },
            "ctrl-k": function() { te.specialkey("ctrl-k"); },
            "ctrl-shift-down": function () { te.specialkey("ctrl-shift-down"); },
            "ctrl-shift-up": function () { te.specialkey("ctrl-shift-up"); },
            "ctrl-shift-c": function () { te.specialkey("ctrl-shift-c"); },
            "ctrl-shift-v": function () { te.specialkey("ctrl-shift-v"); }
        });
        
        editor.renderer.setPadding(15);
        editor.renderer.setScrollMargin(5, 5, 0, 0); // top,bottom,left,right

        //te.editor.getSession().setMode("ace/mode/markdown" + lang);   

        // fill entire view
        te.editor.setOptions({
            maxLines: 0,
            minLines: 0
            //wrapBehavioursEnabled: editorSettings.wrapText
        });


        var keyHandler = debounce(keyHandler, 340);

        $("pre[lang]")        
            .on("keydown",
            function keyDownHandler(e) {
                if (!te.fox)
                    return;

                if (!te.isDirty) {
                    var keycode = e.keyCode;
                    var valid =
                        (e.keycode > 47 && keycode < 58) || // number keys
                            keycode == 32 ||
                            keycode == 13 || // spacebar & return key(s) (if you want to allow carriage returns)
                            (keycode > 64 && keycode < 91) || // letter keys
                            (keycode > 95 && keycode < 112) || // numpad keys
                            (keycode > 185 && keycode < 193) || // ;=,-./` (in order)
                            (keycode > 218 && keycode < 223); // [\]' (in order)
                    // backspace, tab -> handle in key up

                    if (valid) {
                        te.isDirty = true;
                        te.fox.textbox.setDirty(true);
                    }
                }
            });
        $("pre[lang]")
            .on("keyup",
                function keyUpHandler(e) {
                    if (!te.fox)
                        return;

                    // CR , . ? ! '
                    if (e.keyCode == 13 ||
                        e.keyCode == 190 ||
                        e.keyCode == 188 ||
                        e.keyCode == 191 ||
                        e.keyCode == 49 || e.keyCode == 222)
                        te.fox.textbox.PreviewMarkdownCallback();


                    // handle tab/backspace in keyup - not working in keydown
                    if (!te.isDirty) {
                        if (e.keyCode == 8 || e.keyCode == 9) {
                            te.isDirty = true;
                            te.fox.textbox.setDirty(true);
                        }
                    }
                });

        return editor;
    },
    initializeeditor: function() {
        te.configureAceEditor(null, null);
    },
    status: function status(msg) {
        alert(msg);
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
        te.editor.getSession().setUndoManager(new ace.UndoManager())

        setTimeout(function() {
                te.editor.resize(true); //force a redraw
            },
            30);
    },
    refresh: function(ignored) {
        te.editor.resize(true); //force a redraw
    },
    specialkey: function(key)   {
        te.fox.textbox.SpecialKey(key);
    },
    setfont: function (size, fontFace, weight) {
        if (size)
            te.editor.setFontSize(size);
        if (fontFace)
            te.editor.setOption('fontFamily', fontFace);
        if (weight)
            te.editor.setOption('fontWeight', weight);
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
        return te.fox.textbox.CheckSpelling(word,editorSettings.dictionary,false);
    },
    suggestSpelling: function (word, maxCount) {
        if (!editorSettings.enableSpellChecking)
            return null;

        // use typo
        if (spellcheck.dictionary)
            return spellcheck.dictionary.suggest(word);
        
        // use COM object
        var words = te.fox.textbox.GetSuggestions(word, editorSettings.dictionary, false);       
        if (!words)
            return [];

        words = JSON.parse(words);
        if (words.length > maxCount)
            words = words.slice(0, maxCount);

        return words;
    },
    addWordSpelling: function (word) {        
        te.fox.textbox.AddWordToDictionary(word, editorSettings.dictionary);
    },
    onblur: function () {
        fox.textbox.lostfocus();
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

window.ondrop = function (event) {
    // don't allow dropping here - we can't get full file info
    event.preventDefault();
    event.stopPropagation();

    setTimeout(function() {
        alert("To open dropped files in Markdown Monster, please drop files onto the header area of the window.");
    },50);
}
 window.ondragover = function(event) {
 	event.preventDefault();
 	return false;
 }


// This function is global and called by the parent
// to pass in the form object and pass back the text
// editor instance that allows the parent to make
// calls into this component
function initializeinterop(helpBuilderForm, textbox) {

    te.fox = {};
    te.fox.helpBuilderForm = helpBuilderForm;    
    te.fox.textbox = textbox;

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

