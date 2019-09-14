"use strict";
/*
----------------------------------------
highlightJs Badge
----------------------------------------

A copy code and language display badge
for the highlightJs Syntax highlighter.

by Rick Strahl, 2019
License: MIT

Make sure this script is loaded last in your
script loading.

Usage:
------
Load `highlightjs-badge` after `highlight.js`:

```js
<link href="vs2015.css" rel="stylesheet">
<script src="highlight.pack.js"></script>

<script src="highlightjs-badge.js" async></script>  
```

The script contains the template and CSS so nothing
else is needed to execute it.

Customization:
--------------
This code automatically embeds styling and the template.

If you want to customize you can either create a template
in your HTML using the code at the end of this file.

Alternately you can customize the `getTemplate()` function
that renders the code from a string and keep it self contained
within this script.

Requirements:
-------------
Uses some ES6 features so won't work in IE without shims:

* Object.assign
* String.trim

*/

// module header
(function( global, factory ) {

	if ( typeof module === "object" && typeof module.exports === "object" ) {
		// For CommonJS and CommonJS-like environments where a proper `window`
		// is present, execute the factory		
		module.exports = global.document ?
			factory( global, true ) :
			function( w ) {
				if ( !w.document ) {
					throw new Error( "A window with a document is required" );
				}
				return factory( w );
			};
	} else {
		factory( global );
	}

// Pass this if window is not defined yet
}(typeof window !== "undefined" ? window : this, function( window, noGlobal ) {


    
if (typeof highlightJsBadgeAutoLoad !== 'boolean')
    var highlightJsBadgeAutoLoad = false;

function highlightJsBadge(opt) {
    var options = {
        // the selector for the badge template
        templateSelector: "#CodeBadgeTemplate",

        // base content selector that is searched for snippets
        contentSelector: "body",

        // Delay in ms used for `setTimeout` before badging is applied
        // Use if you need to time highlighting and badge application
        // since the badges need to be applied afterwards.
        // 0 - direct execution (ie. you handle timing
        loadDelay: 0,

        // CSS class(es) used to render the copy icon.
        copyIconClass: "fa fa-copy",     
        // optional content for icons class (<i class="fa fa-copy"></i> or <i class="material-icons">file_copy</i>)
        copyIconContent: "",

        // CSS class(es) used to render the done icon.
        checkIconClass: "fa fa-check text-success",
        checkIconContent: ""  
    };

    function initialize(opt) {
        Object.assign(options, opt);

        if (document.readyState == 'loading')
            document.addEventListener("DOMContentLoaded", load);
        else
            load();
    }


    function load() {
        if (options.loadDelay)
            setTimeout(addCodeBadge, loadDelay);
        else
            addCodeBadge();
    }

    function addCodeBadge() {
        // first make sure the template exists - if not we embed it
        if (!document.querySelector(options.templateSelector)) {
            var node = document.createElement("div");
            node.innerHTML = getTemplate();            
            var style = node.querySelector("style");
            var template = node.querySelector(options.templateSelector);
            document.body.appendChild(style);
            document.body.appendChild(template);
        }

        var $codes = document.querySelectorAll("pre>code.hljs");

        var hudText = document.querySelector(options.templateSelector).innerHTML;

        for (var index = 0; index < $codes.length; index++) {
            var el = $codes[index];
            if (el.querySelector(".code-badge"))
                continue; // already exists

            var lang = "";

            for (var i = 0; i < el.classList.length; i++) {
                // class="hljs language-csharp"
                if (el.classList[i].substr(0, 9) === 'language-') {
                    lang = el.classList[i].replace('language-', '');
                    break;
                }
                // class="kotlin hljs"   (auto detected)
                if (!lang) {
                    for (var j = 0; j < el.classList.length; j++) {
                        if (el.classList[j] == 'hljs')
                            continue;
                        lang = el.classList[j];
                        break;
                    }
                }
            }

            if (lang)
                lang = lang.toLowerCase();
            else
                lang = "text";

            if (lang == "ps")
                lang = "powershell";
            else if (lang == "cs")
                lang = "csharp";
            else if (lang == "js")
                lang = "javascript";
            else if (lang == "ts")
                lang = "typescript";
            else if (lang == "fox")
                lang = "foxpro";

                
            var html = hudText.replace("{{language}}", lang)
                              .replace("{{copyIconClass}}",options.copyIconClass)
                              .trim();

            // insert the Hud panel
            var $newHud = document.createElement("div");
            $newHud.innerHTML = html;
            $newHud = $newHud.querySelector(".code-badge");
            $newHud.style.display = "flex";

            if(options.copyIconContent)
              $newHud.querySelector(".code-badge-copy-icon").innerText = options.copyIconContent;

            el.insertBefore($newHud, el.firstChild);
        }

        var $content = document.querySelector(options.contentSelector);

        $content.addEventListener("click",
            function (e) {                               
                var $clicked = e.srcElement;
                if ($clicked.classList.contains("code-badge-copy-icon")) {
                    e.preventDefault();
                    e.cancelBubble = true;
                    copyCodeToClipboard(e);
                }
                return false;
            });
    }

    function copyCodeToClipboard(e) {
           
        var $origCode = e.srcElement.parentElement.parentElement.parentElement;

        // we have to clear out the .code-badge - clone and remove
        var $code = $origCode.cloneNode(true);
        var $elHud = $code.querySelector(".code-badge");
        $elHud.innerHTML = ""; // create text

        var text = $code.innerText;
        var el = document.createElement('textarea');

        el.value = text.trim();
        document.body.appendChild(el);
        el.style.display = "block";

        if (window.document.documentMode)
            el.setSelectionRange(0, el.value.length);
        else
            el.select();
        
        document.execCommand('copy');
        document.body.removeChild(el);
        
        swapIcons($origCode);     
    }

    function swapIcons($code) {
        var copyIcons = options.copyIconClass.split(' ');
        var checkIcons = options.checkIconClass.split(' ');
        
        var $fa = $code.querySelector(".code-badge-copy-icon");
        $fa.innerText = options.checkIconContent;

        for (var i = 0; i < copyIcons.length; i++)
            $fa.classList.remove(copyIcons[i]);
        
        for (var i = 0; i < checkIcons.length; i++)
            $fa.classList.add(checkIcons[i]);
        
        
        setTimeout(function () {
            $fa.innerText = options.copyIconContent;

            for (var i = 0; i < checkIcons.length; i++)
                $fa.classList.remove(checkIcons[i]);
            for (var i = 0; i < copyIcons.length; i++)
                $fa.classList.add(copyIcons[i]);
        }, 2000);
    }

    function getTemplate() {
        var stringArray =
        [
            "<style>",
            "    pre>code.hljs {",
            "        position: relative;",
            "    }",
            "    .fa.text-success:{ color: limegreen !important}",
            "    .code-badge {",
            "        display: flex;",
            "        flex-direction: row;",
            "        white-space: normal;",
            "        background: transparent;",
            "        background: #333;",
            "        color: white;",
            "        font-size: 0.8em;",
            "        opacity: 0.5;",
            "        transition: opacity linear 0.5s;",
            "        border-radius: 0 0 0 7px;",
            "        padding: 5px 8px 5px 8px;",
            "        position: absolute;",
            "        right: 0;",
            "        top: 0;",
            "    }",
            "",
            "    .code-badge.semi-active {",
            "        opacity: .50",
            "    }",
            "",
            "    .code-badge.active {",
            "        opacity: 0.8;",
            "    }",
            "",
            "    .code-badge:hover {",
            "        opacity: .95;",
            "    }",
            "",
            "    .code-badge a,",
            "    .code-badge a:hover {",
            "        text-decoration: none;",
            "    }",
            "",
            "    .code-badge-language {",
            "        margin-right: 10px;",
            "        font-weight: 600;",
            "        color: goldenrod;",
            "    }",
            "    .code-badge-copy-icon {",
            "        font-size: 1.2em;",
            "        cursor: pointer;",
            "        padding: 0 7px;",
            "        margin-top:2;",
            "    }",
            "</style>",
            "<div id=\"CodeBadgeTemplate\" style=\"display:none\">",
            "    <div class=\"code-badge\">",
            "        <div class=\"code-badge-language\" >{{language}}</div>",
            "        <div  title=\"Copy to clipboard\">",
            "            <i class=\"{{copyIconClass}} code-badge-copy-icon\"></i></i></a>",            
            "        </div>",
            "     </div>",
            "</div>"
        ];

        var t = "";
        for (var i = 0; i < stringArray.length; i++)
            t += stringArray[i] + "\n";

        return t;
    }

    initialize();
}


// global reference Window
window.highlightJsBadge = highlightJsBadge;

if (highlightJsBadgeAutoLoad)
    highlightJsBadge();


}));



