/// <reference path="Ace/ace.js" />
/// <reference path="editorSettings.js" />
/// <reference path="editor.js" />

// *** code has two global dependencies:
// te.editor = <editor_instance > ;
// editorSettings = {
// dictionary: "en_US"  // dictionary to use. Get dicts from OpenOffice site
// enableSpellChecking = true
// }

var sc = window.spellcheck = {
    interval: null,
    firstpass: true,
    spellCheck: function() {},
    dictionary: null, // Typo instance
    markers: [],
    excludedWords: ",div,span,td,th,tr,blockquote,src,href,ul,ol,li,png,gif,jpg,js,css,htm,html,topiclink,lang,img,&nbsp;,http,https,---,--,----",
    clearMarkers: function() {
        for (var i in sc.markers) {
            te.editor.session.removeMarker(sc.markers[i]);
        }
        sc.markers_present = [];
    },
    contentModified: true,
    contentModifiedChanged: function(e) {        
        sc.contentModified = true;        
    },
    disable: function() {
        if (sc.interval) {
            clearInterval(sc.interval);
            sc.interval = null;
        }
        te.editor.session.off('change', sc.contentModifiedChanged);
        sc.clearMarkers();
        editorSettings.enableSpellChecking = false;
    },
    enable: function () {
        editorSettings.enableSpellChecking = true;
        sc.spellCheck = spellCheck;
        
        // You also need to load in typo.js and jquery.js
        // You should configure these classes.                        
        var dicData, affData;
        var misspelledDict = [];
        var intervalTimeout = 1800;

        sc.contentModified = true;
        var currentlySpellchecking = false;

        var typoLastAccess = new Date().getSeconds();

        if (sc.firstpass) {
            // Make red underline for gutter and words.
			$("<style type='text/css'>.ace_marker-layer .misspelled { position: absolute; z-index: -2; border-bottom: 1px dashed red; margin-bottom: -1px; }</script>")
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
            $.get(dicPath, function(data) {
                dicData = data;
            }).done(function() {
                $.get(affPath, function(data) {
                    affData = data;
                }).done(function() {
                    // editorSettings.dictionary holds language: "en_US" or "de_DE"
                    sc.dictionary = new Typo(lang, affData, dicData);
                    enableSpellcheck();              
                });
            });
        }

        sc.firstpass = false;


        // handle clicking on a mispelled word
        //te.editor.on('dblclick', showSuggestions);
        var ctxNoShow = false;
        $(document).on("contextmenu", function (e) {
            if (ctxNoShow)
                e.preventDefault();
        });
        te.editor.on('mousedown', function (e) {
            if (e.domEvent.which == 3) {                
                showSuggestions(e);
                // allow context rightclick after a second
                setTimeout(function () { ctxNoShow = false; }, 1000);
            }
        });
	    $("#spellfixes") // handle the click on the selected item
			.on("click", "div", clickSuggestion);

	    //var keydownFunc = function(e) {
		   // alert('keydown'); //e.key + " " + e.keyCode);
		   // if (e.key == ']') {
			  //  var $misspelled = $(e.target).find(".misspelled");
			  //  //if ($misspelled.length < 1) return;
			  //  alert($misspelled.length + " " + $(e.target).html());
			  //  showSuggestions(e);
			  //  //alert($misspelled.html());
			  //  e.preventDefault();
		   // }
	    //};	
			
		// can't access the marker
	    //$(document).on("click", ".misspelled", function() { alert('here we go'); });
		
        return;


        function enableSpellcheck() {            
            spellCheck(true);
            
            if (!sc.interval) {
                sc.interval = setInterval(spellCheck, intervalTimeout);  
            
                // detect changes to content - don't spell check if nothing's changed
                te.editor.session.on('change', sc.contentModifiedChanged);
            }            
        }

        // Check the spelling of a line, and return [start, end]-pairs for misspelled words.
        function misspelled(line) {
            // split lineby word boundaries
            var words = line.split(/[\s,(,),\[,\],<,>,:,",=,?,!,.,;,\,|]/g);
            var i = 0;
            var bads = [];
            for (var wordIndex in words) {
                var word = words[wordIndex] + "";

                // only use words without special characters
                if (word.length > 1 &&                                     
                    sc.excludedWords.indexOf("," + word + ",") == -1 &&
                    !word.match(/[_,-,(,),\[,\],:,*,&,/,\\,~,#,@,%,{,},`,0-9]/g)) {                    
                    
                    if ( word[0] == "'" && word[word.length-1] == "'" )
                       word =  word.substr(1,word.length-2);
                    
                    var isOk = te.checkSpelling(word);
                    if (!isOk)
                        bads[bads.length] = [i, i + words[wordIndex].length, word];
                }
                i += words[wordIndex].length + 1;
            }
            return bads;
        }
        
        // Spell check the Ace editor contents.
        function spellCheck(force) {            
            if (!editorSettings.enableSpellChecking)
                return;
            if (currentlySpellchecking) 
                return;            
            if (!force && !sc.contentModified) 
                return;
            
            currentlySpellchecking = true;
            var session = te.editor.getSession();

            sc.clearMarkers();

            try {
                var Range = ace.require('ace/range').Range;

                var lines = session.getDocument().getAllLines();
                
                
                var isCodeBlock = false;
                var isFrontMatter = false;

                var topRow = Math.ceil(te.editor.renderer.getFirstVisibleRow()) - 30;
                if (topRow < 0)
                    topRow = 0;                    
                var bottomRow = Math.ceil(te.editor.renderer.getLastVisibleRow()) + 5;
                if (bottomRow > lines.length)
                    bottomRow = lines.length;
                var lineCount = topRow;

                //console.log(topRow, bottomRow, lines.length);
                
                for (var i = topRow; i < bottomRow; i++) {

                    var line = i;                    

                //for (var line in lines) {
                    // Clear the gutter.
                    //session.removeGutterDecoration(i, "misspelled");
                    lineCount++;                    

                    // setTimeout to free up processor in between lines
                    setTimeout(function (line, isLast) {
                        var lineText = lines[line];
                        
						if (isFrontMatter && (lineText == "---" || lineText == "...")) 
                            isFrontMatter = false;                                                                                                                               
						if (line == 0 && (lineText == "---" || lineText == "...")) 
                            isFrontMatter = true;                            


                        if (lineText && lineText.length > 2 && lineText.substr(0, 3) === "```") {
                            
                            if (lineText.trim().length > 3)
                                isCodeBlock = true;
                            else
                                isCodeBlock = false;
                        
                        }
                        //isCodeBlock = !isCodeBlock;                            
                        
                        if (!isCodeBlock && !isFrontMatter) {

                            // Check spelling of this line.
                            var misspellings = misspelled(lineText);

                            //// Add markers and gutter markings.
                            //if (misspellings.length > 0) {
                            //    session.addGutterDecoration(i, "misspelled");
                            //}
                            for (var j in misspellings) {
                                j = j * 1;
                                var range = new Range(line * 1, misspellings[j][0], line * 1, misspellings[j][1]);
                                var marker =
                                    session.addMarker(range, "misspelled", "typo", true);
                                var word = misspellings[j][2];
                                range.misspelled = word;
								sc.markers[sc.markers.length] = marker;
                            }
                        }
                        if (isLast) {
                            currentlySpellchecking = false;
                            sc.contentModified = false;                            
                        }
                    }.bind(this, line, lineCount >= bottomRow -1), 40);
                }
            } finally {            
            }
        }


        function showSuggestions(e) {
            var $list = $("#spellfixes");

            // avoid multiple popups - in ActiveX control for 
            // some reason it fires twice
            if ($list.is(":visible"))
                return;

            var markers = te.editor.session.getMarkers(true);
            if (!markers || markers.length == 0)
                return;

            var pos = e.getDocumentPosition();
            var matched = null;

            // look for a misspelled marker  that matches our
            // current document location
            for (var id in markers) {
                var marker = markers[id];
                if (marker.clazz != "misspelled")
                    continue;

                if (pos.row >= marker.range.start.row && pos.row <= marker.range.end.row &&
                    pos.column >= marker.range.start.column && pos.column <= marker.range.end.column) {
                    matched = marker;
                }
            };

            if (!matched)
                return;

            // keep context menu from popping up
            e.preventDefault();
            ctxNoShow = true;

            // pick the mispelled word out of the attached range value
            var misspelledWord = matched.range.misspelled;


            //var sug = dictionary.suggest(misspelledWord, 10);
            var sug = te.suggestSpelling(misspelledWord, 8);
            if (!sug || sug.length < 1)
                return;

            var $item;
            for (var i = 1; i <= sug.length; i++) {
                $item = $("<div>").text(sug[i-1]);
                $item.data("range", matched.range);
                $list.append($item);
            }           
            $item = $("<div class='separator'></div>" +
                      "<div style='text-align:right'>+ add</div>");
            $list.append($item);

            // No good way to close via other events so add a button to close
            $item = $("<div style='text-align:right'>x close</div>");
            $list.append($item);

	        $list
		        .data("misspelled", misspelledWord)
		        .show()
		        .css({ left: e.clientX, top: e.clientY });
    //            .mouseleave(function(e) {
    //                $list.hide();
    //                $list.html("");
				//});

			   // clicking anywhere will close the popup
	           $(document).bind("click.suggestionsdoc", hideSuggestions);
            
                setTimeout(function () {
                    if (e.clientY + $list.height() > document.documentElement.clientHeight)
                        $list.css({ top: e.clientY - $list.height() });
                    if (e.clientX + $list.width() > document.documentElement.clientWidth)
                        $list.css({ left: e.clientX - $list.width() });
                }, 1);
        }

		function hideSuggestions() {
			var $list = $("#spellfixes");
			$list.hide();
			$list.html("");
			$(document).unbind("click.suggestionsdoc");			
		}

        // <div id="spellfixes"> 
        //    <div>word</div>
        //    <div>word2</div>
        // </div>
        function clickSuggestion() {
            var $el = $(this);
            var $list = $("#spellfixes");

            var text = $el.text();
            
            if (text && text.trim() == "+ add") {
                var word = $list.data("misspelled");
                te.addWordSpelling(word);
                sc.contentModified = true;
            }
            else if (text && text.trim() != "x close") {
                var range = $el.data("range");
                te.editor.getSession().replace(range, text);
            }

	        hideSuggestions();            
        }
    }
}
