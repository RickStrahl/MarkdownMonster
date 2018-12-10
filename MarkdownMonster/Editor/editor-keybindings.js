var te = window.textEditor;

te.keyBindings = {
  setupKeyBindings: function() {
    var kbJson = te.mm.textbox.GetKeyBindingsJson();
    var keyBindings = JSON.parse(kbJson);

    for (var i = 0; i < keyBindings.length; i++) {
      var kb = keyBindings[i];
      if (!kb.CommandName)
        continue;
      var handlerName = kb.CommandName[0].toLowerCase() + kb.CommandName.substr(1);
      var handler = eval("te.keyBindings." + handlerName);
      if (!handler)
        continue;

      //alert(kb.CommandName + ": " + kb.Key + " - " + handler + " " + typeof(handler));
      te.editor.commands.addCommand({
        name: kb.CommandName,
        bindKey: { win: kb.Key },
        exec: handler
      });
    }
  },
  saveDocument: function() {
    te.mm.textbox.IsDirty(); // force document to update
    te.keyboardCommand("SaveDocument");
  },
  newDocument: function() {
    te.keyboardCommand("NewDocument");
    // do nothing but:
    // keep ctrl-n browser behavior from happening
    // and let WPF handle the key
  },
  openDocument: function() {
    te.editor.blur(); // HACK: avoid letter o insertion into document IE bug
    te.keyboardCommand("OpenDocument");
    setTimeout(function() { te.editor.focus(); }, 20);
  },
  reloadEditor: function() {
    te.editor.blur(); // HACK: avoid letter o insertion into document IE bug
    te.keyboardCommand("ReloadEditor");
    setTimeout(function() { te.editor.focus(); }, 20);
  },
  showHelp: function() { te.keyboardCommand("ShowHelp") },

  insertBold: function() { te.keyboardCommand("InsertBold"); },
  insertItalic: function() { te.keyboardCommand("InsertItalic"); },

  insertHyperlink: function() { te.keyboardCommand("InsertHyperlink") },
  insertList: function() { te.keyboardCommand("InsertList") },
  insertEmoji: function() { te.keyboardCommand("InsertEmoji") },


  insertImage: function() { te.keyboardCommand("InsertImage"); },

  // find again redirect
  findNext: function() { te.editor.execCommand("findnext") },
  // embed code
  insertCodeblock: function() { te.keyboardCommand("InsertCodeblock"); },
  // inline code 
  insertInlineCode: function() { te.keyboardCommand("InsertInlineCode"); },

  deleteCurrentLine: te.deleteCurrentLine,

  // try to move between tabs
  nextTab: function() { te.keyboardCommand("NextTab"); },
  previousTab: function() { te.keyboardCommand("PreviousTab"); },

  // take over Zoom keys and manually zoom
  zoomEditorDown: function() {
    te.keyboardCommand("ZoomEditorDown");
    return null;
  },
  zoomEditorUp: function() {
    te.keyboardCommand("ZoomEditorUp");
    return null;
  },

  // Paste as Markdown/From Html
  copyMarkdownAsHtml: function() { te.keyboardCommand("CopyMarkdownAsHtml"); },
  pasteHtmlAsMarkdown: function() { te.keyboardCommand("PasteMarkdownAsHtml"); },

  // remove markdown formatting
  removeMarkdownFormatting: function() { te.keyboardCommand("RemoveMarkdownFormatting"); },

  // Capture paste operation in WPF to handle Images
  paste: function() {
    te.mm.textbox.PasteOperation();
    //setTimeout(function() { alert('test'); }, 1000);
  },
  paste2: function() {
    te.mm.textbox.PasteOperation();
    //setTimeout(function() { alert('test'); }, 1000);
  },
  nextSpellCheckError: function () {
    var pos = te.getCursorPosition();   
    var markers = te.editor.session.getMarkers(true);
    
    for(var key in markers) {         
      var range = markers[key].range;
      if (range.end.row > pos.row || range.end.row === pos.row && range.end.column > pos.column) {        
        te.setSelectionRange(range);
        return;
      }
    }

    if (pos.row + 30 > te.editor.session.getLength() - 1)
      return;

    var st = te.getScrollTop();
    console.log(st);
    te.setScrollTop(st + 500);


    setTimeout(function() {
      sc.spellCheck();
      setTimeout(te.keyBindings.nextSpellCheckError, 700);
    });  
  }
};
