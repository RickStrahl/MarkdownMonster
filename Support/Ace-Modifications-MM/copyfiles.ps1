$editorPath = "\projects\markdownmonster\MarkdownMonster\Editor"
$aceBuildPath = "\projects\ace\build\src"

copy $aceBuildPath\theme-vscodedark.js  $editorPath\scripts\Ace\theme-vscodedark.js
copy $aceBuildPath\theme-vscodelight.js $editorPath\scripts\Ace\theme-vscodelight.js
copy $aceBuildPath\theme-visualstudio.js $editorPath\scripts\Ace\theme-visualstudio.js


copy $aceBuildPath\mode-csharp.js $editorPath\scripts\Ace\mode-csharp.js
copy $aceBuildPath\mode-foxpro.js $editorPath\scripts\Ace\mode-foxpro.js
copy $aceBuildPath\mode-markdown.js $editorPath\scripts\Ace\mode-markdown.js

copy  .\compiled-modes\mode-http.js $editorPath\scripts\Ace\mode-http.js