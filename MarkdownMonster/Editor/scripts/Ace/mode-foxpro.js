/* ***** BEGIN LICENSE BLOCK *****
 * Distributed under the BSD license:
 *
 * Copyright (c) 2012, Ajax.org B.V.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Ajax.org B.V. nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL AJAX.ORG B.V. BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 *
 * Contributor(s):
 * Rick Strahl, West Wind Technologies (west-wind.com)
 *
 *
 * ***** END LICENSE BLOCK ***** */

define('ace/mode/foxpro', ['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/text', 'ace/tokenizer', 'ace/mode/foxpro_highlight_rules'], function (require, exports, module) {


    var oop = require("../lib/oop");
    var TextMode = require("./text").Mode;
    var Tokenizer = require("../tokenizer").Tokenizer;
    var FoxProHighlightRules = require("./foxpro_highlight_rules").FoxProHighlightRules;

    var Mode = function () {
        this.HighlightRules = FoxProHighlightRules;
    };
    oop.inherits(Mode, TextMode);

    (function () {

        this.lineCommentStart = ["*", "&&"];

        this.$id = "ace/mode/foxpro";
    }).call(Mode.prototype);

    exports.Mode = Mode;
});


define('ace/mode/foxpro_highlight_rules', ['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/text_highlight_rules'], function (require, exports, module)
{


var oop = require("../lib/oop");
var TextHighlightRules = require("./text_highlight_rules").TextHighlightRules;

var FoxProHighlightRules = function() {

    var functions = (
        "Abs|Aclass|Acopy|Acos|Adatabases|Adbobjects|Addbs|Addproperty|Adel|Adir|Adlls|Adockstate|Aelement|Aerror|Aevents|Afields|Afont|Agetclass|Agetfileversion|Ains|Ainstance|Alanguage|Alen|Alias|Alines|Alltrim|Amembers|Amouseobj|Anetresources|Aprinters|Aprocinfo|Asc|Ascan|Aselobj|Asessions|Asin|Asort|Astackinfo|Asubscript|At|At_c|Ataginfo|Atan|Atc|Atcc|Atcline|Atline|Atn2|Aused|Avcxclasses" +
        "Bar|Barcount|Barprompt|Between|Bindevent|Bintoc|Bitand|Bitclear|Bitlshift|Bitnot|Bitor|Bitrshift|Bitset|Bittest|Bitxor|Bof|" +
        "Candidate|Capslock|Cdow|Cdx|Ceiling|Chr|Chrsaw|Chrtran|Chrtranc|Cmonth|Cntbar|Cntpad|Col|Com|Comarray|Comclassinfo|Compobj|Comprop|Comreturnerror|Cos|Cpconvert|Cpcurrent|Cpdbf|Createbinary|Createobject|Createobjectex|Createoffline|Ctobin|Ctod|Ctot|Curdir|Cursorgetprop|Cursorsetprop|Cursortoxml|Curval" +
        "Date|Datetime|Day|Dbc|Dbf|Dbgetprop|Dbsetprop|Dbused|Ddeaborttrans|Ddeadvise|Ddeenabled|Ddeexecute|Ddeinitiate|Ddelasterror|Ddepoke|Dderequest|Ddesetoption|Ddesetservice|Ddesettopic|Ddeterminate|Defaultext|Deleted|Descending|Difference|Directory|Diskspace|Displaypath|Dmy|Dodefault|Dow|Drivetype|Dropoffline|Dtoc|Dtor|Dtos|Dtot" +
        "Editsource|Empty|Eof|Error|Evaluate|Eventhandler|Evl|Execscript|Exp|" +
        "Fchsize|Fclose|Fcount|Fcreate|Fdate|Feof|Ferror|Fflush|Fgets|Field|File|Filetostr|Filter|Fklabel|Fkmax|Fldlist|Flock|Floor|Fontmetric|Fopen|For|Forceext|Forcepath|Found|Fputs|Fread|Fseek|Fsize|Ftime|Fullpath|Function|Command|Fv|Fwrite|" +
        "Getbar|Getcolor|Getcp|Getdir|Getenv|Getfile|Getfldstate|Getfont|Getinterface|Getnextmodified|Getobject|Getpad|Getpem|Getpict|Getprinter|Getwordcount|Getwordnum|Getcursoradapter|Gomonth|" +
        "Header|Home|Hour" +
        "Idxcollate|Iif|Imestatus|Indbc|Indexseek|Inkey|Inlist|Inputbox|Insmode|Int|Isalpha|Isblank|Iscolor|Isdigit|Isexclusive|Isflocked|Isleadbyte|Islower|Ismouse|Isnull|Isreadonly|Isrlocked|Isupper|" +
        "Justdrive|Justext|Justfname|Justpath|Juststem|" +
        "Key|Keymatch|" +
        "Lastkey|Left|Leftc|Len|Lenc|Like|Likec|Lineno|Loadpicture|Locfile|Lock|Log|Log10|Lookup|Lower|Ltrim|Lupdate|" +
        "Max|Mcol|Mdown|Mdx|Mdy|Memlines|Memory|Menu|Message|Messagebox|Min|Minute|Mline|Mod|Month|Mrkbar|Mrkpad|Mrow|Mton|Mwindow|" +
        "Ndx|Newobject|Normalize|Ntom|Numlock|Nvl|" +
        "Objnum|Objtoclient|Objvar|Occurs|Oemtoansi|Oldval|On|Order|Os|" +
        "Pad|Padl|Padr|Padc|Parameters|Payment|Pcol|Pcount|Pemstatus|Pi|Popup|Primary|Printstatus|Prmbar|Prmpad|Program|Prompt|Proper|Prow|Prtinfo|Putfile|Pv" +
        "Quarter|" +
        "Raiseevent|Rand|Rat|Ratc|Ratline|Rdlevel|Readkey|Reccount|Recno|Recsize|Refresh|Relation|Replicate|Requery|Rgb|Rgbscheme|Right|Rightc|Rlock|Round|Row|Rtod|Rtrim|" +
        "Savepicture|Scheme|Scols|Sec|Seconds|Seek|Select|Set|Setfldstate|Sign|Sin|Skpbar|Skppad|Soundex|Space|Sqlcancel|Sqlcolumns|Sqlcommit|Sqlconnect|Sqldisconnect|Sqlexec|Sqlgetprop|Sqlmoreresults|Sqlprepare|Sqlrollback|Sqlsetprop|Sqlstringconnect|Sqltables|Sqrt|Srows|Str|Strconv|Strextract|Strtofile|Strtran|Stuff|Stuffc|Substr|Substrc|Syss|Overview|Sysmetric|" +
        "Tablerevert|Tableupdate|Tag|Tagcount|Tagno|Tan|Target|Textmerge|Time|Transform|Trim|Ttoc|Ttod|Txnlevel|Txtwidth|Type|" +
        "Unbindevents|Unique|Updated|Upper|Used|" +
        "Val|Varread|Vartype|Version|" +
        "Wborder|Wchild|Wcols|Wdockable|Week|Wexist|Wfont|Wlast|Wlcol|Wlrow|Wmaximum|Wminimum|Wontop|Woutput|Wparent|Wread|Wrows|Wtitle|Wvisible|" +
        "Xmltocursor|Xmlupdategram|" +
        "Year_Alignment|_Asciicols|_Asciirows|_Assist|_Beautify|_Box|_Browser|_Builder|_Calcmem|_Calcvalue|_Cliptext|_Converter|_Coverage|_Coverage|_Curobj|_Dblclick|_Diarydate|_Dos|_Foxdoc|_Foxgraph|_Gallery|_Gengraph|_Genhtml|_Genmenu|_Genpd|_Genscrn|_Genxtab|_Getexpr|_Include|_Indent|_Lmargin|_Mac|_Mbr_appnd|_Mbr_cpart|_Mbr_delet|_Mbr_font|_Mbr_goto|_Mbr_grid|_Mbr_link|_Mbr_mode|_Mbr_mvfld|_Mbr_mvprt|_Mbr_seek|_Mbr_sp100|_Mbr_sp200|_Mbr_szfld|_Mbrowse|_Mda_appnd|_Mda_avg|_Mda_brow|_Mda_calc|_Mda_copy|_Mda_count|_Mda_label|_Mda_pack|_Mda_reprt|_Mda_rindx|_Mda_setup|_Mda_sort|_Mda_sp100|_Mda_sp200|_Mda_sp300|_Mda_sum|_Mda_total|_Mdata|_Mdiary|_Med_clear|_Med_copy|_Med_cut|_Med_cvtst|_Med_find|_Med_finda|_Med_goto|_Med_insob|_Med_link|_Med_obj|_Med_paste|_Med_pref|_Med_pstlk|_Med_redo|_Med_repl|_Med_repla|_Med_slcta|_Med_sp100|_Med_sp200|_Med_sp300|_Med_sp400|_Med_sp500|_Med_undo|_Medit|_Mfi_clall|_Mfi_close|_Mfi_export|_Mfi_import|_Mfi_new|_Mfi_open|_Mfi_pgset|_Mfi_prevu|_Mfi_print|_Mfi_quit|_Mfi_revrt|_Mfi_savas|_Mfi_save|_Mfi_send|_Mfi_setup|_Mfi_sp100|_Mfi_sp200|_Mfi_sp300|_Mfi_sp400|_Mfile|_Mfiler|_Mfirst|_Mlabel|_Mlast|_Mline|_Mmacro|_Mmbldr|_Mpr_beaut|_Mpr_cancl|_Mpr_compl|_Mpr_do|_Mpr_docum|_Mpr_formwz|_Mpr_gener|_Mpr_graph|_Mpr_resum|_Mpr_sp100|_Mpr_sp200|_Mpr_sp300|_Mpr_suspend|_Mprog|_Mproj|_Mrc_appnd|_Mrc_chnge|_Mrc_cont|_Mrc_delet|_Mrc_goto|_Mrc_locat|_Mrc_recal|_Mrc_repl|_Mrc_seek|_Mrc_sp100|_Mrc_sp200|_Mrecord|_Mreport|_Mrqbe|_Mscreen|_Msm_data|_Msm_edit|_Msm_file|_Msm_format|_Msm_prog|_Msm_recrd|_Msm_systm|_Msm_text|_Msm_tools|_Msm_view|_Msm_windo|_Mst_about|_Mst_ascii|_Mst_calcu|_Mst_captr|_Mst_dbase|_Mst_diary|_Mst_filer|_Mst_help|_Mst_hphow|_Mst_hpsch|_Mst_macro|_Mst_office|_Mst_puzzl|_Mst_sp100|_Mst_sp200|_Mst_sp300|_Mst_specl|_Msysmenu|_Msystem|_Mtable|_Mtb_appnd|_Mtb_cpart|_Mtb_delet|_Mtb_delrc|_Mtb_goto|_Mtb_link|_Mtb_mvfld|_Mtb_mvprt|_Mtb_props|_Mtb_recal|_Mtb_sp100|_Mtb_sp200|_Mtb_sp300|_Mtb_sp400|_Mtb_szfld|_Mwi_arran|_Mwi_clear|_Mwi_cmd|_Mwi_color|_Mwi_debug|_Mwi_hide|_Mwi_hidea|_Mwi_min|_Mwi_move|_Mwi_rotat|_Mwi_showa|_Mwi_size|_Mwi_sp100|_Mwi_sp200|_Mwi_toolb|_Mwi_trace|_Mwi_view|_Mwi_zoom|_Mwindow|_Mwizards|_Mwz_all|_Mwz_form|_Mwz_foxdoc|_Mwz_import|_Mwz_label|_Mwz_mail|_Mwz_pivot|_Mwz_query|_Mwz_reprt|_Mwz_setup|_Mwz_table|_Mwz_upsizing|_Netware|_Oracle|_Padvance|_Pageno|_Pbpage|_Pcolno|_Pcopies|_Pdparms|_Pdriver|_Pdsetup|_Pecode|_Peject|_Pepage|_Pform|_Plength|_Plineno|_Ploffset|_Ppitch|_Pquality|_Pretext|_Pscode|_Pspacing|_Pwait|_Rmargin|_Runactivedoc|_Samples|_Screen|_Shell|_Spellchk|_Sqlserver|_Startup|_Tabs|_Tally|_Text|_Throttle|_Transport|_Triggerlevel|_Unix|_WebDevOnly|_WebMenu|_WebMsftHomePage|_WebVFPHomePage|_WebVfpOnlineSupport|_Windows|_Wizard|_Wrap|_scctext|_vfp" 
    );

    var constants = (
       "\\.t\\.|\\.f\\.|\\.null\\.|null"
    );

    var keywords = (        
        "By|" +
        "Case|Catch|" +
        "Define|Do|" +
        "Else|Endcase|Enddefine|Enddo|Endfor|Endfunc|Endif|Endprintjob|Endproc|Endscan|Endtext|Endtry|Endwith|Exit" +
        "Finally|For|Function|Func|" +
        "Hidden|" +
        "If|" +
        "Local|Lparameter|Lparameters|" +
        "Next|Noshow|" +
        "Otherwise|" +
        "Parameters|Printjob|Procedure|Proc|Protected|Public|" +
        "Scan|" +
        "Text|Then|Throw|To|Try|" +
        "While|With|" +
        "Activate|Add|Alter|Alternate|Ansi|App|Append|Array|Assert|Asserts|Assist|Autoincerror|Autosave|Average|" +
        "Bar|Begin|Bell|Blank|Blocksize|Border|Box|Browse|Browseime|Brstatus|Build" +
        "Calculate|Call|Cancel|Carry|Catch|Cd|Century|Change|Chdir|Class|Classlib|Clear|Clock|Close|Collate|Color|Compatible|Compile|Confirm|Connection|Connections|Console|Continue|Copy|Count|Coverage|Cpcompile|Cpdialog|Create|Currency|Cursor|Cursor" +
        "Database|Datasession|Date|Deactivate|Debug|Debugout|Decimals|Declare|Default|Define|Delete|Deleted|Delimiters|Development|Device|Dimension|Dir|Directory|Display|Dll|Dlls|Dock|Doevents|Dohistory|Drop|" +
        "Echo|Edit|Eject|End|Enginebehavior|Erase|Error|Escape|Eventlist|Events|Eventtracking|Exact|Exclusive|Exe|Export|Extended|External" +
        "Fdow|Fields|File|Files|Filter|Finally|Find|Fixed|Flush|Form|Format|Free|From|Fullpath|Function|Fweek" +
        "Gather|General|Get|Getexpr|Gets|Go|Goto|" +
        "Headings|Help|Hide|Hours|" +
        "Id|Import|Index|Indexes|Input|Insert|Intensity|" +
        "Join|" +
        "Key|Keyboard|Keycomp|" +
        "Label|Library|List|Load|Local|Locate|Lock|Logerrors|Loop|Lparameters|" +
        "Mackey|Macro|Macros||Margin|Mark|Md|Memo|Memory|Memowidth|Menu|Menus|Message|Mkdir|Modify|Mouse|Move|Mtdll|Multilocks|" +
        "Near|Nocptrans|Note|Notify|Nulldisplay|" +
        "Object|Objects|Odometer|Of|Off|Oleobject|On|Open|Optimize|Order" +
        "Pack|Pad|Page|Palette|Parameters|Path|Pdsetup|Play|Point|Pop|Popup|Popups|Printer|Private|Procedure|Procedures|Project|Public|Push" +
        "Query|Quit" +
        "Rd|Read|Readborder|Readerror|Recall|Refresh|Reindex|Relation|Release|Remove|Rename|Replace|Report|Reprocess|Resource|Restore|Resume|Retry|Return|Rmdir|Rollback|Run" +
        "Safety|Save|Scatter|Scheme|Screen|Scroll|Seconds|Seek|Select|Selection|Separator|Set|Show|Shutdown|Size|Skip|Skip|Sort|Space|Sql|Status|Step|Store|Strictdate|Structure|Sum|Suspend|Sysformats|Sysmenu" +
        "Table|Tables|Tablevalidate|Tag|Talk|Textmerge|This|Throw|To|Topic|Total|Transaction|Trbetween|Trigger|Try|Type|Typeahead" +
        "Udfparms|Unique|Unlock||Update|Use|" +
        "Validate|View|Views|" +
        "Wait|Window|Windows|Windows|" +
        "Zap|Zoom" 
    );

    var keywordMapper = this.createKeywordMapper({
        "support.function": functions,
        "keyword": keywords,
        "constant.language": constants
    }, "identifier", true);

    this.$rules = {
        "start": [
            {
                token: "comment",
                regex: "\\*.*$",
                //regex: "^\\s?\\*.*$"
            },
            {
                token: "comment",
                regex: "&&.*$"
            },
            {
                token : "keyword", // pre-compiler directives
                regex : "#\\s*(?:include|import|pragma|line|define|undef|if|ifdef|endif|else|elif|ifndef)\\b",
                next  : "directive"
            },
            {
                token: "string", // " string
                regex: '".*?"'
            },
            {
                token: "string", // ' string
                regex: "'.*?'"
            },
            {
                token: "string", // ' string
                regex: "\\[.*?\\]"
            },
            {
                token: "constant.numeric", // float
                regex: "[+-]?\\d+(?:(?:\\.\\d*)?(?:[eE][+-]?\\d+)?)?\\b"
            },
            {
                token: keywordMapper,
                regex: "[a-zA-Z_$][a-zA-Z0-9_$]*\\b"
            },
            {
                token: "keyword.operator",
                regex: "\\+|\\-|\\/|\\/\\/|%|<@>|@>|<@|&|\\^|~|<|>|<=|=>|==|!=|<>|="
            },
            {
                token: "paren.lparen",
                regex: "[\\(]"
            },
            {
                token: "paren.rparen",
                regex: "[\\)]"
            },
            {
                token: "text",
                regex: "\\s+"
            }
        ]
    };
};

oop.inherits(FoxProHighlightRules, TextHighlightRules);

exports.FoxProHighlightRules = FoxProHighlightRules;
});