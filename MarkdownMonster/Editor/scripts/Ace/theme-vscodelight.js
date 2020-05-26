define('ace/theme/vscodelight', ['require', 'exports', 'module' , 'ace/lib/dom'], function(require, exports, module) {


exports.isDark = false;
exports.cssClass = "ace-vslight";
exports.cssText = ".ace-vslight .ace_gutter {\
background: #f9f9f90;\
color: #222;\
}\
.ace-vslight .ace_support.ace_function {\
color: #800000;\
}\
.ace_underline {\
color: #a31515;\
cursor: pointer;\
}\
.ace-vslight .ace_print-margin {\
width: 1px;\
background: #e8e8e8;\
}\
.ace-vslight .ace_fold {\
background-color: #6B72E6;\
}\
.ace-vslight {\
background-color: #FCFCFC;\
color: #222;\
}\
.ace-vslight .ace_cursor {\
color: black;\
}\
.ace-vslight .ace_invisible {\
color: rgb(191, 191, 191);\
}\
.ace-vslight .ace_storage, .ace-vslight .ace_keyword {\
color: #0000ff;\
}\
.ace-vslight .ace_constant {\
color: rgb(197, 6, 11);\
}\
.ace-vslight .ace_constant.ace_buildin {\
color: orange;\
}\
.ace-vslight .ace_constant.ace_language {\
color: rgb(88, 92, 246);\
}\
.ace-vslight .ace_constant.ace_library {\
color: rgb(6, 150, 14);\
}\
.ace-vslight .ace_invalid {\
background-color: rgba(255, 0, 0, 0.1);\
color: red;\
}\
.ace-vslight .ace_support.ace_constant {\
color: rgb(6, 150, 14);\
}\
.ace-vslight .ace_support.ace_type, .ace-vslight .ace_support.ace_class {\
color: rgb(109, 121, 222);\
}\
.ace-vslight .ace_keyword.ace_operator {\
color: blue;\
}\
.ace-vslight .ace_string {\
color: maroon;\
}\
.ace-vslight .ace_comment {\
color: green;\
}\
.ace-vslight .ace_comment.ace_doc {\
color: rgb(0, 102, 255);\
}\
.ace-vslight .ace_comment.ace_doc.ace_tag {\
color: rgb(128, 159, 191);\
}\
.ace-vslight .ace_constant.ace_numeric {\
color: rgb(0, 0, 205);\
}\
.ace-vslight .ace_variable {\
color: rgb(49, 132, 149);\
}\
.ace-vslight .ace_xml-pe {\
color: rgb(104, 104, 91);\
}\
.ace-vslight .ace_entity.ace_name.ace_function {\
color: #0000A2;\
}\
.ace-vslight .ace_list {\
color: rgb(185, 6, 144);\
}\
.ace-vslight .ace_meta.ace_tag {\
color: #b70202;\
}\
.ace-vslight .ace_tag.ace_punctuation{\
color: blue;\
}\
.ace-vslight .ace_attribute-name {\
color: red;\
}\
.ace-vslight .ace_string.ace_regex {\
color: blue;\
}\
.ace-vslight .ace_marker-layer .ace_selection {\
background: rgb(181, 213, 255);\
}\
.ace-vslight.ace_multiselect .ace_selection.ace_start {\
box-shadow: 0 0 3px 0px white;\
border-radius: 2px;\
}\
.ace-vslight .ace_marker-layer .ace_step {\
background: rgb(252, 255, 0);\
}\
.ace-vslight .ace_marker-layer .ace_stack {\
background: rgb(164, 229, 101);\
}\
.ace-vslight .ace_marker-layer .ace_bracket {\
margin: -1px 0 0 -1px;\
border: 1px solid rgb(192, 192, 192);\
}\
.ace-vslight .ace_marker-layer .ace_active-line {\
background: rgba(0, 0, 0, 0.07);\
}\
.ace-vslight .ace_gutter-active-line {\
background-color: #eee;\
}\
.ace-vslight .ace_marker-layer .ace_selected-word {\
background: red;\
border: 1px solid rgb(200, 200, 250);\
}\
.ace-vslight .ace_heading {\
color: #800000;\
font-weight: bold;\
}\
.ace-vslight .ace_list {\
color: #000;\
}\
.ace-vslight .ace_markup.ace_list {\
color: #0451A5\
}\
.ace-vslight .ace_strong {\
font-weight: bold !important;\
}\
.ace-vslight .ace_markup.ace_strong,.ace-vslight .ace_string.ace_strong {\
color: #000080;\
}\
.ace-vslight .ace_emphasis {\
font-style: italic;\
}\
";

// .ace-vslight .ace_indent-guide {\
// background: url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAAE0lEQVQImWP4////f4bLly//BwAmVgd1/w11/gAAAABJRU5ErkJggg==\") right repeat-y;\

var dom = require("../lib/dom");
dom.importCssString(exports.cssText, exports.cssClass);
});
