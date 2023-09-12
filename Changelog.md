[![download](https://img.shields.io/badge/Download-Installer-blue.svg)](https://markdownmonster.west-wind.com/download)
[![Web Site](https://img.shields.io/badge/Markdown_Monster-WebSite-blue.svg)](https://markdownmonster.west-wind.com)

### 3.0.3

* **Installation moved to Program Files**  
We've moved the default installation location for the full installation to the `Program Files` folder to avoid common installation issues related to Windows Account usage that in the past would install certain components in the wrong location when elevation was required. We've also moved all remaining updatable content (except the preview templates and previewers) out of the install folder into common file location.

* **Portable Install Behavior Remains unchanged**  
Besides the changes to the full install, the Portable Install can still be installed in any location of your choice as a self-contained install that can store all application, and common updatable configuration and support data in a local folder hierarchy. Portable installs by default use a contained `PortableSettings` folder which falls back to the `%appdata%\Markdown Monster` common path that is also used by the full install **if** permissions are not available to write files. This behavior is mostly unchanged except the additional files that now go into the `PortableSettings` or common folder.

* **Fix: WebView Install Folder in Startup Location**  
Fixed issue where the YouTube embedding dialog was causing the WebView to create a separate WebView environment in the install folder rather than the common shared location used by all other WebView controls. This also fixes a common point of crashes

#### Breaking and Recommended Changes 
* **Recommend full uninstall and reinstall for Full Installations**  
Due to the move to `Program Files` we recommend you do a full, uninstall and then re-install Markdown Monster if you are using the full installer or Chocolatey install. It's not required, and if you don't re-install the existing `%localappdata%\Markdown Monster` or your own custom location will continue to be used. The explicit uninstall ensures that the new Programs Files path is used on a new install. Portable installs don't need to have anything changed.

* **Remove `WebView_` Folder From Markdown Monster Install Folder (if present)**  
If your MM installation folder contains a `WebView_*` folder, it's recommended that you shut down MM and delete the entire folder. This folder was not intended to be placed and should be deleted so it will no longer take up space.

### 3.0.1 - Official Release

* **Updated Version runs on .NET 7.0**  
We've moved Markdown Monster to run under **.NET 7.0** to take advantage of better integration with new .NET features and support libraries as well as slightly improved performance and many development improvements on our end. This release requires the **.NET 7.0 Desktop Runtime** (7.0.3 or later). The latest runtime is installed as part of the full setup process *if a compatible version is not present*. For the portable version the runtime either needs to be present or has to be installed. To install the runtime you can use  `mm.exe -runtimeinstall` or manually download and install from Microsoft's [.NET Download site](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).

* **Added GitHub Dark Preview Theme**  
Since GitHub has supported dark mode for some time we've added a `GitHub Dark` theme. This is now also the default preview theme for dark mode. Additionally there are a few adjustments to the GitHub (light) theme to keep it up to date with the actual GitHub site.

* **Light Theme Improvements**  
Added a number of modifications and cleanups to the light theme. Many improvements in background/foreground combos, selections as well as using more distinctive colors for separators and borders.

* **Updated Icons throughout Application**  
We needed to move off our old FontAwesome 4 icon set due to incompatibility issues in .NET Core, and in the process we are now are running on FontAwesome 6 which provides many additional icon options as well as improved color and stroke support. New and sharper icons make for clearer toolbar, button and menu options throughout the application.

* **Drag and Drop support for Video and Audio Files Links**  
You can now drag video and music files from the folder browser and Explorer into the editor and have them auto-linked as media files. Videos embed as a video player, audio files as audio player.  
*Note: Audio/Video linking is not supported by all Markdown platforms/renderers, so make sure that your target platform supports this functionality.*

* **New WebLog Post Option to post to New-Posts Folder first**  
For those of you that write a lot of blog posts or procrastinate with finishing posts, it can be useful to create new posts in the *New Posts* folder until the post is ready to be published. If the post is in `New-Posts` or `Drafts` folder, publishing will prompt you to optionally save the post in the WebLog post folder structure (`postFolder/yyyy-MM/title-of-the-post`). There's no obligation to use this of course and you can always move your posts to whatever location you like, but this provides an organized way to keep track of your posts and drafts.

* **[Add Custom External Programs for Documents](https://markdownmonster.west-wind.com/docs/_6if1ephjm.htm)**  
You can now add external programs to launch for active document in the editor or for files and folders in the Folder Browser. Launch individual programs or URLs and specify arguments in a variety of ways. (belatedly fixes [#947](https://github.com/RickStrahl/MarkdownMonster/issues/947))

* **[Enable Mermaid Charts By Default](https://markdownmonster.west-wind.com/docs/_5ef0x96or.htm)**  
With wider support of Mermaid in various rendering platforms including GitHub, Mermaid rendering is now enabled by default. There's still a Setting flag that can be used to turn checking for Mermaid code on and off which can improve load and render speed very slightly when `false`. *Note: If you're updating a previous version, your current setting will not change so if it was set at default of `false` it'll continue to stay that way and have to be manually enabled. You can use Tools|Settings|Mermaid to set this value.*

* **Update ACE Editor to current version**   
It's been a while since the last synced to Ace Editor updates and it looks like there are a number of improvements and updates to the Markdown syntax. Also added a few additional updates to the csharp syntax.

* **Preview Zoom Sticky across Tabs**  
Preview Zoom previously only worked on the active Preview tab and would not stick around if a new tab was activated or even when navigating back to the same tab. Now applying a zoom level (via Ctrl-Mousewheel or `ctrl-+` and `ctrl+-`) is applied to **all preview tabs** and remains sticky for the entire session.

* **Add Non-Breaking Space and Non-Breaking Hyphen Markup**  
Added menu and command palette options for inserting non-breaking space and non-breaking (`&nbsp;`) and non-breaking hyphen (`&#8209;`) characters into markup. You can find it on the `Extra Markup (ctrl-x)` menu and using the command palette. ([#1034](https://github.com/RickStrahl/MarkdownMonster/issues/1034))

* **Add Smaller Text Icon to Main Toolbar**  
There's now a small text icon on the toolbar that wraps text with `<small>text</small>` tags.

* **Improvements to Folder Browser Navigation and Context Menu**  
More improvements to improve folder browser navigation. Selections should be much smoother, drag and drop more responsive and the context menu should show with less jitter.

* **Folder Browser PDF Documents now display in the Previewer**  
When you use the Folder Browser and navigate to a Pdf file, the file is now displayed in the previewer without requiring to click in explicitly. You can still open PDF files externally via `Open` options on the context menu. The previewer is a Chromium based preview so you get all standard browser print and PDF export options as you would in an external browser.

* **Video and MP3 Files now open in the Previewer**  
`.mp4` and `.mp3` files now open in the Previewer to play when double clicked in the Folder Browser.

* **[Built-in Web Server improvements](https://markdownmonster.west-wind.com/docs/_5s1009yx1.htm)**  
The built-in Web server now is more responsive and provides additional commands for remote execution.

* **Change: Unlabeled fenced Code Blocks render as Text**  
Code blocks that don't have an explicit language specified (ie. ` ``` ` instead of  ` ```csharp`) now render as plain text instead of attempting to auto-detect language. Auto-detection often would pick the wrong language as it can be very ambiguous and GitHub also renders as plain text. ([#1001](https://github.com/RickStrahl/MarkdownMonster/issues/1001))


* **[Support for Async Code Snippet Templates](https://markdownmonster.west-wind.com/docs/_5gs0uc49h.htm#c-code-execution)**  
Code Snippets now support `await` calls in C# expressions or code blocks. This is necessary for accessing many of the `Model.ActiveEditor` methods that effect editor behavior (most commonly `await Model.ActiveEditor.GetSelection()`). 

* **[Support for Structured Statements in Code Snippet Templates](https://markdownmonster.west-wind.com/docs/_5gs0uc49h.htm#c-code-execution)**  
C# snippets now also support structured code blocks using `{{% <statement> }}` that are directly embedded as code. This allows for `if` and `for` type structured statement blocks that can bracket other text or expressions. But it also allows for arbitrary C# code blocks to be executed and act as integrated code.


* **[Custom, On-Demand .NET Runtime Installation Launcher](https://weblog.west-wind.com/posts/2023/Jun/21/Creating-a-Runtime-Checker-and-Installer-for-a-NET-Core-WPF-Application)**  
MM 3.0 uses .NET 7.0 and requires a Desktop Runtime installation to run and these runtimes are not installed on Windows by default.  We've created a custom, on-demand runtime checker and download and install process that only runs if the minimum runtime requested for MM isn't installed. This helps run on .NET 7.0 without having to ship runtimes as part of the distribution keeping download sizes down. Currently we require .NET 7.0.3 or later.

* **Removed automatic WebView Installer in Setup**  
As Windows 10+ now automatically updates the WebView control as part of Windows updates, we'll assume a valid version of the WebView is installed, so the WebView installer no longer runs on setup, but rather checks for a valid version inside of the application on startup. In the rare case that a newer version is required we then run the installer explicitly. This makes for a much cleaner and quicker install experience in most cases.

* **Dropped Support for Windows 7 and 8, Server 2012 and earlier**
This unfortunate change is due to requirements of the WebView component and Chromium Engine used by that that is dropping support for pre-Windows 10 versions of Windows. The WebView is used in MM to render the editor and preview panes as well as various support screens. MM still works as of now until the 1900 releases of the WebView come into release which officially drop the earlier Windows versions.
*versions prior to 3.0 should continue to work with older Windows versions*

* **[Add `mmcli enable-windows-longpaths`](https://markdownmonster.west-wind.com/docs/_68d0r8rej.htm)**  
Added two `mmcli` commands to allow enable/disable Windows Long Path support. Two commands are `enable-windows-longpaths` and `disable-windows-longpaths`.

* **Fix: Opening folders from the Command Line in Folder Browser**  
Fixed issue When using the Commandline syntax (ie. `mm .` or `mm c:\temp`) to open folders which open in the folder browser. Folder often would or start loading and 'get stuck'. Fixed via slightly delayed load and async updates.

* **Fix: High DPI Scaling for Editor Pane**  
Fix issue where large DPI zoom levels were over adjusting the size of the editor pane's font size.

* **Fix: Track Requests in Folder Browser**  
Fix the tracking feature so that it selects the currently active file in addition to the folder. Regression fix for behavior change in recent async upgrade.

* **Fix Presentation Mode when navigating to other documents** 
Fix issue where navigating to other documents from within presentation mode would jump out of presentation mode and into a broken intermediate mode where you need to press F11 once to get back to 'regular' edit/preview mode and twice to get back into presentation mode. The fix lets you navigate the target document which then also shows in presentation mode. ([#1033](https://github.com/RickStrahl/MarkdownMonster/issues/1033))

* **Fix: Folder Browser Context Menu doesn't close**  
Fix issue with Folder Browser context menu not closing. Fixed. ([#1036](https://github.com/RickStrahl/MarkdownMonster/issues/1036))

* **Fix: Slow Git Window Activation on large Git Repos**  
When accessing large Git repos first load of the Git dialog was very slow. Reduced reptitive status look up and added an async wrapper to initial status retrieval. Fixed.

* **Fix: Mermaid Rendering to use new Markup Syntax**  
Mermaid recently switched its default rendering syntax from using `<div>` tags to `<pre>` tags. Oddly some charts still worked, while others were failing only working with `<pre>` tags. We now use `<pre>` tag translation of **``` mermaid** directives.

* **Fix: Better Error Messages for Unbalanced Grid Tables**  
Provide better error message that includes the row that failed when Grid tables cannot be parsed - typically due to unbalanced `|` characters. You now get a status error message that lists the row where the problem occurs.

* **Fix: Distraction Free Mode Issues**  
Fix distraction free mode bug that didn't hide Tabs in Light mode. Fix issues with properly restoring mode when undoing distraction free mode.

### 2.9

<small>April 2nd, 2023</small>

* **Multi-Line Markup for Inline Elements**  
You can now select multiple lines of inline elements - bold, italic, strikethrough, underline etc. - and apply it to multiple lines. ([#1003](https://github.com/RickStrahl/MarkdownMonster/issues/1003))

* **Multi-Line Markup for Soft Returns**  
You can now select multiple lines and apply Soft Returns to all the lines in the selection which makes it easier to work with list text that doesn't use bullets.  ([#1003](https://github.com/RickStrahl/MarkdownMonster/issues/1003))

* **Easier Html Exports with a new Export Dialog**  
Html Exports from your markdown are now easier with a dedicated dialog that lets you select the type of Html export (raw fragment, self-contained Html document, folder assets, zip file). This makes the export a lot less cryptic than previous Save File dialog that used only file types for 'hints' on functionality. ([#1000](https://github.com/RickStrahl/MarkdownMonster/issues/1000))

* **Add Mermaid Markup ShortCuts and Command Palette**  
You can now use the Extra Markdown dropdown and the Command Palette to wrap Mermaid diagrams automatically with the required Mermaid markup tags. You can wrap Mermaid code from the Clipboard or from the editor text selection. ([#1018](https://github.com/RickStrahl/MarkdownMonster/issues/1018))

* **Add DarkMode to Emoji Picker**  
Added dark mode operation to the Emoji picker and also bumped up the size of the individual emojis a bit.

* **Add Copy to Clipboard for Code Snippets to GitHub Preview**  
The GitHub preview now has buttons on code snippets to copy code to clipboard when previewing or exporting as HTML.

* **Re-enable Shell Drag and Drop for Images and Markdown Documents**  
You can now once again drag images and Markdown documents into the editor from Explorer or other shell explorers. Files can only be dropped at the currently selected cursor location - the location cannot be changed with the drag cursor unfortunately. This feature was temporarily disabled as the new WebView uses a completely different mechanism for file dropping.

* **Embedded Document TOC now preserves header Markup (bold, italic, code etc.)**  
When you embed a TOC into the document, any markup in extracted headers is preserved. So the TOC now includes inline bold, italic, and code text to match the original headers. 

* **Show Diff Editor for Crash Backup files when Opening Files**  
If `AutoSaveBackups` is enabled MM creates a backup file of the open document that is removed when the document is saved or closed. If the file is around when the document is opened MM likely crashed and lost some changes. If the backup file is found, MM will now open a Diff editor if configured and also position the folder browser on the back up file.

* **Add a Show All Files Button to the Folder Browser**  
The Folder Browser now has a button that shows all files that ignores the file and folder ignore list. This lets you see back up files (if enabled), git and editor files and more as well as any custom ignored files you've added.


* **Fix: Preview Link Navigation**  
Fix regression bug related to async processing which caused navigation to external links to navigate the browser and lose the preview document. Fixed. Also refactored the document processing pipeline for opening documents from the previewer to fix previous lockups in that process and navigation to specific lines in the editor after opening.

* **Fix: Random Application Crashes**  
Fix a problem with random crashes related to dialogs opening in an invalid Task context and Alt menu activation in some instances causing crashes. Both operations are related to a recent bug introduced in the WebView and code getting triggered that fails at the system level. Fixed by properly handling the Task environment for these operations, so that the WebView code that triggers these errors is not actually invoked.

* **Fix: Alt Menu Activation**  
Alt Menu activation for the Window menu was not working and often crashing the application due to a change in the underlying WebView2 control. Fixed menu activation logic.  
*Note: Alt menu activation from within the editor tends to need an **extra character** to activate the menu for navigation (ie. to activate the Window menu  `alt-alt-w` or `alt-w-w` get you there instead of `alt-w`).*

* **Fix: Automatic File Backup not Working**  
Fixed issue where Auto file back was not correctly saving the backup file in the same location as the base document. Fixed. Related Fix: Added support to Show all Files in Folder browser that lets you see the backup files in the Folder browser.

* **Fix: Change File Detection in Inactive Tabs**  
Fix issue where inactive tabs that have underlying file changes and aren't dirty locally were not automatically updating from underlying file changes. Now an underlying file change will automatically update the document when the tab is activated.

* **Fix: PDF Links with Spaces or Extended Characters**  
Fix an issue where PDF output was breaking links that were UrlEncoded due to spaces or extended characters in the link. Fixed by Url Decoding before rendering. ([#1011](https://github.com/RickStrahl/MarkdownMonster/issues/1011))

* **Fix: Save as PDF Extra Page**  
Fix issue where when you use the Print Dialog's Save to PDF generates a trailing blank page due to CSS styling. Fix CSS to remove extra page.

* **Fix Presentation Mode Scrolling and Sync**  
Fix issue with presentation mode where the viewer was not scrolling properly in presentation mode. Fixed. Also fixed issue with the sync between the editor and the preview in presentation mode which now preserves the location when toggling between modes and keeps in sync in presentation mode. ([#1019](https://github.com/RickStrahl/MarkdownMonster/issues/1019))

* **Fix: File System Drag and Drop into the Editor**  
When the WebView was introduced file system drag and drop into the editor no longer worked. Files dropped would open in a browser window or open in the shell. This update supports various types of dropped files, from explorer by dropping them into the document at the current editor location (no support for moving the caret though). Browser links and images can now also be dropped and auto-link as links or remote images (use **Copy Image** for locally copying Web images).

* **Fix: Context Menu Oddities**  
Fixed timing issue with context menus where menus were occasionally displaying the default Edge context menu instead of MMs native menu. Removed async call interface, and only applied to the spellcheck display now which improves menu popup speed and always bypasses the native menu. This was most notable with Table Editor functionality like Edit and Format Table.

* **Fix: Document Outline Sidebar with Bold and Italic Text**  
Document outline incorrectly displayed inline bold and italic text as Markdig Inlines. Fixed. ([#1013](https://github.com/RickStrahl/MarkdownMonster/issues/1013))

* **Fix: Startup Offscreen Positioning**  
Changed logic used for MM startup detection of offscreen locations on the desktop, and moving the window into the visible Viewport if completely or mostly obscured. Previously, in some instances Windows were moved even if they were fully visible. ([#998](https://github.com/RickStrahl/MarkdownMonster/issues/998))

* **Fix: Format Table with Left and Center Alignment**  
Fix issue where left and center alignment on Pipe tables was adding an extra space. ([#1005](https://github.com/RickStrahl/MarkdownMonster/issues/1005))

* **Fix: Text Drag and Drop starting with # tag No longer drops as Link**  
Fixed issue where dragging a text selection in the editor that starts with `#` would drop as a link expansion rather than plain text. The `#` now pastes as plain text as expected. ([#1002](https://github.com/RickStrahl/MarkdownMonster/issues/1002))

* **Fix: Snippet Execution Fails with Dynamic C# Expressions/Code**  
Fix bug with Snippets execution which include code snippets due to a removed reference.
([#1027](https://github.com/RickStrahl/MarkdownMonster/issues/1027))

* **Fix: Distraction Free Mode Issues**  
Fix Tab Headers not hiding in Light Theme mode. Fix resizing when coming out of DFM. Don't save Window sizing information when shutting down and restore to previous non DFM state.


### 2.8

*<small>January 26th, 2023</small>*

* **Add Proper and Camel Case to Extra Commands and Command Palette**  
You can now make selected text proper/title cased or camel cased via the command palette.

* **Command Palette: Added Font Settings**  
You can now access the font settings in the Visual Settings Editor via `Ctrl-Shift-P Font`. Also added a tooltip to the font size dialog to point at the Settings and Command Palette shortcuts.

* **Folder Browser Keyboard Navigation now Previews**  
When navigating the folder browser with the up and down keys, the editor now displays a preview of documents similar to the behavior when single clicking on a document. Editable (Markdown, text and other known editable formats) are opened in preview mode, meaning once you navigate off the document is closed or replaced by the next preview document, unless the document has been edited or explicitly opened as a full document.

* **Table Editor Paste now supports CSV directly**  
CSV imports into the table editor have been supported via **Load From** for a long time, but you can now also use the **Paste** operation to CSV content from Excel, Google Docs etc. directly into the table editor. **Load From** still has more control but **Paste** is quicker assuming the content uses tab delimited clipboard CSV. Paste now supports: Grid Table, Pipe Table, HTML, JSON and CSV. Load From also allows loading CSV and JSON from files (and clipboard explicitly with options).

* **Update: Default Font Size to 16px**  
Set default font-size a little smaller to prevent very low res displays from displaying too big of a font initially. This will make fonts smaller for hi-res displays, but people that have hi-resolutiuon displays are used to having to adjust font-sizes up typically.

* **Update: Markdown Monster now uses latest WebView Async Model**  
A few versions ago, changes in the async WebView control model broke most of Markdown Monsters JavaScript to .NET interop functionality that caused us to stick with older versions of the .NET WebView connector. In this release we've refactored all of our JS->.NET interfaces to work with the new async behaviors. This true async support should also improve responsiveness of some tasks, as well as minimize the occasional WebView crashes we've observed in our logs.

* **Fix: Git Commit Dialog Load Time**  
Fix issue with the Git dialog taking a long time to load with large repositories. We've refactored the dialog to load the repository after the form has initialized which removes a race condition that was severely slowing down the initial load of the repository. The form now loads immediately with a status message in case of slowish loads (which should now be rare) and for most repos the load should be near instant.

* **Fix: Table Editor Add Row on Bottom**  
Fix behavior of Add row on bottom to continue keeping focus and scroll into visible viewport. Also now allow adding multiple empty rows at the end (previously this wasn't allowed). You can now also add a new Header row by Add new Row in the header.

* **Fix: Various Range Bound Errors in Table Editor**  
Fix several errors that caused the table editor to crash when adding/removing many rows/columns.

* **Fix: Status Icon Animation Sizing**  
Fix issue where on occasion the status icon would bloat to massive size if messages were overlapping.

* **Fix: Document Navigation in the Previewer**  
Fix crash/hang issue when navigating to other documents from the Preview Window.

* **Fix: Change Scrollbar Sizes for Editor and Preview**  
The default scrollbar sizes for the Editor and Previewer have been bumped up slightly to better work with hi-res displays and sizing has been switched from hard pixel sizing to `em` sizing. Previewer and Editor scrollbar styling can be applied in the generic `Editor/editor.css` and `PreviewThemes/MyTheme/theme.css` for the specifically active theme.

* **Fix: Folder Browser File Renaming Issues**  
Fix issue where rename operations would in some cases not save pending changes and wouldn't delete the originally renamed file. Error was introduced during async conversion and due to timing issues. Fixed. [#986](https://github.com/RickStrahl/MarkdownMonster/issues/986)

### 2.7

*<small>October 18th, 2022</small>*

* **Add File Info Tooltips for Preview Tabs**  
Preview tabs now show the filename and display a tooltip that has file information in the same way as editor documents do.

* **Add Image Size to Image File Tooltips**  
Images in the Folder Browser and open Tabs now display image tooltips that include image size (width x height) and image DPI.

* **Preview Tab File Names and Tooltip**  
Preview tabs for non-document files now show the filename and file tooltip info just like document tabs. Previously, non-document tabs behaved differently due to the missing document instance.

* **Command Palette: Add Speech/Read Aloud Commands**  
Add commands for reading aloud to the command palette. Access via *Speak* or *Read Aloud* keywords.

* **Command Palette: Many more Commands Added**  
All List operations (plain, numbered and checkbox), Emojii, YouTube embedding, Find in document, Find Files, Find in Files, among otherrs. To access Command Palette press `Ctrl-Shift-P` or use the searchbox in the Title bar.

* **Fix: Refresh Cached Images from Clipboard**  
Fixed issue where pasting an image from the clipboard would not properly refresh the preview and scroll to the top of the preview - until next cursor movement. Also fixed caching issue where pasting new same name image would cache and not show the new image.

* **Fix: *Open With...* From Folder Browser**  
Open With stopped working apparently due to a framework change that related to the `UseShellExecute` setting which has to be explicitly set off to open the dialog.

* **Fix: Startup Path Fixup for `CommonFolder`**  
Fixed issues with the common folder location that requires that some files and folders to be stored on the local machine. Previously the `InternalCommonFolder` was erroneously the same as `CommonFolder` causing some look up issues

* **Fix: Folder Browser External File Update and File Info**  
Fix issue where newly added files and files that have changed did not reflect current file information. Change detection now reloads updated file information.

* **Fix: Right Click Focus in Folder Browser**  
Fix folder browser focus when right clicking on files. In some situations the selection of the previous item would not clear and the context menu would pop up for the previous selection. Updated logic to explicitly select on right click. Fixed.

* **Fix: Context Menu Key not working**  
Fix the Context Menu key on keyboards not working. Re-enabled, but behavior is not ideal as it displays at mouse cursor, not at editor position. 

* **Fix: Escape Pipe `|` Characters in Grid Tables**  
Pipe characters in Grid Tables have to be escaped and using pipes in Grid Tables prerviously broke Grid parsing as the `|` was interpreted as a column break. Fixed by now properly escaping `|` with `\|` in the Markdown editor and using plain `|` in the Table Editor.

* **Fix: Git Pull Issue with Pull when using Branch other than Master**  
Fixed issue with Pull not respecting the active Branch in the Git dialog. ([#976](https://github.com/RickStrahl/MarkdownMonster/issues/976))

* **Fix: Weblog Post not updating Markdown YAML Metadata**  
Depending on your selection of `Auto Save Document` information typed into the Weblog Publishing dialog would not update the meta data in the document. When Auto Save was not set the data was not updated (incorrect) and not saved (correct). Fixed: Now the document is always updated, and the flag correctly affects only whether the document is auto-saved after the meta data has been updated.

* **Fix: Pop up Context Menu with Context Menu Key at Cursor Position**  
Fix issue where keyboard Context Menu key was not opening the context menu at the current cursor location.

* **Fix: Cached WebBrowser Environment Location**  
Fixed browser environment location which now properly uses the local machine common path, rather than a potentially shared common path. Sharing in a common path could cause corruption and unexpected browser lockups and crashes.

* **Fix: License Unregistering**  
Fixed issue with licenses getting unregistered due to a registration issue on the server end missing the version number. Fixed recent licenses by adding versions properly, and new licenses creating with product version supplied.

### 2.6
 
<small>June 30th, 2022</small>
 
* **[Drag and Drop Link Insertion from Document Outline into Editor](https://markdownmonster.west-wind.com/docs/_55o1bd4n1.htm)**  
You can now drag a document outline selection into the open Markdown Document as a link that points at the `#Hash` id in the document. <small>([#936](https://github.com/RickStrahl/MarkdownMonster/issues/936))</small>


* **Improved Snippet Startup Speed**  
With the new Roslyn integration which runs in-process,  startup speed of first snippet activation  is much improved even on a cold start. Additionally the `PreloadCSharpCompiler` configuration flag in the Snippets addin can reduce startup speed down to a fractional second.

* **Add Subscript and Superscript Markup Operations**  
Added sub and superscript to the Extra Markdown operation drop down menu which creates `<sub></sub>` and `<super></super>` wrappings around text selections.

* **Keybindings for Numbered List and CheckBox List**  
Added keybindings for number list (`ctrl-shift-l`) and checkbox list (`ctrl-alt-l`).

* **Many Updated Command Palette Commands**  
Added many additional Commands to the Command Palette including many more markup operations, a number of toggle settings, and view options.

* **Add ShortCut Keys to the Web Lookup links in Link Dialog**  
The link dialog now has shortcut keys for the `Search on Web` (alt-s) and `Search and Link` (alt-l) buttons in the Link Dialog to allow for better hands free operation of these two operations.

* **Add Preview And Sidebar Width to Window Presets**  
The Sidebar Window drop down now allows for setting the preview and sidebar panel widths in addition to a Window size. 

* **Update Ace Editor to v1.5**  
Updated to latest Ace Editor version (1.5.1). Several small additional tweaks to the markdown editor behavior for the editor.

* **Fix: Format PipeTable with Line Breaks in Header**  
Fix issue where line breaks (via `<br>`) in Pipe Table headers was breaking the formatter and resulted in not being able to format the table or edit it in the Table editor. The change now formates based on the full single line instead of the individual line lengths which - assuming the table width is not too wide - will still nicely format a table even with linebreaks. <small>([#959](https://github.com/RickStrahl/MarkdownMonster/issues/959))</small>

* **Fix: GridTable with leading Spaces in multi-line Cells**  
Fix issue where multi-line cells in a grid table would strip leading spaces resulting in potentially lost markdown formatting. <small>([#961](https://github.com/RickStrahl/MarkdownMonster/issues/961))</small>

* **Fix: GridTable white space issues**  
Fix issue where white space is is not cleaned up properly when deleting lines from a cell in a gridtable. 
<small>([#962](https://github.com/RickStrahl/MarkdownMonster/issues/962))</small>

* **Fix: Preview not rendering first Mermaid Diagram**  
Fixed issue where entering a first Mermaid diagram into a page will not render until the page or tab is fully refreshed. This is due to the way MM caches the page and only replaces content, so when Mermaid is added after the page is loaded the script was not available to transform Mermaid charts. Added logic to explicitly check for Mermaid script and refresh page if not found. <small>([#960](https://github.com/RickStrahl/MarkdownMonster/issues/960))</small>

* **Fix: Display full Git Error Messages for Commit and Push Operations**  
Change display of error messages so the full message is displayed instead of the status bar only message which is often truncated. Display a message box instead so the full error can be captured. Important due to the new Git Security `safe.directory` features that require (`git config --global --add safe.directory <path>`)


#### Breaking Changes



* **Recommend a full Uninstall/Reinstall**  
The updated Roslyn support in version 2.5.5 and later changes a number of runtime dependencies and it's recommended that if you were running a pre-2.5.5 version you completely uninstall Markdown Monster and reinstall in order to clean the installation folder of old dependencies.

* **Razor Support removed for Code Snippet Templates**  
Razor language support has been removed from the Snippets addin as the new C# script syntax supports similar functionality for scripting. Razor has been problematic and adds a host of dependencies and inhibit future migration to .NET 6.0. To migrate you can move your scripts to the [Handlebars style C# syntax](https://markdownmonster.west-wind.com/docs/_5gs0uc49h.htm#text-with-c-expressions).


### 2.5

<small>May 10th, 2022</small>

* **[Command Palette](https://markdownmonster.west-wind.com/docs/_6b10l43hf.htm) (ctrl-shift-p)**  
You now have a command palette that lets you search for most operations and apply them. There's a searchbox in the Windows title bar, where you can type commands to find and execute them. While this doesn't replace native shortcuts it provides for additional discovery of available commands and options.   
*commands are still being added but most common functionality is available now*

* **[Folder Browser Markdown Preview Modes](https://markdownmonster.west-wind.com/docs/_6bw0k5a23.htm)**  
Added a Folder Browser configuration switch `FolderBrowser.MarkdownPreviewMode` that lets you choose between the previous `EditorPreview` mode that displays a transitory document that is closed when another document is accessed **unless the document has been changed**, or the new `HtmlPreview` mode which displays the rendered Html view. Preview mode is triggered by a single click in the Folder browser, while double click (or **Open in Editor**) opens the document as a normal fully editable document.

* **Improved Document Loading**  
Updated the document load sequence to improve performance and consistency loading documents. Most performance gains can be seen around navigation from the folder browser which reduces extra document open/close operations by now reusing tabs in some situations. Overall better performance and less document load flicker.

* **Improved Preview Document Loading in Folder Browser** 
When clicking at editable documents in the Folder browser, documents are initially displayed in *Preview Mode* which is a transient tab that disappears until a change is made or - now - you click into the preview mode document. Improved how these tabs are created and they are now reused to provide quicker and much smoother navigation of Markdown documents.

* **Clicking into Preview Document now switches to Full Document**
Previously preview document tabs required changing text in the document to trigger becoming a 'normal', full document. Now, clicking into the documentis all it takes to make it a full, non-transitory tab.

* **Improved Markdown List UI on Toolbar**  
Added support for Checkbox Lists. The 3 list options now show on  drop down with the main button still adding a plain list as before. The three options are:  **Standard List**, **Numbered List**, **Checkbox List**. Also improved handling of single line selections or no selections on line which now always add the list operator *to the front of the line* unlike before at the cursor position.

* **Remove Leading White space on Extra Markdown Features Menu**  
This option on the Extra Features Dropdown strips all common leading white space from a multi-line selection. This is useful for stripping off white space from code pasted from a code editor that has indentation or other text that might be otherwise indented. Removes white space that is common to all lines of text. (then again for code use `alt-c` code pasting do this and fencing automatically)

* **Sticky Search Subfolders for Folder Browser File Searching**  
The subfolder option for inline and explicit search pane searches is now sticky via a `FolderBrowser.SearchInSubFolders` configuration setting. Once updated it's remembered across MM sessions. [#934](https://github.com/RickStrahl/MarkdownMonster/issues/934)

* **Add Drag and Drop Links from Favorites**  
You can now drag favorites into a document and link to the file in the same way you can for the Folder Browser. Not super practical unless the favorite link is relative to the current file or project.

* **WebLog Custom Fields Context Menu**  
Added a Custom Fields context menu which draws values from a user defined menu of custom weblog post values (such as `mt_githuburl`, `mt_location`, `mt_date`) etc. These are typically user defined values that have special meaning on the server, but can be painful to remember if you're using them for custom values. You can now define any custom keys in the Weblog configuration as `WeblogConfig.CustomFieldNames`.

* **Make Spell Underlining Stylable**  
Add logic to allow customizing the way the spell check highlighting looks via `editor-user-extesions.css` and the `.ace_marker-layer .misspelled` style. Allows changing colors, thickness etc. as other page elements. This was previously hardcoded in the custom spell check logic MM uses.

* **Fix: Cleanup Folder Browser Markdown Document Navigation**  
Fixed several issues related to document navigation in the Folder browser that resulted in overly janky document opening and occasionally double opened documents.

* **Fix: Folder Browser Double Click Flashes and Occasional Load Failures**  
Fix issues related to double clicking in the folder browser that occasionally would cause documents to double load or fail to load and display a blank document.

* **Fix: Non-intended Drag and Drop Operations**  
Fix problem where slight mouse movements while clicking would trigger drag and drop operations that could cause accidental file moves. Widened drag minimums and fixed location pinning that was off and previously resulted in over eager drag operations.

* **Fix: Html Edit/Format Table Detection Failing**  
Fixed issue where Edit and Format Table context menu choices were not working when trying to open the Table editor. [#932](https://github.com/RickStrahl/MarkdownMonster/issues/932)

* **Fix: Table Editor with Pipe char in Pipe and Grid Tables**  
Fix bug where editing or formatting a Pipe or Grid Table that contains **escaped pipe characters** - ie. `\|`- would create additional columns rather than capturing the pipe character. Fixed.
[#933](https://github.com/RickStrahl/MarkdownMonster/issues/933)

* **Fix: MM won't launch if an older WebView Runtime is installed**  
MM by default won't launch if an older WebView runtime is installed and prompt for re-installing the latest WebView runtime. Usually this is not a problem but in some cases an older version may be installed and the app should be allowed to run (such as when running Canary builds). You can now override this behavior via a
`System.IgnoreWebViewVersionMismatch=true` configuration setting.

* **Fix: Toggle External to Preview Browser**  
Fix issue where the internal preview would not display when toggling from external to internal previewer. Previously you had to manually toggle the Previewer's visibility. Now external -> internal properly shows the internal previewer activated.

* **Fix: Footnote Link Navigation in Previewer**  
Fix issue with clicking on Footnote links and references not navigating the document to the footnote. Fixed. [#937](https://github.com/RickStrahl/MarkdownMonster/issues/937)

### 2.4

<small>March 10th, 2022</small>

* **[You Tube Embedding Window](https://markdownmonster.west-wind.com/docs/_69d0zwck0.htm)**  
You can now embed YouTube videos into a document using the YouTube Widget on the toolbar. You can paste a YouTube Url (watch, embed or shortcut), preview the video, set a title and default resolution, then embed it into the page as an HTML fragment. The HTML is formatted to auto-size both horizontally and vertically adjusted to the width of the document.

* **[Twitter Tweet Embedding Information](https://markdownmonster.west-wind.com/docs/_69e0rchvh.htm)**  
MM doesn't include special UI to embed Tweets into your content as Twitter provides an easy way to pick up ready to paste HTML that you can paste into a Markdown document. However to make this process more discoverable we've added a shortcut Tweet toolbar button in the Extension Tags dropdown of the toolbar. This button links to a help topic that describes the two steps to create an HTML widget on Twitter and paste it into markdown.

* **[Updated Gist Embedding Addin](https://github.com/RickStrahl/GistIntegration-MarkdownMonster-Addin)**  
Although external, this add in is used by quite a few people. The addin has been updated with a few UI updates to make it quicker and easier to use. You can now also copy a Gist id or script tag for existing Gists, delete Gists. Save to and Load From Gist also have a host of updates to make it easier to access these options from the Gist Listing view.

* **Added Markdown HtmlEncode and UrlEncode Shortcuts**  
You can now easily HtmlEncode a block of text or UrlEncode a value using shortcuts on the Extended Markdown Operations dropdown from the toolbar.

* **Alt-X Shortcut for dropping down Extended Markdown Features**  
There's new Alt-X (default) shortcut key that drops down the Extended Markdown features from the toolbar for quick access. This menu has things like Upper/Lower Case, bolditalic, HtmlEncode, UrlEncode etc. Also updated the [documentation for Markdown Monster Shortcut keys](https://markdownmonster.west-wind.com/docs/_4rd0xjy44.htm).

* **Improved Markdown Quote Handling (ctrl-q)**  
Quote handling (`> content`) now better supports single line or no selections, prepending the quote mark at the beginning of the line. MM now also checks for already quoted lines and doesn't double quote any longer.

* **[Command Line Opening of Files using `filename:lineno` Syntax](https://markdownmonster.west-wind.com/docs/_5fp0xp68p.htm#editor-ui-commands-mm.exe)**  
In addition to the `--lineno` command line switch you can now also use `:lineno` at the end of a filename to open a file at that line. Example: `c:\temp\test.md:22`. The individual file lineno overrides the `--lineno` parameter which works only against the first opened file.

* **Support For Better Html Document Previews**  
There's now better raw HTML document editing support (ie. `.html files`) in Markdown Monster as previews now show related resource content. You get many of the same live preview benefits that are also available with Markdown documents. Images, styles, scripts and other related assets now correctly load in the previewer for HTML documents via an inject `<base>` tag that points back to the document's host folder. [#907](https://github.com/RickStrahl/MarkdownMonster/issues/907)

* **Addins: Fix Addin Repository Urls**  
Due to changes at GitHub related to branch names etc. we've had to change the way addins report their default Urls. The new Urls require providing a branch name (ie. `/tree/main` etc. suffix to repo). This is required since we can no longer assume a `master` or `main` branch.  
**This is a breaking change** - the new addin repository Urls break old applications so for older v2 versions the Addin Manager is broken. *Please update to latest*.

* **Addins: Add AdditionalDropdownMenuItems to AddinMenuItem**  
You can now add additional menu items to the Addin drop down menu on the Toolbar. This allows addins to be more obvious about features available by the addin **in one place**, in addition to optional integration into the Main Menu (which requires a little more work).

* **Addins: New `OnApplicationInitialized()` Handler**  
This handler replaces the `OnModelLoaded()` handled which more clearly identifies the purpose of this handler. This method is now set up to be the default Addin configuration method where the default Id, name and menu item configuration is placed instead of in `OnApplicationStart()`. The problem with `OnApplicationStart()` is that it fires before there is any app context - no Window, no Dispatcher, no Model. `OnApplicationInitialized()` ensures the Window is instantiated, a Dispatcher is available and the `AppModel` is available.  

* **Addins: Updated `dotnet new` and Visual Studio Addin Project Templates**  
We've updated both types of templates in line with the changes for `OnApplicationInitialized()` and cleaned up the templates and added additional comments to the generated addin and configuration classes.

* **Updated: [Mermaid](https://markdownmonster.west-wind.com/docs/_5ef0x96or.htm) and [MathMl](https://markdownmonster.west-wind.com/docs/_59l0mv2uw.htm) now work without requiring Allow Script Rendering**  
These two RenderExtensions provide diagram and math equation rendering into Markdown now work without explicitly requiring the `AllowRenderScriptTags` option to be set as they don't actually require JavaScript code inside of the rendered Markdown body any longer. They are still disabled by default but can now be enabled via just the `UseMermaid` and `UseMathematics` configuration settings. A restart is required for changes to these values as the RenderExtensions need to be reloaded.

* **Fix: Re-enabled the [Microsoft DocFx Markdown Parser](https://markdownmonster.west-wind.com/docs/_5750qtgr2.htm#the-official-microsoft-docfx-markdown-parser)**  
We temporarily had to remove the Microsoft DocFx parser, due to a dependency version conflict with the MarkDig parser. Now that MarkDig versions have been re-synced to the latest versions the `DocFx` Parser is available again from the Markdown Parser dropdown on the toolbar.

* **Fix: Focus with New and Non-Existing Documents from Command Line**  
Fixed focus issues for opening a new or non-existing document from the command line. Focus now starts in the editor.

* **Fix: Presentation Mode preserves Folder Browser Sidebar Visibility**  
When you use Presentation Mode to toggle between edit and presentation modes, MM now remembers and restores the preview state of the left folder browser sidebar.

* **Fix: GridTable LineFeed Issues**  
Fix Grid table edit and format table inputs when table cells have empty lines. These empty lines are no longer stripped. Fix extra linefeed at end of generated GridTable output/paste operations.
[#901](https://github.com/RickStrahl/MarkdownMonster/issues?q=is%3Aopen+is%3Aissue)

* **Fix: `markdownmonster:` Protocol Handler with text**  
Fix bug where the `markdownmonster:untitled.base64:<data>` handler was not assigning the document data passed into the newly opened document. Fixed.

* **Fix: GridTable Parsing with Back to Back Tables**  
Fix Grid Tables when tables are butted up against each other without separating lines. Note: This is legal but the preview won't actually render it and most Markdown parsers fail to render this correctly. Although the editor now supports this functionality, it's best to use a blank line between two tables to ensure it renders correctly regardless of parser. *[#904](https://github.com/RickStrahl/MarkdownMonster/issues/904)*

* **Fix Mermaid Rendering**  
Fix Mermaid rendering for certain Mermaid content by Html Encoding the body to render. Previously the unencoded text would fail to render correctly. Encoding is applied only the `mermaid` sections, not the raw Html `<div>` which is used as is meaning that user is responsible for encoding. [#911](https://github.com/RickStrahl/MarkdownMonster/issues/911)

### 2.3 

<small>January 14th, 2021</small>

* **[Support for Long Path Names](https://markdownmonster.west-wind.com/docs/_68d0r8rej.htm) (if enabled in Windows)**  
Added improved support for long path names in MM via manifest setting that allows you to open and save documents and assets with paths longer than the Windows `MAX_PATH` (255 chars). It now also works for many of the external application integrations. For this to work, Long Path Names have to be enabled in Windows (10/11) via registry or group policy setting.

* **Improved Document Load Time**  
We've improved the way editor and preview documents get loaded, which now load faster, with less flicker and without the occasional brief document load error page displayed.

* **Add File Updated and Created to Folder Browser Tooltip**  
Files and folders now display both created and updated dates in the File and Folder Browser.

* **Document Tab ToolTip now includes File Information**  
The document tab tooltip now displays file information including file size and updated and creation times, using similar format to what's used for tooltips in the Folder Browser.

* **Move Symbol Configuration for Italic and Soft Line Breaks Characters**  
Moved the Symbol configuration that allows you to specify what symbols to use for italic (`*` or `_`) and Soft Line Breaks (`  ` or `\`) into the main Markdown configuration so it's visible in the interactive editor. Previously these two values were nested in a sub key below the Markdown configuration and not visible in the editor. These symbols are used by the Toolbar/Shortcut insertion operations.

* **Improved Drag and Drop Operations in the Folder Browser**  
Updated drag and drop logic that affects initial drag state activation to be less sensitive resulting in fewer accidental drag operations. Also fixed various selection and drop issues as well as better supporting dropping shell files into the Folder Browser.

* **Fix: Folder and File Visibility when a Filter is Set**  
Fix issue where new files or folders added would not respect the search filters and show up regardless of the filter settings.

* **Fix: Lockups on opening Documents**  
Fixed issue with lockups when opening documents in some situations such as after search, dragging items. 

* **Fix: Document Title Display with Full Path when switching from Preview** Fix an issue when switching from a Preview document to an edited document where the tab title would display the path even though there's no duplicate item. Correct behavior is to display only the filename when a single file with that name is open.

### 2.2

<small>November 23rd, 2021</small>

* **Import from JSON to Markdown Table**  
You can now import JSON object arrays as Markdown tables from file or the clipboard. Field names are mapped to headers values as row content. An optional field exclusion list can be applied.

* **Move Table Rows Up and Down**  
You can now move table rows in the Table Editor, up and down via context menu or `alt-up` and `alt-down`.

* **Keyboard Navigation for Common Table Editor Cell and Row Operations**  
You can now use keyboard navigation for moving rows up and down (`alt-up`, `alt-down`) and moving columns left and right (`alt-left`, `alt-right`). You can also insert rows above and below using (`alt-shift-up`, `alt-shift-down`).

* **Copy To Clipboard for Markdown Tables in Table Editor**  
The Markdown Table Editor has a new Copy button to copy the currently active table in the editor to Markdown and the clipboard. This allows using the editor for easy pasting of Markdown Tables into other applications.

* **mmCli cleanup-webview Command to clear WebView Environment**  
Added command line helper to clear out the WebView Environment that MM uses. The environment is private and separate from global settings, and only used for MM's local rendering of generated content.


* **Spellchecking: Look up on Web**  
The spell checking context menu now has an option to **Lookup on Web** when a spell checking error is found. This is useful if you have no match in the list of suggestions and need to find out correct spelling. Opens Search page in your specified search engine.

* **Add Neeva and Brave to configurable Search Engines for Lookups**  
Add the Neeva and Brave search engines to the list of supported search engines for various lookup operations. Look up operations are available for finding and embedding URLs, and the spell checker for example.

* **Copy Favorites Path to Clipboard on Favorites Context Menu**  
You can now copy the current favorite item's path to the clipboard from the Favorites Context Menu.

* **Command Snippets: Warm up C# Compiler**  
If you're using snippets that contain C# code (Expressions or Razor) there's now a configuration option that allows preloading the C# compiler to speed up first compiled snippet operation. Also fix code focus delay issue with Snippet insertion.

* **Fix: Update RelativePath Processing**  
Fix exceptions in Relative Path creation used for dropped and pasted files and images. Fix issue where invalid paths would cause a hard failure due to Path object exceptions.

* **Fix: Favorites Intra Section Drag and Drop**  
Fix issue where dragging a Favorite in section would not 'stay' in the new drop location and revert after short delay.

* **Fix: Control Tab Focus Handling**  
Fix various issues with Control Tab locking the UI and not focusing the cursor. Fixes Ctrl-Tab and Click focus with cursor becoming active in editor for each.

* **Fix: PDF Generation Intermediary HTML File Name**  
Fixed the intermediary HTML filename used when generating PDF files. Previously the HTML file generated was the same name as the PDF file, which could cause conflicts if the HTML file already existed prior to generation. Added `__` prefix to the temp file.

* **Fix: SnagIt Image Capture Embedding**  
Fix issue with SnagIt image embedding from the Screen Capture add-in. Previously, save operation failed silently. Fixed.

* **Fix: Physical Path Formatting for Links and Images**  
When embedding physical file locations into Markdown (in untitled documents mainly or if referencing non-relative locations) paths previously where using Windows style syntax with forward slashes. This worked previously but started failing recently with the WebView renderer. All paths - both physical and relative - are now embedded using URL style forward slashes even for Windows paths.

* **Update: Log more Exception Data**  
Exceptions now log both the top level and base exception to provide a little extra info on failures.


### 2.1

<small>October 12th, 2021</small>

* **Output Generation icons on main Toolbar**  
Added PDF, Print and HTML Output icons to the toolbar to make these features more discoverable and more easily accessible with a single click.

* **Basic Syntax Detection for Pasted Text into Empty Untitled Documents** When you paste content into a new `untitled` document, MM now tries to detect syntax and automatically sets the editor's syntax to this mode. This only applies if the document is empty otherwise the syntax is sticky.
 
* **Remember Table Embedding Mode in Table Editor**  
When you embed a table using the Table Editor window, your last selection is now preserved in a configuration setting `Editor.TablePasteMode`.

* **Set File Sort Order in Folder Browser**  
The context menu in the folder browser now has an option to set the file sort order to Name, Date Ascending and Date Descending. The file order setting is tracked via sticky configuration in `FolderBrowser.FileOrder`.

* **Optional File Browser File Tooltips**  
You can now optionally enable a new `FolderBrowser.ShowToolTips` configuration option to show file information when hovering over files and folders. The tooltip shows the file's full name, parent folder, file size and last write date.

* **PDF specific Preview Theme**  
Added a **Pdf Output** Preview Theme that can be used when printing output to PDF. This theme is specifically designed to work around the differences in PDF output which include printing code with non-syntax coloring, and special font handling in order for code snippets to display properly. You can copy and customize this theme as necessary.

* **PDF Output now lets you set the Theme**  
When you create PDF output via the **Save To -> Save to PDF** dialog, you can now select the theme that is used to render the document. Due to limitations of the PDF rendering we recommend you use the printing specific **Pdf Output** theme which is the default, but any theme will work and you can create your own custom themes for printing.

* **Change: Markdown Link Rendering**  
Changed Markdown link rendering to remove underscore using regular display to make it easier to read links that might have underscores in them. Links now display without link underscore, and only show the underscore when hovering to indicate that the link can be navigated (via Ctrl-Click). There's now also a tooltip to notify you of ctrl-click behavior. 

* **Update: PDF Generation Tools**  
Update to latest tool version of wkhtml2pdf for PDF generation which fixes a number of small rendering quirks in the PDF generation engine. Also added some rudimentary support for emoji rendering in PDF documents since that seems a common theme.

* **Update: [Markdown Monster Addin Templates](https://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm)**  
Both the Visual Studio and `dotnet new` Markdown Monster Addin templates have now been updated to support v2 Addin projects. The addin interfaces have changed to mostly `Task` based `async`, to make it easier to interact with the async document functionality required for many editor interactions. If you need to update a v1 project to v2, you will need to convert the main addin entry points to updated async signatures (`Task` or `async Task`) instead of the old `void` method types.

* **Fix: Alt Key Handling**  
Fixed a number of issues around `alt` key handling. Fixed issue where some editor commands like `alt-shift` and `alt-ctrl` selection weren't working. This is due to the custom menu activation handling which requires `alt` plus a small delay to trigger now vs. simultaneous key press for `alt` key chords. Editor alt chord commands are expected to be **simultaneous** press operations to work. This fixes block selections, block column selections and a host of other alt key based editor operations.

* **Fix: Favorites Tree Data Entry**  
Fix various issues with the Favorites list where entering a new item would add two items and focus would switch unpredictably. You can now also navigate the list with the keyboard with `Enter` opening the item with editor focus and `Space` opening without editor focus.

* **Fix: Allow Render Scripts State Switches**  
Fix issue where switching the allow render state flag on the menu or in settings wouldn't affect the actual rendering and still allow rendering scripts and `iframe` when rendered. Fixed binding for menu, and updated the refresh mechanism so that open documents are updated with the new setting and refreshed (from menu update).

* **Fix: Startup Menu Folder Opening**  
Fix issue Startup Screen folder opening issues where clicking on the folder icon next to the file, would not reliably open the Folder Browser. Fixed.

* **Fix: Tabs not auto-activating when loading from CLI**  
When loading files from the command line files would not open properly in the editor, with tabs opened but not activated and showing an empty preview. Fixed.

* **Fix: Search Settings in Settings Dialog**  
Fix issue with Search Settings entry which was hanging up and not immediately refreshing the entered text. Reduced delay and ensured that operation occurs in dispatched mode to see the change.

* **Fix: WordPress Publishing Thumbnail Image Failure**  
Fix issue with the WordPress publishing mechanism where if thumbnails are published the addin would fail due to a data type error.

* **Fix: EnableBulletAutoComplete**  
Fix this setting so it works on initial document load. Previously the setting only worked after the document was 're-activated'. Fixed.

* **Fix: Edit CenteredMode Pixel Width in Menu**  
Fixed focus issue in the Centered Mode editor document width that determines how wide the editor column to actually edit is. This allows creating of white space around text if the size of the overall edit pane is larger. Fixed issue where focus could not be set easily in the menu option, which made it hard to edit the value. Fixed.

* **Fix: Configuration Editor Click to Jump to Json Editor Links**  
Several configuration options that edit a collection of items, have links in the configuration editor. These links now properly open the JSON configuration file and jump to the appropriate section in the JSON document. Also fixed focus issue for JSON activation by closing the visual editor.

* **Fix: Remember last document location in Recent Documents**  
Last document location was no longer saving when documents were closed (due to async changes). Value is now picked up again before saving and applied when recent documents are opened.

* **Fix: Adding links as Link Collections**  
Fixed issue where **Add Link Collection** in the Paste Link dialog was failing and not producing a link.

### 2.0.5 - Official release of v2.0

<small>July 20th, 2021 &bull; [Release Blog Post](https://weblog.west-wind.com/posts/2021/Jul/22/Markdown-Monster-20-is-here)</small>

* **Use WebView2 and Chromium for all Web Rendering including Editor**  
Markdown Monster now uses a Chromium based browser for **all Web rendering** including the Editor, the Preview, the Table Editor, Code Windows, and Browser Dialogs. Previously only the Preview was optionally enabled by using the Chromium Preview addin. The Addin is no longer needed as all content always uses the Chromium engine. This improves rendering fidelity and also provides better responsiveness due to asynchronous rendering of content which allows for larger content to be displayed and synced while typing.

* **Table Editor Improvements**  
With Chromium rendering a number of odd IE browser bugs are fixed that affected navigation and selection in the old version. Table Editor can now **Move Columns** to the left or right.

* **64 bit Application**  
With the removal of the IE based Web Browser control, Markdown Monster can now run as 64 bit application again. 32 bit mode is still possible on 32 bit systems as well.

* **Allow Swapping Editor and Preview Location**  
You can now swap the editor and preview location via a new **View->Swap Editor and Preview Location** menu option and a via Editor/Preview Splitter Context Menu.

* **New Splitter Context Menu**  
Added a new context menu that displays options for swapping editor and preview, entering presentation mode and toggling the preview display.

* **[Track Active Document in Folder Browser](https://markdownmonster.west-wind.com/docs/_4wu1cjyka.htm)**  
As a heavily requested feature, we've added support for optional document tracking in the folder browser. Using the `FolderBrowser.TrackDocumentInFolderBrowser` configuration switch (also via  a toggle button in the Folder Browser) any time you change the document the Folder Browser navigates to that file.

* **Improved Folder Browser Navigation**  
Folder browser navigation now shows previews for most text type documents in 'inactive' mode that is temporary until the next document is accessed. Documents become 'active' once you edit the document or double click to explicitly open for editing. Single click now also previews any non-edit formats externally, like PDFs, Office docs, etc. Executables open selected in Explorer but are not executed. Drag and Drop start operations are now less twitchy. 

* **Move Support Binaries out of Root Folder**  
Support binaries are now moved out of the root folder into a `BinSupport` sub folder to avoid ending up on the User's path and causing naming conflicts. Only applications that should be visible on the user path now are: `MarkdownMonster`, `mm` and `mmcli`.

* **Make Settings HelpText Selectable**  
You can now select the help text associated with a configuration setting in the Settings window. This allows picking up URLs and other fixed values more easily. (#817)

* **Dev: Add Debug Editor and Preview Template Paths**  
Added configurable Editor and Preview Template paths that are configurable and allow pointing the template folders to the original development folder, rather than the deployed applications' folders. This allows making changes to the Html/Web templates without having to recompile code. Settings are `System.DebugEditorHtmlTemplatesPath` and `System.DebugPreviewHtmlTemplatesPath` and they default to `.\Editor` and `.\PreviewThemes` which are fixed up at runtime.

* **Fix: Remove WebViewPreviewer Addin from 1.x Installs**  
Added logic to remove the WebViewPreviewer Addin from v1 which throws an error. If found this addin is now explicitly removed at startup since the code has moved inline.

* **Fix: PDF Generation Errors**
Fix issue where repeated output to PDF would report an error for PDF generation even when the PDF was generated.

* **Fix: PDF Code Snippet Background**  
Fix issue where the PDF output for code snippets was not properly applying the background color. Works in HTML but not for the PDF generator. Added custom print style sheet overrides for `pre > code` style explicitly to match highlightjs color theme.

* **Fix: Folder Browser Click and DoubleClick Behavior**  
Fix issues where clicking would not allow keyboard navigation after click, folder opening wasn't opening folders on first click, and preview operations could hang.