define("ace/theme/vscodedark",["require","exports","module","ace/lib/dom"], function(require, exports, module) {

exports.isDark = true;
exports.cssClass = "ace-vscodedark";
exports.cssText = ".ace-vscodedark {\
background: #222;\
color: #EEEEEE;\
font-weight: normal;\
}\
.ace-vscodedark .ace_gutter {\
background: #191919;\
color: #E2E2E2\
}\
.ace-vscodedark .ace_print-margin {\
width: 1px;\
background: #333\
}\
.ace-vscodedark .ace_cursor {\
color: #A7A7A7\
}\
.ace-vscodedark .ace_marker-layer .ace_selection {\
background: #264f78;\
color: #eee;\
}\
.ace-vscodedark.ace_multiselect .ace_selection.ace_start {\
box-shadow: 0 0 3px 0px #141414;\
}\
.ace-vscodedark .ace_marker-layer .ace_step {\
background: rgb(102, 82, 0)\
}\
.ace-vscodedark .ace_marker-layer .ace_bracket .ace_paren {\
margin: -1px 0 0 -1px;\
border: 1px solid #888;\
}\
.ace-vscodedark .ace_marker-layer .ace_active-line {\
background: #303030;\
}\
.ace-vscodedark .ace_gutter-active-line {\
background-color: rgba(255, 255, 255, 0.031)\
}\
.ace-vscodedark .ace_marker-layer .ace_selected-word {\
border: 1px solid rgba(221, 240, 255, 0.20)\
}\
.ace_underline {\
color: #CEAA78;\
}\
.ace-vscodedark .ace_invisible {\
color: rgba(255, 255, 255, 0.25)\
}\
.ace-vscodedark .ace_meta {\
color: #CDA869\
}\
.ace-vscodedark .ace_constant,\
.ace-vscodedark .ace_constant.ace_character,\
.ace-vscodedark .ace_constant.ace_other,\
.ace-vscodedark .ace_heading,\
.ace-vscodedark .ace_markup.ace_heading,\
.ace-vscodedark .ace_support.ace_constant {\
color:  #4c84b3;;\
}\
.ace-vscodedark .ace_constant.ace_escape {\
color: goldenrod !important;\
}\
.ace-vscodedark .ace_constant.ace_language,\
.ace-vscodedark .ace_constant.ace_attribute\
{\
color: #57bebe;\
}\
.ace-vscodedark .ace_keyword {\
color: #569bd5;\
}\
.ace-vscodedark .ace_identifier {\
color: rgb(213, 231, 255);\
}\
.ace-vscodedark .ace_name {\
color: #dbdba9 !important;\
}\
.ace-vscodedark .ace_type {\
color: #45c8af !important;\
}\
.ace-vscodedark .ace_invalid.ace_illegal {\
color: #F8F8F8;\
background-color: rgba(86, 45, 86, 0.75)\
}\
.ace-vscodedark .ace_invalid.ace_deprecated {\
text-decoration: underline;\
font-style: italic;\
color: cornsilk;\
}\
.ace-vscodedark .ace_support {\
color: #9B859D\
}\
.ace-vscodedark .ace_fold {\
background-color: #AC885B;\
border-color: #F8F8F8\
}\
.ace-vscodedark .ace_support.ace_function,\
.ace-vscodedark .ace_function {\
color: lightskyblue;\
}\
.ace-vscodedark .ace_markup.ace_list,\
.ace-vscodedark .ace_storage {\
color: #6789bb;\
}\
.ace-vscodedark .ace_entity.ace_name.ace_function,\
.ace-vscodedark .ace_meta.ace_tag,\
.ace-vscodedark .ace_variable {\
color: #569bd5;\
}\
.ace-vscodedark .ace_string {\
color: #cd9071;\
}\
.ace-vscodedark .ace_string.ace_regexp {\
color: #E9C062\
}\
.ace-vscodedark .ace_comment {\
font-style: italic;\
color: #608a4e;\
}\
.ace-vscodedark .ace_variable,\
.ace-vscodedark .ace_attribute-name{\
color:#9bdbdf !important;\
}\
.ace-vscodedark .ace_numeric {\
color: #95cda7\
}\
.ace-vscodedark .ace_xml-pe {\
color: #494949\
}\
ace_line.ace_selected {\
background-color: #777 !important;\
}\
.ace-vscodedark .ace_heading {\
font-weight: bold;\
}\
.ace-vscodedark .ace_strong {\
color: #569bd5;\
font-weight: bold !important;\
}\
.ace_blockquote{\
color: #54aa4e !important;\
}\
.ace-vscodedark .ace_emphasis {\
font-style: italic;\
}\
.ace-vscodedark .ace_indent-guide {\
background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAAEklEQVQImWMQERFpYLC1tf0PAAgOAnPnhxyiAAAAAElFTkSuQmCC) right repeat-y\
}";

var dom = require("../lib/dom");
dom.importCssString(exports.cssText, exports.cssClass);
});
                (function() {
                    window.require(["ace/theme/vscodedark"], function(m) {
                        if (typeof module == "object" && typeof exports == "object" && module) {
                            module.exports = m;
                        }
                    });
                })();
            