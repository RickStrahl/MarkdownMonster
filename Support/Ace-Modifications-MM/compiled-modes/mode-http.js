define("ace/mode/http_highlight_rules",["require","exports","module","ace/lib/oop","ace/lib/lang","ace/mode/text_highlight_rules"], function(require, exports, module) {
    "use strict";
    
    var oop = require("../lib/oop");
    var lang = require("../lib/lang");
    var TextHighlightRules = require("./text_highlight_rules").TextHighlightRules;
    
    function safeCreateRegexp(source, flag) {
        try {
            return new RegExp(source, flag);
        } catch(e) {}
    }
    
    
    var httpHighlightRules = function() {
        this.$rules = {
            "start" : [               
                {
                    tokenNames : ["httpresults.constant.numeric", "httpresults.text", "httpresults.text", "httpresults.keyword",],
                    regex : /(^\s+[0-9]+)(:)(\d*\s?)([^\r\n]+)/,
                    onMatch : function(val, state, stack) {
                            var values = this.splitRegex.exec(val);
                        
                        var types = this.tokenNames;
                        var tokens = [{
                            type: types[0],
                            value: values[1]
                        }, {
                            type: types[1],
                            value: values[2]
                        }];
                        
                        if (values[3]) {
                            if (values[3] == " ")
                                tokens[1] = { type: types[1], value: values[2] + " " };
                            else
                                tokens.push({ type: types[1], value: values[3] });
                        }
                        var regex = stack[1];
                        var str = values[4];
                        
                        var m;
                        var last = 0;
                        if (regex && regex.exec) {
                            regex.lastIndex = 0;
                            while (m = regex.exec(str)) {
                                var skipped = str.substring(last, m.index);
                                last = regex.lastIndex;
                                if (skipped)
                                    tokens.push({type: types[2], value: skipped});
                                if (m[0])
                                    tokens.push({type: types[3], value: m[0]});
                                else if (!skipped)
                                    break;
                            }
                        }
                        if (last < str.length)
                            tokens.push({type: types[2], value: str.substr(last)});
                        return tokens;
                    }
                },
                {
                    regex : "^Searching for [^\\r\\n]*$",
                    onMatch: function(val, state, stack) {
                        var parts = val.split("\x01");
                        if (parts.length < 3)
                            return "text";
    
                        var options, search;
                        
                        var i = 0;
                        var tokens = [{
                            value: parts[i++] + "'",
                            type: "text"
                        }, {
                            value: search = parts[i++],
                            type: "text" // "httpresults.keyword"
                        }, {
                            value: "'" + parts[i++],
                            type: "text"
                        }];
                        if (parts[2] !== " in") {
                            tokens.push({
                                value: "'" + parts[i++] + "'",
                                type: "text"
                            }, {
                                value: parts[i++],
                                type: "text"
                            });
                        }
                        tokens.push({
                            value: " " + parts[i++] + " ",
                            type: "text"
                        });
                        if (parts[i+1]) {
                            options = parts[i+1];
                            tokens.push({
                                value: "(" + parts[i+1] + ")",
                                type: "text"
                            });
                            i += 1;
                        } else {
                            i -= 1;
                        }
                        while (i++ < parts.length) {
                            parts[i] && tokens.push({
                                value: parts[i],
                                type: "text"
                            });
                        }
                        
                        if (search) {
                            if (!/regex/.test(options))
                                search = lang.escapeRegExp(search);
                            if (/whole/.test(options))
                                search = "\\b" + search + "\\b";
                        }
                        
                        var regex = search && safeCreateRegexp(
                            "(" + search + ")",
                            / sensitive/.test(options) ? "g" : "ig"
                        );
                        if (regex) {
                            stack[0] = state;
                            stack[1] = regex;
                        }
                        
                        return tokens;
                    }
                },
                {
                    regex : "^(?=Found \\d+ matches)",
                    token : "text",
                    next : "numbers"
                },
                {
                    token : "string", // single line
                    regex : "^\\S:?[^:]+",
                    next : "numbers"
                }
            ],
            numbers:[{
                    regex : "\\d+",
                    token : "constant.numeric"
                }, {
                    regex : "$",
                    token : "text",
                    next : "start"
                },
                {
                    regex : "application/json|text/xml|multipart/mixed|application/x-www-form-urlencoded|multipart/form-data|text/html" +
                    "|Bearer|if-modified|no-cache|bytes|chunked|keep-alive",                    
                    token : "constant.keyword",                   
                },
                {
                    regex : "gzip|deflate|charset|utf-8",                    
                    token : "constant.numeric",                   
                }    
            ],            
        };
        this.normalizeRules();
    };
    
    oop.inherits(httpHighlightRules, TextHighlightRules);
    
    exports.httpHighlightRules = httpHighlightRules;
    
    });
    
    define("ace/mode/matching_brace_outdent",["require","exports","module","ace/range"], function(require, exports, module) {
    "use strict";
    
    var Range = require("../range").Range;
    
    var MatchingBraceOutdent = function() {};
    
    (function() {
    
        this.checkOutdent = function(line, input) {
            if (! /^\s+$/.test(line))
                return false;
    
            return /^\s*\}/.test(input);
        };
    
        this.autoOutdent = function(doc, row) {
            var line = doc.getLine(row);
            var match = line.match(/^(\s*\})/);
    
            if (!match) return 0;
    
            var column = match[1].length;
            var openBracePos = doc.findMatchingBracket({row: row, column: column});
    
            if (!openBracePos || openBracePos.row == row) return 0;
    
            var indent = this.$getIndent(doc.getLine(openBracePos.row));
            doc.replace(new Range(row, 0, row, column-1), indent);
        };
    
        this.$getIndent = function(line) {
            return line.match(/^\s*/)[0];
        };
    
    }).call(MatchingBraceOutdent.prototype);
    
    exports.MatchingBraceOutdent = MatchingBraceOutdent;
    });
    
    
    define("ace/mode/http",["require","exports","module","ace/lib/oop","ace/mode/text","ace/mode/http_highlight_rules","ace/mode/matching_brace_outdent"], function(require, exports, module) {
    "use strict";
    
    var oop = require("../lib/oop");
    var TextMode = require("./text").Mode;
    var httpHighlightRules = require("./http_highlight_rules").httpHighlightRules;
    var MatchingBraceOutdent = require("./matching_brace_outdent").MatchingBraceOutdent;
        
    var Mode = function() {
        this.HighlightRules = httpHighlightRules;
        this.$outdent = new MatchingBraceOutdent();        
    };
    oop.inherits(Mode, TextMode);
    
    (function() {
        
        this.getNextLineIndent = function(state, line, tab) {
            var indent = this.$getIndent(line);
            return indent;
        };
    
        this.checkOutdent = function(state, line, input) {
            return this.$outdent.checkOutdent(line, input);
        };
    
        this.autoOutdent = function(state, doc, row) {
            this.$outdent.autoOutdent(doc, row);
        };
    
        this.$id = "ace/mode/http";
    }).call(Mode.prototype);
    
    exports.Mode = Mode;
    
    });                (function() {
                        window.require(["ace/mode/http"], function(m) {
                            if (typeof module == "object" && typeof exports == "object" && module) {
                                module.exports = m;
                            }
                        });
                    })();
                