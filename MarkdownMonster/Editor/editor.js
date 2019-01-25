/// <reference path="editorsettings.js"/>
/// <reference path="editorSpellcheck.js"/>

(function () {
 
  var Split = ace.require("ace/split").Split;


  var te = window.textEditor = {
    mm: null, // Markdown Monster MarkdownDocumentEditor COM object    

    // Editor and Split instances
    splitInstance: null,
    editor: null, // Ace Editor instance - can be a split instance
    mainEditor: null, // The main editor instance (root instance)
    editorElement: null, // Ace Editor DOM element bount to

    previewRefresh: 800,
    settings: editorSettings,
    lastError: null,
    dic: null,
    aff: null,
    isDirty: false,
    mousePos: { column: 0, row: 0 },
    spellcheck: null,
    codeScrolled: null,

    setCodeScrolled: function(ignored) {
      te.codeScrolled = new Date().getTime();
    },
    initialize: function() {
      // attach ace to formatted code controls if they are loaded and visible
      var $el = $("pre[lang]");
      te.editorElement = $el[0];
      try {
        var codeLang = $el.attr('lang');

        //te.editor = ace.edit(te.editorElement);

        //Splitting          
        var theme = "ace/theme/twilight";
        var split = new Split(te.editorElement, theme, 1);
        te.splitInstance = split;
        te.editor = split.getEditor(0);
        te.mainEditor = te.editor; // keep track of the main editor        
        split.on("focus",
          function(editor) {
            te.editor = editor;
          });

        te.configureAceEditor(te.editor, editorSettings);
        te.setlanguage(codeLang);
      } catch (ex) {
        if (typeof console !== "undefined")
          console.log("Failed to bind syntax: " + codeLang + " - " + ex.message);
      }
    },
    configureAceEditor: function(editor, editorSettings) {
      if (!editor)
        editor = te.editor;
      if (!editorSettings)
        editorSettings = te.settings;

      var session = editor.getSession();
      session.name = "markdownmonster_" + new Date().getTime();

      var theme = "ace/theme/" + editorSettings.theme;
      editor.setTheme(theme);

      te.editor.name = "Editor1";

      // basic configuration
      editor.setReadOnly(false);
      editor.setHighlightActiveLine(editorSettings.highlightActiveLine);
      editor.setShowPrintMargin(editorSettings.showPrintMargin);
      editor.setShowInvisibles(editorSettings.showInvisibles);


      editor.setOptions({
        fontFamily: editorSettings.font,
        fontSize: editorSettings.fontSize
      });

      // allow editor to soft wrap text
      session.setUseWrapMode(editorSettings.wrapText);
      session.setOption("indentedSoftWrap", false);

      editor.renderer.setShowGutter(editorSettings.showLineNumbers);
      editor.setOption("scrollPastEnd", 0.7); // will have additional scroll  0.7% of screen height
      editor.$blockScrolling = Infinity;

      session.setTabSize(editorSettings.tabSpaces);

      //set.$bidiHandler.setRtlDirection(editor, true);

      session.setNewLineMode("windows");

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
          if (te.isDirty) {
            te.mm.textbox.PreviewMarkdownCallback(true); // don't get markdown again
            te.updateDocumentStats();
          }
        },
        te.previewRefresh);

      $("pre[lang]").on("keyup", updateDocument);


      // always have mouse position available when drop or paste
      te.editor.on("mousemove",
        function(e) {
          te.mousePos = e.getDocumentPosition();
        });
      te.editor.on("mouseup",
        function() {
          if (te.mm)
            te.mm.textbox.PreviewMarkdownCallback(true);

          // spellcheck - force recheck on next cycle
          if (sc)
            sc.contentModified = true;
        });

      // Notify WPF of focus change
      te.editor.on("blur", te.onBlur);
      te.editor.on("focus", te.onGotFocus);

      // used to force mouse position to whatever the existing cursor position is
      // when dragging from explorer. Without this files are always dropped at the
      // end of the document. With this it's dropped at the current cursor position
      // (better but not optimal)
      window.ondragover = function(e) {
        te.mousePos = te.editor.getCursorPosition();
      }
      // Let browser navigate events handle drop operations
      // in the WPF host application
      // handle file browser dragged files dropped
      // *** Don't Remove! Explorer dragging captures navigation event in WPF
      //     This captures requests from the Filebrowser
      window.ondrop =
        function(e) {
          // these don't really have any effect'
          //e.stopPropagation();
          //e.preventDefault();		    
          var file = e.dataTransfer.getData('text');

          // image file names dropped from FolderBrowser
          //if (file && /(.png|.jpg|.gif|.jpeg|.bmp|.svg)$/i.test(file)) {
          if (file && /\..\w*$/.test(file)) {
            //// IE will *ALWAYS* drop the file text but selects the drops text
            //// delay and the collapse selection and let
            //// WPF paste the image expansion
            setTimeout(function() {
                // embed the image or open the file
                te.mm.textbox.EmbedDroppedFileAsImage(file);
              },
              1);

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

      var changeScrollTop = debounce(function(e) {
          // don't do anything if we moved without requesting
          // a document refresh (from preview refresh)
          if (te.codeScrolled) {
            var t = new Date().getTime();
            if (te.codeScrolled > t - 850)
              return;
          }
          te.codeScrolled = null;

          // if there is a selection don't set cursor position
          // or preview. Mouseup will scroll to position at end
          // of selection            
          var sel = te.getselection();
          if (sel && sel.length > 0)
            return;

          setTimeout(function() {
              var firstRow = te.editor.renderer.getFirstVisibleRow();
              if (firstRow > 2)
                firstRow += 3;

              // preview and highlight top of display
              te.mm.textbox.PreviewMarkdownCallback(true, firstRow);

              if (sc)
                sc.contentModified = true;
            },
            90);
        },
        35);
      te.editor.session.on("changeScrollTop", changeScrollTop);
      return editor;
    },
    initializeeditor: function() {
      te.configureAceEditor(null, null);
    },
    status: function status(msg) {
      //alert(msg);
      status(msg);
    },
    getscrolltop: function(ignored) {
      var st = te.editor.getSession().getScrollTop();
      return st;
    },
    setscrolltop: function(scrollTop) {
      setTimeout(function() {
          return te.editor.getSession().setScrollTop(scrollTop);
        },
        100);
    },
    getvalue: function(ignored) {
      var text = te.editor.getSession().getValue();
      return text.toString();
    },
    setvalue: function(text, pos, keepUndo) {
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
      if (offset > 0)
        te.setselposition(offset, 0);

      if (!keepUndo) {
        // load a new document
        te.editor.getSession().setUndoManager(new ace.UndoManager());
        setTimeout(function() {
            te.editor.resize(true); //force a redraw
          },
          30);
      }
    },
    setReadOnly: function(status) {
      if (te.editor.readOnly == status)
        return;
      te.editor.setReadOnly(status);
      //.readOnly = status;        
      if (status) {
        te.editor.container.style.opacity = 0.70;
        $(te.editor.container).on("dblclick", te.readOnlyDoubleClick);
      } else {
        $(te.editor.container).off("dblclick", te.readOnlyDoubleClick);
        te.editor.container.style.opacity = 1; // or use svg filter to make it gray            
      }
    },
    readOnlyDoubleClick: function() {
      if (!te.mm)
        return;

      te.mm.textbox.NotifyAddins("ReadOnlyEditorDoubleClick", null);
    },
    // replaces content without completely reloading the document
    // by using clipboard replacement
    // Leaves scroll position intact
    replacecontent: function(text) {
      var sel = te.editor.getSelection();
      sel.selectAll();
      te.setselection(text);
    },
    refresh: function(ignored) {
      te.editor.resize(true); //force a redraw
    },
    keyboardCommand: function(key) {
      if (te.mm)
        te.mm.textbox.keyboardCommand(key);
    },
    editorSelectionOperation: function(action, text) {
      if (te.mm)
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

    gotoLine: function(line, noRefresh, noSelection) {
      setTimeout(function() {
          te.editor.scrollToLine(line);

          if (!noSelection) {
            var sel = te.editor.getSelection();
            var range = sel.getRange();
            range.setStart({ row: line, column: 0 });
            range.setEnd({ row: line, column: 0 });
            sel.setSelectionRange(range);
          }
          if (!noRefresh)
            setTimeout(te.refreshPreview, 10);
          else
            te.codeScrolled = new Date().getTime();

        },
        70);
    },
    gotoBottom: function(noRefresh) {
      setTimeout(function() {
          var row = te.editor.session.getLength() - 1;
          var column = te.editor.session.getLine(row).length; // or simply Infinity
          te.editor.selection.moveTo(row, column);

          if (!noRefresh)
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
      te.editor.renderer.scrollSelectionIntoView();
    },

    getselection: function(ignored) {
      return te.editor.getSelectedText();
    },
    getselectionrange: function(ignored) {
      var range = te.editor.getSelectionRange();
      return {
        startRow: range.start.row,
        endRow: range.end.row,
        startColumn: range.start.column,
        endColumn: range.end.column
      };
    },
    getCursorPosition: function(ignored) { // returns {row: y, column: x}               
      return te.editor.selection.getCursor();
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

      te.editor.renderer.scrollSelectionIntoView();
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
    setCursorPosition: function(row, column) { // col and also be pos: { row: y, column: x }  
      var pos;
      if (typeof row === "object")
        pos = row;
      else
        pos = { column: column, row: row };

      te.editor.gotoLine(pos.row, pos.column, true);
    },
    setSelectionRange: function(startRow, startColumn, endRow, endColumn) {
      var sel = te.editor.getSelection();
      // assume a selection range if an object is passed
      if (typeof startRow == "object") {
        sel.setSelectionRange(startRow);
        return;
      }


      var range = sel.getRange();
      range.setStart({ row: startRow, column: startColumn });
      range.setEnd({ row: endRow, column: endColumn });
      sel.setSelectionRange(range);
    },
    deleteCurrentLine: function() {
      var sel = te.getselection();
      if (sel) {
        document.execCommand('cut');
        return;
      }

      te.editor.selection.selectLine();
      te.editor.removeLines();
    },
    moveCursorLeft: function(count) {
      if (!count)
        count = 1;
      var sel = te.editor.getSelection();

      for (var i = 0; i < count; i++) {
        sel.moveCursorLeft();
      }
    },
    moveCursorRight: function(count) {
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
      range.end.column = 5000;


      te.setselection(replace);

    },
    findAndReplaceTextInCurrentLine: function(search, replace) {
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

    setlanguage: function(lang) {

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
    setRightToLeft: function(onOff) {
      te.editor.session.$bidiHandler.setRtlDirection(te.editor, onOff);
    },
    lastStyle: null,
    setEditorStyle: function(styleJson, editor) {

      if (!editor)
        editor = te.editor;

      setTimeout(function() {
          var style;
          if (typeof styleJson === "object")
            style = styleJson;
          else
            style = JSON.parse(styleJson);

          te.lastStyle = style;

          editor.container.style.lineHeight = style.LineHeight;

          var activeTheme = editor.getTheme();
          var theme = "ace/theme/" + style.Theme;
          if (activeTheme !== theme)
            editor.setTheme("ace/theme/" + style.Theme);

          editor.setOptions({
            fontFamily: style.Font,
            fontSize: style.FontSize
          });
          //setRightToLeft(style.RightToLeft);

          // these value are used in Resize to keep the editor size
          // limited to a max-width
          te.adjustPadding(true);

          var wrapText = style.WrapText;

          var session = editor.getSession();

          session.setUseWrapMode(wrapText);
          session.setOption("indentedSoftWrap", true);
          session.setOptions({ useSoftTabs: style.UseSoftTabs, tabSize: style.TabSize });

          editor.setHighlightActiveLine(style.HighlightActiveLine);

          editor.renderer.setShowGutter(style.ShowLineNumbers);
          editor.renderer.setShowInvisibles(style.ShowInvisibles);

          //style.wrapMargin = 50;
          //if (style.wrapMargin > 0) {
          //    session.setWrapLimitRange(style.wrapMarin, style.wrapMargin);
          //    te.editor.setShowPrintMargin(true);
          //    te.editor.setPrintMarginColumn(style.wrapMargin + 1);
          //} else {
          //    session.setWrapLimitRange(null, null);
          //    te.editor.setShowPrintMargin(false);                
          //}

          //var keyboardHandler = style.KeyboardHandler.toLowerCase();
          //if (!keyboardHandler || keyboardHandler == "default" || keyboardHandler == "ace")
          //    te.editor.setKeyboardHandler("");
          //else
          //    te.editor.setKeyboardHandler("ace/keyboard/" + keyboardHandler);


          if (!style.EnableBulletAutoCompletion) {
            // turn off bullet auto-completion (or any new line auto-completion)
            editor.getSession().getMode().getNextLineIndent = function(state, line) {
              return this.$getIndent(line);
            };
          }
        },
        1);

      setTimeout(te.updateDocumentStats, 100);
    },
    setShowLineNumbers: function(showLineNumbers) {
      te.editor.renderer.setShowGutter(showLineNumbers);
    },
    setShowInvisibles: function(showInvisibles) {
      te.editor.renderer.setShowInvisibles(showInvisibles);
    },
    setWordWrap: function(enable) {
      te.editor.session.setUseWrapMode(enable);
    },
    execcommand: function(cmd, parm1, parm2) {
      te.editor.execCommand(cmd);
    },
    curStats: { wordCount: 0, lines: 0, characters: 0 },
    getDocumentStats: function() {
      var text = te.getvalue();

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
      setTimeout(function() {
          te.mm.textbox.updateDocumentStats(te.getDocumentStats());
        },
        50);
    },
    enablespellchecking: function(disable, dictionary) {
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
    // force document to check spelling
    spellcheckDocument: function(force) {
      if (te.spellcheck)
        te.spellcheck.spellCheck(force);
    },
    spellcheckNext: function(ignored) {
      if (te.spellcheck)
        te.keyBindings.nextSpellCheckError();
    },
    spellcheckPrevious: function(ignored) {
      if (te.spellcheck)
        te.keyBindings.previousSpellCheckError();
    },
    checkSpelling: function(word) {
      if (!word || !editorSettings.enableSpellChecking)
        return true;
      if (te.mm)
      // use COM object        
        return te.mm.textbox.CheckSpelling(word, editorSettings.dictionary, false);
    },
    showSuggestions: function(e) {
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
    addWordSpelling: function(word) {
      te.mm.textbox.AddWordToDictionary(word, editorSettings.dictionary);
      if (sc)
        sc.spellCheck(true);
    },
    replaceSpellRange: function(range, text) {
      te.editor.getSession().replace(range, text);
      if (sc)
        sc.spellCheck(true);
    },

    gotfocus: function(ignored) {
      te.setfocus();
    },
    setfocus: function(ignored) {
      //te.editor.resize(true);

      setTimeout(function() {
          te.editor.focus();
          setTimeout(function() {
              te.editor.focus();
            },
            300);
        },
        50);
    },
    // forces Ace to lose focus
    losefocus: function(ignored) {
      $("#losefocus").focus();
    },
    onBlur: function() {
      if (te.mm)
        te.mm.textbox.LostFocus();
    },
    onGotFocus: function() {
      if (te.mm)
        te.mm.textbox.GotFocus();
    },
    blurEditorAndRefocus: function(mstimeout) {
      if (!mstimeout)
        mstimeout = 50;

      te.noRefreshPreview = true;
      te.editor.blur(); // HACK: avoid letter o insertion into document                 
      setTimeout(function() {
          te.editor.focus();
          te.noRefreshPreview = false;
        },
        mstimeout);
    },

    adjustPadding: function(forceRefresh) {
      if (!te.lastStyle || !te.splitInstance)
        return;

      var lastPad = te.lastStyle.Padding;

      // single pane
      if (!te.splitInstance || te.splitInstance.$splits < 2) {
        // just apply fixed padding
        if (te.lastStyle.MaxWidth == 0) {                    
          te.editor.renderer.setPadding(lastPad);
        } else {

          // Apply width
          var w = window.innerWidth - te.lastStyle.MaxWidth;
          if (w > te.lastStyle.Padding * 2) {
            var pad = w / 2;
            te.editor.renderer.setPadding(w / 2);
          } else
            te.editor.renderer.setPadding(lastPad);
        }
        te.splitInstance.resize();

        return;
      }


      // we have multiple panes

      var ed = te.splitInstance.getEditor(0);
      var ed2 = te.splitInstance.getEditor(1);


      // if there's no MaxWidth just apply fixed padding to both splits
      if (te.lastStyle.MaxWidth == 0) {
        ed2.renderer.setPadding(lastPad);
        ed.renderer.setPadding(lastPad);
      }
      // Horizontal splits      
      else if (te.splitInstance.getOrientation() == te.splitInstance.BESIDE) {
        // Set padding for two horizontal splits
        var w = window.innerWidth / 2 - te.lastStyle.MaxWidth;

        if (w > te.lastStyle.Padding * 2) {
          // calc padding
          var pad = Math.floor(w / 2);
          ed2.renderer.setPadding(pad);
          ed.renderer.setPadding(pad);
        } else {
          // smaller than max width - use padding setting          
          ed.renderer.setPadding(lastPad);
          ed2.renderer.setPadding(lastPad);          
        }
      }
      // vertical splits
      else {
        var w = window.innerWidth - te.lastStyle.MaxWidth;

        if (w > te.lastStyle.Padding * 2) {
          var pad = w / 2;
          ed2.renderer.setPadding(pad);
          ed.renderer.setPadding(pad);
        } else {
          ed.renderer.setPadding(lastPad);
          ed2.renderer.setPadding(lastPad);
        }
      }

      te.splitInstance.resize();
    },

    // Below or Beside or None
    split: function(value) {
      var split = te.splitInstance;

      if (value === "Below" || value === "Beside") {

        split.setOrientation(value == "Below" ? split.BELOW : split.BESIDE);
        split.setSplits(2);

        // IMPORTANT: reset large padding first - if padding is too large panel won't render right
        te.mainEditor.renderer.setPadding(te.lastStyle.Padding);
        te.mainEditor.resize(true);
        
        // setSplits creates a second editor instance
        var editor2 = split.getEditor(1);
        editor2.Name = "Editor2";

        // get the old window session and assign to the new editor instance
        var session = te.mainEditor.session; //split.getEditor(0).session;
        var newSession = split.setSession(session, 1);
        newSession.name = session.name;

        // Update editor to match current settings (app specific - cached JSON settings from WPF host)
        te.editor = editor2;

        te.configureAceEditor(editor2);
        te.setEditorStyle(te.lastStyle, editor2);
        te.keyBindings.setupKeyBindings();

      } else {
        te.editor = te.mainEditor;
        split.setSplits(1);
        te.setEditorStyle(te.lastStyle, te.editor);
      }

      te.setfocus();
    }
  }

// Remove - delay and let .NET initialize to
// avoid double styling flash
//$(document).ready(function () {
//  te.initialize();
//});


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

    console.log(msg, filename, lineno, colno, error);

    // don't let errors trigger browser window
    return true;
  }


  function windowResize() {
    //if (te.mm && te.mm.textbox)
    //  te.mm.textbox.resizeWindow();
    
    te.adjustPadding();
  }
  window.onresize = windowResize; //debounce(windowResize, 1);


  window.onmousewheel = function (e) {   
    if (e.ctrlKey) {
      e.cancelBubble = true;
      e.returnValue = false;

      if (e.wheelDelta > 0)
        te.keyboardCommand("ZoomEditorUp");
      if (e.wheelDelta < 0)
        te.keyboardCommand("ZoomEditorDown");

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
  window.oncontextmenu = function(e) {
    var isIE = navigator.userAgent.indexOf("Trident") > -1 ? true : false;
    if (!isIE)
      return;

    e.preventDefault();
    e.cancelBubble = true;

    if (te.mm)
      te.showSuggestions(e);

    return false;
  }


  function status(msg) {
    var $el = $("#message");
    if (!msg)
      $el.hide();
    else {
      var dt = new Date();
      $el.text(dt.getHours() +
        ":" +
        dt.getMinutes() +
        ":" +
        dt.getSeconds() +
        "." +
        dt.getMilliseconds() +
        ": " +
        msg);
      $el.show();
      setTimeout(function() { $("#message").fadeOut() }, 7000);
    }
  }




})();


// This function is global and called by the .NET parent app
// to pass in the form object and pass back the text
// editor instance that allows the parent to make
// calls into this component
function initializeinterop(textbox, jsonStyle) {  
  var te = window.textEditor;

  te.mm = {};
  te.mm.textbox = textbox;

  var style = JSON.parse(jsonStyle);

  editorSettings.theme = style.Theme;
  editorSettings.fontSize = style.FontSize;
  editorSettings.font = style.Font;
  editorSettings.lineHeight = style.LineHeight;

  te.initialize();

  te.setEditorStyle(style);

  setTimeout(te.keyBindings.setupKeyBindings, 800);

  return window.textEditor;
}