/*
<style>
    pre>code.hljs {
        position: relative;
    }
    .fa.text-success:{ color: limegreen !important}

    .code-badge {
        display: flex;
        flex-direction: row;
        white-space: normal;
        background: transparent;
        background: #333;
        color: white;
        font-size: 0.8em;
        opacity: 0.5;
        border-radius: 0 0 0 7px;
        padding: 5px 8px 5px 8px;
        position: absolute;
        right: 0;
        top: 0;
    }

    .code-badge.semi-active {
        opacity: .50
    }

    .code-badge.active {
        opacity: 0.8;
    }

    .code-badge:hover {
        opacity: .95;
    }

    .code-badge a,
    .code-badge a:hover {
        text-decoration: none;
    }

    .code-badge-language {
        margin-right: 10px;
        font-weight: 600;
        color: goldenrod;
    }
    .code-badge-copy-icon {
        font-size: 1.2em;
        cursor: pointer;
        padding: 0 7px;
        margin-top:2;
    }
</style>
<div id="CodeBadgeTemplate" style="display:none">
    <div class="code-badge">
        <div class="code-badge-language">{{language}}</div>
        <div title="Copy to clipboard">
            <i class="{{copyIconClass}} code-badge-copy-icon"></i>
        </div>
     </div>
</div>
*/
