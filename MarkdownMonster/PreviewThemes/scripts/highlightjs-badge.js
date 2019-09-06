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

if (typeof highlightJsBadgeAutoLoad !== 'boolean')
    var highlightJsBadgeAutoLoad = true;

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
        loadDelay: 0
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
            document.body.appendChild(node.firstChild);
            document.body.appendChild(node.childNodes[1]);
        }

        var $codes = document.querySelectorAll("pre>code.hljs");

        var hudText = document.querySelector(options.templateSelector).innerHTML;

        for (var index = 0; index < $codes.length; index++) {
            var el = $codes[index];
            if (el.querySelector(".code-hud"))
                continue; // already exists

            var lang = "";

            for (var i = 0; i < el.classList.length; i++) {
                if (el.classList[i].substr(0, 9) === 'language-') {
                    lang = el.classList[i].replace('language-', '');
                    break;
                }
            }

            if (lang)
                lang = lang.toLowerCase();

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

            var html = hudText.replace("{{lang}}", lang).trim();


            // insert the Hud panel
            var $newHud = document.createElement("div");
            $newHud.innerHTML = html;
            $newHud = $newHud.querySelector(".code-hud");
            $newHud.style.display = "flex";

            el.insertBefore($newHud, el.firstChild);
        }

        var $content = document.querySelector(options.contentSelector);

        $content.addEventListener("click",
            function (e) {                
                e.preventDefault();
                e.cancelBubble = true;
                var $clicked = e.srcElement;
                if ($clicked.classList.contains("code-hud-copy-icon"))
                    copyCodeToClipboard(e);

                return false;
            });
    }

    function copyCodeToClipboard(e) {
           
        var $origCode = e.srcElement.parentElement.parentElement.parentElement;


        // we have to clear out the .code-hud - clone and remove
        var $code = $origCode.cloneNode(true);
        var $elHud = $code.querySelector(".code-hud");
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

        var $fa = $origCode.querySelector(".code-hud .fa");
        $fa.classList.remove("fa-copy");
        $fa.classList.add("fa-check");
        setTimeout(function () {
            $fa.classList.remove("fa-check");
            $fa.classList.add("fa-copy");
        }, 2000);

       

    }


    function getTemplate() {
        var stringArray =
            [
                "<style>",
                "    pre>code.hljs {",
                "        position: relative;",
                "    }",
                "",
                "    .code-hud {",
                "        display: flex;",
                "        flex-direction: row;",
                "        white-space: normal;",

                "        background: transparent;",
                "        background: #333;",
                "        color: white;",
                "        font-size: 0.8em;",
                "        opacity: 0.5;",
                "        border-radius: 0 0 0 7px;",
                "        padding: 5px 15px 5px 10px !important;",
                "        position: absolute;",
                "        right: 0;",
                "        top: 0;",
                "    }",
                "",
                "    .code-hud.semi-active {",
                "        opacity: .50",
                "    }",
                "",
                "    .code-hud.active {",
                "        opacity: 0.8;",
                "    }",
                "",
                "    .code-hud:hover {",
                "        opacity: .95;",
                "    }",
                "",
                "    .code-hud a,",
                "    .code-hud a:hover {",
                "        text-decoration: none;",
                "    }",
                "",
                "    .code-hud-lang {",
                "        margin-right: 10px;",
                "        font-weight: 600;",
                "        color: darkgoldenrod;",
                "    }",
                "    .code-hud-copy-icon {",
                "        padding: 0 7px;",
                "    }",
                "</style>",
                "<div id=\"CodeBadgeTemplate\" style=\"display:none\">",
                "    <div class=\"code-hud\">",
                "        <div class=\"code-hud-lang\" >{{lang}}</div>",
                "        <div  title=\"Copy to clipboard\"><i class=\"fa fa-copy code-hud-copy-icon\"",
                "                style=\"font-size: 1.2em; cursor: pointer; \"></i></a>",
                "    </div>",
                "</div>"
            ]

        var t = "";
        for (let i = 0; i < stringArray.length; i++) {
            t += stringArray[i] + "\n";
        }

        return t;
    }

    initialize();
}
if (highlightJsBadgeAutoLoad)
    highlightJsBadge();

/*
<style>
    pre>code.hljs {
        position: relative;
    }

    .code-hud {
        display: flex;
        flex-direction: row;
        white-space: normal;

        background: transparent;
        background: #333;
        color: white;
        font-size: 0.8em;
        opacity: 0.5;
        border-radius: 0 0 0 7px;
        padding: 5px 15px !important;
        position: absolute;
        right: 0;
        top: 0;
    }

    .code-hud.semi-active {
        opacity: .50
    }

    .code-hud.active {
        opacity: 0.8;
    }

    .code-hud:hover {
        opacity: .95;
    }

    .code-hud a,
    .code-hud a:hover {
        text-decoration: none;
        color: lightsteelblue;
    }

    .code-hud-lang {
        margin-right: 20px;
        font-weight: 600;
        color: darkgoldenrod;
    }
</style>
<div id="CodeBadgeTemplate" style="display:none">
    <div class="code-hud">
        <div class="code-hud-lang" style="flex: 1">{{lang}}</div>
        <a href="#x0" title="Copy to clipboard"><i class="fa fa-copy code-hud-copy-icon"
                style="font-size: 1.2em"></i></a>
    </div>
</div>
*/
