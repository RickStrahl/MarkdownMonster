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
*/


function highlightJsBadge() {
    

    // Configure template selector
    var templateId = "#CodeBadgeTemplate";
        
    // Configure base selector for content (or "body")
    var contentId = "body";

    if (document.readyState == 'loading')
        document.addEventListener("DOMContentLoaded",addCodeBadge);
    else
        setTimeout(addCodeBadge,1);



    function addCodeBadge() {
        // first make sure the template exists - if not we embed it
        if (!document.querySelector(templateId))
        {        
            var node  = document.createElement("div");
            node.innerHTML = getTemplate();
            document.body.appendChild(node.firstChild);
            document.body.appendChild(node.childNodes[1]);
        }
    
        var $codes = document.querySelectorAll("pre>code.hljs");

        var hudText = document.querySelector(templateId).innerHTML;
        
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
            
            el.insertBefore($newHud,el.firstChild);                         
        }

        var $content=document.querySelector(contentId);

        $content.addEventListener("click", 
                function(e) {
                    e.preventDefault();
                    e.cancelable = true;
                    e.cancelBubble = true;
                    
                    var $clicked = e.srcElement;                    
                    if ($clicked.classList.contains("code-hud-copy-icon"))
                        copyCodeToClipboard(e);
                        
                    return false;                        
                }  );            
    }

    function copyCodeToClipboard(e) {        
        var $origCode = e.srcElement.parentElement.parentElement.parentElement.parentElement;
        
        
        // we have to clear out the .code-hud - clone and remove
        var $code = $origCode.cloneNode(true);
        var $elHud =$code.querySelector(".code-hud");        
        $elHud.innerHTML = ""; // create text

        var text = $code.innerText;
        var el = document.createElement('textarea');
    
        el.value = text.trim();
        document.body.appendChild(el);
        el.style.display = "block";

        if(window.document.documentMode)
            el.setSelectionRange(0,el.value.length -1);
        else
            el.select();

                
        document.execCommand('copy');
        document.body.removeChild(el);

        $fa =$origCode.querySelector(".code-hud .fa");        
        $fa.classList.remove("fa-copy");
        $fa.classList.add("fa-check");
        setTimeout(function() { 
            $fa.classList.remove("fa-check");
            $fa.classList.add("fa-copy");
        },2000);       
    
    }
    
    // String.trim() for ES5 polyfill
    if (!String.prototype.trim) {
        String.prototype.trim = function () {
          return this.replace(/^[\s\uFEFF\xA0]+|[\s\uFEFF\xA0]+$/g, '');
        };
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
                "        padding: 5px 15px !important;",
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
                "        color: lightsteelblue;",
                "    }",
                "",
                "    .code-hud-lang {",
                "        margin-right: 20px;",
                "        font-weight: 600;",
                "        color: darkgoldenrod;",
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
}
window.highlightJsBadge = highlightJsBadge;
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