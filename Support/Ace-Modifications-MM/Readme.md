# Build ACE for Markdown Monster

In order to get Ace Markdown highlighting to work properly with a few custom enhancements it's important to actually build ACE or at minimum copy a number of files.

In order to build final output you can do:

```powershell
Set-Location \projects\Ace-Modifications-MM
.\CopyToAceBuild.ps1
explorer ..\ace\build\src
```


## Syntax Modes
The following compiled modes are available:

* `mode-foxpro`
* `mode-http`
* `mode-markdown`

### Markdown Mode Syntax
The Markdown mode is not unique and is based on the ACE provided Markdown syntax which is frequently updated, so it's a good idea from time to time to rebuild this mode which has to be done by **rebuilding the ACE source code**.

The customized Markdown syntax provides syntax highlighting for code fenced code blocks:

* csharp
* typescript
* json
* powershell
* FoxPro

The code is available in:

* `modes/markdown.js`  (build with ACE)
* `compiled-modes/mode-markdown.js`  (pre-build and ready to copy but may be out of date)

When compiling in Ace:

* Compare `modes/markdown.js` with the file in `ace/lib/ace/mode/markdown.js`
* This customized version adds the extra syntax for code fencing
* Go into the `ace/build` folder
* Build the output:

```ps
# ace root folder
npm install
node ./Makefile.dryice.js
```

* Copy updated files from the `bld/src/folder` (uncompressed)
* Ideally use BeyondCompare and copy all the Red flagged file only
* Check for anything else that might be of interest

### Themes
Install the following themes:

* `theme-visualstudio.js`
* `theme-vscodedark.js`
* `theme-vscodelight.js`

  
## Compare MM with Ace using Beyond Compare
Use **Beyond Compare** to compare the `ace/build/src` folder to the `/MM/MarkdownMonster/Editor/scripts/ace` folder. Update all files that are marked in red as having changes.
