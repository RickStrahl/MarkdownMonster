/// <reference path="scripts/Ace/ace.js" />
/// <reference path="scripts/Ace/ace.js" />
/// <reference path="editorSettings.js" />
/// <reference path="editor.js" />

// *** code has two global dependencies:
// te.editor = <editor_instance > ;
// editorSettings = {
// dictionary: "en_US"  // dictionary to use. Get dicts from OpenOffice site
// enableSpellChecking = true
// }

(function () {
  var te = null;

  window.sc = window.spellcheck = {
    interval: null,
    intervalTimeout: 200,
    firstpass: true,
    spellCheck: null,  // spellCheck function set in enable
    misspelled: misspelled,
    dictionary: null, // Typo instance
    markers: [],
    excludedWords:
      "body,html,xml,div,span,td,th,tr,thead,tbody,blockquote,src,href,ul,ol,li,png,gif,jpg,js,css,htm,html,topiclink,lang,img,&nbsp;,http,https," +
      "www,com,---,--",
    clearMarkers: function() {
        for (var i in sc.markers) {
            te.editor.session.removeMarker(sc.markers[i]);
        }
        sc.markers = [];
    },
    selectMarker: function (index) {
      var marker = sc.markers[index];
      if (!marker)
        return;
    },
    contentModified: true,
    contentModifiedChanged: function (e) {
      sc.contentModified = true;
    },
    disable: function () {
      te = window.textEditor;

      if (sc.interval) {
        clearInterval(sc.interval);
        sc.interval = null;
      }
      te.editor.session.off('change', sc.contentModifiedChanged);
      sc.clearMarkers();
      editorSettings.enableSpellChecking = false;
    },
    enable: function () {
      te = window.textEditor;

      editorSettings.enableSpellChecking = true;
      sc.spellCheck = spellCheck;
      te.spellcheck = sc;

      // You also need to load in typo.js and jquery.js
      // You should configure these classes.
      var dicData, affData;
      //var misspelledDict = [];

      sc.contentModified = true;
      var currentlySpellchecking = false;

      if (sc.firstpass) {
        // Make red underline for gutter and words.
        $("<style>.ace_marker-layer .misspelled { position: absolute; z-index: -2; border-bottom: 1px dashed red; margin-bottom: -1px; }</style>")
          .appendTo("head");
      }

      if (te.mm) //te.dic && te.aff) {
      {
        //dictionary = new Typo(lang, te.aff, te.dic);
        // use .net type checking
        enableSpellcheck();
        //spellCheck();
      } else {
        var lang = editorSettings.dictionary;
        var dicPath = lang + ".dic";
        var affPath = lang + ".aff";

        // Load the dictionary from url
        // Make sure MIME type exists for .dic and .aff
        // We have to load the dictionary files sequentially to ensure
        $.get(dicPath,
          function (data) {
            dicData = data;
          }).done(function () {
            $.get(affPath,
              function (data) {
                affData = data;
              }).done(function () {
                // editorSettings.dictionary holds language: "en_US" or "de_DE"
                sc.dictionary = new Typo(lang, affData, dicData);
                enableSpellcheck();
              });
          });
      }

      sc.firstpass = false;


      return;


      function enableSpellcheck() {
        spellCheck(true);

        if (!sc.interval) {
          sc.interval = setInterval(spellCheck, sc.intervalTimeout);

          // detect changes to content - don't spell check if nothing's changed
          te.editor.session.on('change', sc.contentModifiedChanged);
        }
      }


      // Spell check the Ace editor contents.
      function spellCheck(force) {
        if (!editorSettings.enableSpellChecking)
          return;
        if (currentlySpellchecking)
          return;
        if (!force && !sc.contentModified)
          return;

        sc.contentModified = false;
        currentlySpellchecking = true;
        spellcheckErrors = 0;
        var session = te.editor.getSession();

        var Range = ace.require('ace/range').Range;

        try {
          var lines = session.getDocument().getAllLines();
          var isCodeBlock = false;
          var isFrontMatter = false;

          var topRow = Math.ceil(te.editor.renderer.getFirstVisibleRow()) - 50;
          if (topRow < 0)
            topRow = 0;
          var bottomRow = Math.ceil(te.editor.renderer.getLastVisibleRow()) + 5;
          if (bottomRow > lines.length)
            bottomRow = lines.length;
          var lineCount = topRow;

          //console.log(topRow, bottomRow, lines.length);

          var curPos = te.getCursorPosition();

          //sc.clearMarkers();

          // save old markers and delete them at the end of processing
          // to avoid flicker.
          var oldMarkers = sc.markers;
          sc.markers = [];

          for (var i = topRow; i < bottomRow; i++) {
            var line = i;

            lineCount++;

            // setTimeout to free up processor in between lines
            setTimeout(function (line, isLast) {
              var lineText = lines[line];

              // skip embedded images
              if(lineText.indexOf("]: data:image/") > -1)
                return;

              var trimText = lineText.trim();

              // end front matter check
              if (isFrontMatter && (trimText == "---" || trimText == "..."))
                isFrontMatter = false;
              // start front matter check
              if (line == 0 && trimText == "---")
                isFrontMatter = true;


              if (lineText && lineText.length > 2 && lineText.substr(0, 3) === "```") {

                if (lineText.trim().length > 3)
                  isCodeBlock = true;
                else {
                  isCodeBlock = false;
                  return;
                }

              }
              if (!isCodeBlock && !isFrontMatter) {

                // skip active word we're typing right now 
                var skipWord = null;
                if (curPos.row === line) {
                  skipWord = te.editor.session.getTextRange(
                      te.editor.session.getWordRange(curPos.row, curPos.column));
                }


                // Check spelling of this line.
                var misspellings = misspelled(lineText, skipWord);

                //// Add markers and gutter markings.
                //if (misspellings.length > 0) {
                //    session.addGutterDecoration(i, "misspelled");
                //}
                for (var j in misspellings) {
                  j = j * 1;
                  var range = new Range(line * 1,
                    misspellings[j][0],
                    line * 1,
                    misspellings[j][1]);
                  var marker =
                    session.addMarker(range, "misspelled", "mm", true);
                  var word = misspellings[j][2];

                  // attach a misspelled property so we have state
                  range.misspelled = word;

                  sc.markers[sc.markers.length] = marker;
                  spellcheckErrors++;
                }
              }
              if (isLast) {
                currentlySpellchecking = false;
                  sc.contentModified = false;

                  // Clear old Markers now that the new markers are in place
                  // to avoid flicker
                  for (var i in oldMarkers) {
                      te.editor.session.removeMarker(oldMarkers[i]);
                  }

                  if (spellcheckErrors > editorSettings.spellcheckerErrorLimit) {
                    // disable both in editor and MM
                    te.mm.textbox.SetSpellChecking(true);
                    te.enablespellchecking(true);
                  }
              }
            }.bind(this, line, lineCount >= bottomRow - 1),
              40);
          }
        } finally {
        }
      }

    }
  }

  // Check the spelling of a line, and return [start, end]-pairs for misspelled words.
  // skipWord - a word to skip translating most likely because we're on it
  function misspelled(line, skipWord) {
    if (!line)
      return [];

    // replace inline code blocks with 9's so it isn't parsed
    var matches = line.match(/`.*?`|href=".*?"|src=".*?"/g);
    var match = null;
    if (matches) {
      for (var i = 0; i < matches.length; i++) {
        match = matches[i];
        line = line.replace(match, new Array(match.length + 1).join("9")); // repeat
      }
    }

    // ignore links
    matches = line.match(/\]\(.*?\)/g);
    if (matches) {
      for (var i = 0; i < matches.length; i++) {
        match = matches[i];
        line = line.replace(match, " " + new Array(match.length).join("9")); // repeat
      }
    }

    // ignore raw links
    matches = line.match(/(http|https):\/\/.*?(\s|\,|\/\.\s|$)/g);
    if (matches) {
      for (var i = 0; i < matches.length; i++) {

        match = matches[i].trim();
        line = line.replace(match, new Array(match.length).join("9")+ " "); // repeat
      }
    }

    // Fix up 'special apostrophies
    line = line.replace(/[´,’]/, "'");

    // split line by word boundaries - any non alpha-numeric characters plus ' (\u0027) and white space
    //var words = line.split(/[^a-zA-Z0-9\u00C0-\u02AF']|\s/);
    //var words = line.Split(/[\P{L}]/);  // not supported in IE/JavaScript

    // split line by word boundaries and non-alpha numeric chars by unicode range. ' (\u0027) is handled special
    // Exclusion list: Include a-zA-Z0-9 - everything
    var words = line.split(/[\u0000-\u0026\u0028-\u002F\u003A-\u0040\u005B-\u0060\u007B-\u00BF\u02B9-\u0385\u1ffd-\u2c66]/);
    
      
    var i = 0;
    var bads = [];
    for (var wordIndex in words) {
      var word = words[wordIndex] + "";

      if (!word) {
        i += 1;
        continue;
      }

        if (word && (word === skipWord || word.indexOf("--") > -1)) {
          i += word.length + 1;
          continue;
      }

      var beginCharoffset = 0; // if we strip characters adjust the offsets
      var endCharoffset = 0;

      // only use words without special characters
      if (word.length > 1 &&
        sc.excludedWords.indexOf("," + word + ",") == -1) {

        if (word[0] === "'") {
          word = word.substr(1);
          beginCharoffset++;
        }
        if (word[word.length - 1] === "'") {
          word = word.substr(0, word.length - 1);
          endCharoffset++;
        }

        var isOk = te.checkSpelling(word);
        if (!isOk) {
          bads[bads.length] = [i + beginCharoffset, i + words[wordIndex].length - endCharoffset, word];
        }
      }
      i += word.length + 1;
    }
    return bads;
  }
})();
