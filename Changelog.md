# Markdown Monster Change Log 
 
[![download](https://img.shields.io/badge/Download-Installer-blue.svg)](https://markdownmonster.west-wind.com/download.aspx)
[![Chocolatey](https://img.shields.io/chocolatey/dt/markdownmonster.svg)](https://chocolatey.org/packages/MarkdownMonster)
[![Web Site](https://img.shields.io/badge/Markdown_Monster-WebSite-blue.svg)](https://markdownmonster.west-wind.com)

### 1.20.7
*<small>not released yet</small>* 

* **Add *Open Document in new Tab* to Context Menu for Relative Markdown Links**  
There's a new context menu option that lets you navigate relative Markdown links by opening them in a new (or existing) editor tab. Supported file types are opened in an editor tab, everything else is opened the Windows default viewer. HTTP links are opened in the browser.

* **Clickable Links in Editor**  
Links are now clickable in the editor by Control Clicking which displays the link in the appropriate editor. Hyperlinks are opened in the browser, supported documents are opened in the editor and any others are opened in the appropriate Windows editors.

* **Remove Code Copy Badges in PDF/Print Views**  
The code badges were overlaying the print content and since there's no transparency and you can easily scroll code, the badges are superfluous and would obscure content below it. Removed for PDF and Print output.

* **Code Badge Copy Code Linefeeds in Previewer**  
Previously the Code Badge copying would not properly handle line feeds in code snippets. It worked fine for external preview, but for the IE preview the line breaks were lost. Fixed.

* **Code Badge Scolling Fix**  
Fix issue with the code badge positioning when the code block is scrolled. Previously the code badge failed to stay pinned to the right in the scrolled content. This fix keeps it always pinned to the right of the code block.

* **Fix: Two-way Code Editor Preview Sync Jumpiness**  
Resolved another issue in two-way sync preview mode that was causing the editor to jump when editing or pasting into large blocks of text or code at the top or bottom of the editor. Finally found a solution to separating actual scroll events from explicitly navigated scroll events and refresh operations.

* **Fix: HTML Rendering and Preview Sync**  
Fix preview HTML editor wonkiness (#609). Refactor HTML document sync separately from the Markdown document sync for greatly improved HTML preview sync to the editor position when editing or scrolling HTML content.

* **Fix: Remove Depedendent ShimGen Exes from Distribution**   
Removed extra EXE files from the distribution for the Roslyn compiler and set up Chocolatey to not generate ShimGen files for the remaining non-MM exe files (pingo, wkhtmltopdf, vbcscompiler).

### 1.20.5
*<small>December 13, 2019</small>* 

* **Add Toolbar DropDown for Additional Editor Operations**  
The main toolbar now has a new dropdown at the end of the editor operations, that provide additional editor insertion operations that are less common but were not discoverable before. Added operations: `<small>`, `<mark>` and `<u>` (underscore) as well as inserting a Page Break (for PDF generation or printing).

* **Fix: Document Outline not closing when closing last document**  
Fix Document Outline issue when closing the last document where the Outline stayed active after the document was closed.

### 1.20
*<small>November 8th, 2019</small>* 

* **Add Hotkeys to the Table Editor Context Menu**  
The table editor context menu now has shortcuts for all operations like Insert Row Above, Below and Insert Column left and right, as well as delete row and column. This makes the options hotkey enabled via `ContextMenu Key + B` for example for Add Row Below within a cell.

* **Format Table Editor Context Menu Option**  
In the Markdown editor you can now use the context menu over a Markdown or HTML table and re-format that table using the new **Format Table** context menu option.

* **Improved Preview Scrolling**  
We've tweaked the preview scroll behavior which should now result in better consistency when scrolling the editor as well as less latency between editor and preview in two-way sync mode. The overall change involves trying to keep the 'in-focus' content near the top for the synced editor or preview so it's easier to track relevant content in one consistent place. Click sync and cursor movement sync scrolls the current cursor position content near the top of the preview. Also fixed several issues that could on rare occasion 'bounce the editor' when the preview and editor refresh out of sync. Preview scroll should now be much more responsive for both editor->preview and preview->editor scrolling. 
* **Improved Spellcheck latency**  
Reduced latency in the spell checking algorithm by checking the document more frequently to avoid jarring highlight movement. Change marker update logic to remove old markers only after new ones have rendered which removes/reduces flicker.

* **Add Copy Code and Syntax Display to Generated HTML Code Snippets**     
Code snippets in the editor and exported to HTML (in the Preview or if exported) now show a transparent badge that allows copying the code to the clipboard with a simple button click. The badge also shows the syntax in use if any.

* **Add Open With to the Editor Context Menu**  
Add a new context menu option to **Open With...** that allows opening the current document in a different editor configured on the system.

* **Open Settings Folder in Configuration Window**  
The Configuration Settings window now has an additional button to quickly open the configuration folder where you find all related configuration files. Same as the Tools menu option.

* **Add Reset Button to the Settings Form**  
The Settings form now has an additional toolbar button to reset the Markdown Monster installation to installation defaults. Clicking the button backs up the configuration file and then resets all configuration settings to default, followed by a restart.

* **Update Application Theme Changing and Toggling**  
Fixed a few issues related to switching between light and dark themes. Application now properly restarts after switching or toggling themes. The application theme toggle now sits more noticeably on the top window bar to make it easier to find for new users.

* **Fix: Preview Sync Problems with Two-way Synching at bottom of Document**  
Fixed issue where in some cases the cursor would jump up from the bottom of the document when doing two-way preview syncing between the editor and the previewer.

* **Partial Fix: Display Bold and Italic in editor**  
Fix behavior of markdown text that uses both bold and italic in the editor: `***bold and italic***`. Text now correctly displays in editor as both text and italic. However, mixed mode like `_**bold and italic**_` still don't work in the editor. Note that all combinations do work for Markdown output generation.

* **Fix: Table Editor Insert New Row with Tab at End of List**  
Fixed tab behavior on the last column of the table: When pressing Tab on the last cell a new row is inserted and the cursor moved to the first column of the new row. This actually was supposed to work before and a new row was being inserted, but the UI wasn't properly refreshing. Fixed.

* **Fix: Table Editor Column Width Formatting when Embedding**  
Fixed regression that removed the column sizing logic that attempts to fix table widths to make the tables line up properly.

* **Fix: Alternate Single Quotes and Spellchecking of Apostrophied Words**  
Fixed issue where spell checking wasn't working correctly with 'special' single quotes (SmartyPants or imported from something like Word). Fixed by replacing special quotes with single quotes for handling apostrophied words.

* **Fix: Document Outline Refresh** 
The document outline refresh previously was to conservative in refreshing. Added logic to every editor preview refresh to check for outline updates. Tested with very large documents to ensure there's no major performance hit.

* **Fix: Multi-Binding KeyBoardBindings**  
Fixed issue where multiple keyboard bindings to the same command were not properly firing all the commands. Changed keybindings handler to use IDs as names with command names separate. Any duplicate `CommandName` entries, should use separate `Id` values.

* **Fix: Save Fails Silently**  
Fix bug where a failed Save Operation would fail to save files and not let you know that the save failed. Fixed - if file save fails there's now a status bar message and the Save As dialog pops up to provide a new filename or try again.

* **Fix: Document Outline Editor Navigation**  
Document outline navigation now locates the navigated editor at the top of the editor page. Previously it was closer to the middle and somewhat erratic. This is related to the previews point about more consistent scroll behavior that pushes the in focus content to the top of the preview or to the editor depending on which window you are scrolling.

* **Fix: MathJax and MathML Rendering**  
Fix bug that wouldn't properly auto-detect documents that contain math expressions. Fixed search logic and tweaked RenderExtension with some additional improvements.

* **Fix: Version Check Last Check Date**  
Fix version check shutdown logic to properly save the last check date.

* **Dev: Update to ACE Editor 1.4.6**  
Updated to the latest version of ACE Editor which fixes a few small bugs that have been plaguing the editor namely fenced HTML code block tag lock ups/slowdowns and end of document caret movement.


### 1.19 
*<small>September 4th, 2019</small>*

* **[New Configuration Editor](https://markdownmonster.west-wind.com/docs/_4nk01yq6q.htm)**  
Added a VS Code style Configuration settings editor UI, that allows searching for settings. It also prevents entering invalid JSON data into any non text fields that expect specific values. If you loved the JSON based configuration, not to worry: You can still edit the raw JSON to make settings changes too. 

* **New Find in Files in Folder Browser**  
Added Find in Files functionality that lets you search the active folder structure in the open Folder Browser folder. You can search filenames and content with partial matching. Access via **Edit -> Find in Files** or via **File Browser -> Search Icon**.

* **Pre-Configured Window Size Dropdown**  
The Control Box now has a dropdown button, that lets you select a pre-configured Window size and resize your window to one of these sizes. The list is customizable and you can modify or add your own sizes or add the current window size to the list. Resizing is smart enough to create windows that fit onto the active screen and translates for high DPI modes.

* **Add Visual Studio Code Light Theme**  
Add a new Visual Studio Code Light theme that's similar (but not identical) to the Visual Studio Code Markdown theme colors. Also adjust  colors for the Visual Studio Dark theme. 

* **Add Configuration Backup**  
You can now backup your Markdown Monster configuration to Zip file or to a disk folder. This feature writes out all files from the configuration folder into the zip or folder as a backup mechanism.

* **[Open Document File Change Monitoring](https://raw.githubusercontent.com/RickStrahl/ImageDrop/master/MarkdownMonster/OpenDocumentFileChangeTracking.gif)**  
The open document now actively tracks changes that are made externally and updates the document immediately if the document in the editor has no changes. Changes are reflected even when the editor is not active, so external changes can be seen updating. As before, if the document has changes, updates are held off until you activate the document, at which point you get a prompt on whether to reload, or keep the editor document.

* **Improved External Preview Window Configuration**  
The preview window now has additional options for managing the Window Window stack order including synced to main window, always on top or manually activated.

* **Add explicit Editor Linefeed Format**  
You can now explicitly specify the linefeed format of the editor in the Editor configuration. Previously MM always used Unix style `Lf` formatting (the default for ACE Editor). You can now specify `CrLf` for Windows specific formatting. This setting affects how files are saved and how Copy/Cut/Paste works. The default remains at `Lf` only since that appears to be the more compatible format that works for almost everything.

* **[Additional Edit Toolbar Icons on Toolbar](https://markdownmonster.west-wind.com/docs/_5im10bjpw.htm)**  
You can now add additional toolbar icons via configuration in `Editor.AdditionalToolbarIcons` by using additional built-in toolbar commands as well as custom HTML or markup tags that wrap selected text. Also added new `MainWindow.AddEditToolbarIcon()` that allow addins to easily add toolbar buttons.

* **Double Click on Empty Tab Header to open new Document**  
You can now double click the empty tab header to open a new document.

* **Additional Languages for Code Highlighting**  
Added Dart, Kotlin, Nginx and Apache as additional syntax languages to display in the preview.

* **New Document Default Filename in Folder Browser**  
When you create a new file in the Folder browser it now defaults to `README.md` (or `NewFile.md` if it exists) and pre-selects the file stem portion of the file name for quick typing. Related: Fix issue where escaping didn't always clear the newly typed filename when aborting a new file operation (Shift-F2) in the Folder Browser.

* **Support for Cut, Copy, Paste of Files in Folder Browser**  
The folder browser now supports cut, copy and paste operations for files both for files from the folder browser as well as to and from the Windows Shell/Explorer.

* **Add Support for Text to Speech of Editor Text**  
You can now use **Edit->Speak** to speak the current selection, the entire document or text from the clipboard using the Windows Speech API.

* **Support for setting Document Encoding**  
You can now set document encoding to UTF-8 or Unicode encodings on your documents. Previously new docs defaulted to UTF-8 with BOM and for other documents respected and maintained existing encodings.

* **Fix: Multi-Monitor Location Preservation**  
Fixed a number issues related to multi-monitor positioning when restoring settings. Maximized windows now restore to their previously un-maximized position when restored.

* **Fix: DPI Sizing of Editor MaxWidth**  
Add DPI adjustment for scaled displayed in the Editor MaxWidth setting for centered view in the editor. This avoids the problem of the display 'shrinking' on scaled displays.

* **Fix: Presentation Mode from External Preview**  
Fix issue with the Presentation View when activated when the external preview is active. New behavior switches to internal preview before activating presentation view.

* **Fix: Preview of inline, wrapped Code Style in various Themes**  
Fixed display of inline code that wraps across multiple lines in the GitHub and Medium styles. These styles were previously set to not wrap causing long lines for long `<code>` content.

* **Fix: No menu focus after closing last Tab**  
Fix issue where focus is lost after closing the last tab in Markdown Monster.

* **Fix: Ctrl-Tab/Next Tab navigation Focus Issues**  
Fix problem with Ctrl-Tab navigation of tabs and Ctrl-F4 tab closing operations losing proper tab focus.

* **Fix: Git Show in External Diff Tool**  
Fixed bug that wouldn't show diffs for files in subfolders of the tree.

* **Fix: Html Entity Display in Document View**  
Fix bug with HTML Entities in headers in the Document View. Headers now properly decode HTML entities and capture the entire content.

* **Dev: Refactor Support Editor Usage**  
Internally consolidated the internal editor usage for various editor operations like Code blocks and Snippet Templates to use the same HTML editor and script code as the main editor. The JavaScript client editor selectively disables features not used/usable for the 'EditorSimple' implementation.


### 1.18
<small>June 17th, 2019</small>

* **[Project Support](https://markdownmonster.west-wind.com/docs/_5i51e89dw.htm)**  
You can now save the open document collection and folder browser configuration as a project file (`.mdproj`). At a later point you can re-open the project and restore all the documents, document positions and the folder browser to the last point the project was saved. Once opened a project stays open and can be easily re-saved.

* **Open in New Window**  
Added context menu option on the editor tab to open a new instance of Markdown Monster with the current document loaded. This makes it easier to display multiple windows side by side for copying content between them.

* **Support for DocFx Note/Warning/Tip/Caution etc** 
Move DocFx Processing to DocFx Render Extension and add support for handling DocFx Note Blocks and DocFx Include File operations.

* **Renamed `ParseDocFxIncludes` to `ParseDocFx` Config Setting**  
Renamed the `Markdown.ParseDocFxIncludes` setting to `Markdown.ParseDocFx` to indicate that multiple DocFx features are rendered.

* **Pick up Hash Links from Clipboard in the HREF Dialog**  
In addition to automatically picking up URLs from the clipboard, the URL Dialog now also automatically detects and fills in Hash links (`#link-id`) for quick fill operations. You can use `Ctrl-K -> Ctrl-Enter` to quickly embed links if links or hashes are on the clipboard.

* **[Drag and Drop Documents between Instances](https://markdownmonster.west-wind.com/docs/_5i10rquxc.htm)**  
You can now drag and drop documents between two open instances of Markdown Monster in addition to the new **Open in New Window** feature introduced in the last update.

* **Refactored File Menu**  
Broke out **Open From**, **Save To** and **Project** options into submenus to reduce clutter in the File menu.

* **New Documents open at the front of the TabList**
Changed behavior that opens new tabs at the beginning of the list of tabs. This makes the newly opened documents more prominent if many tabs are open.

* **Open common files in the Previewer with Shell**  
If you link to local files in the editor MM now opens many common file formats in their respective applications. So a `.docx` file opens in Word, a .zip file in 7zip or WinZip. Some files are opened in Explorer and highlighted.

* **Improved Hash Navigation in the Previewer**  
Hash navigation is now intercepted more effectively and navigates the previewer and editor when you click a link. You can also use the context menu on a link in the Previewer to explicitly navigate to its hash source in the editor.

* **Show Link Id in Document Outline Header Tooltip**  
Document headers now show a tooltip for the link Id as preview of an Id that you can copy from the context menu.

*  **Add Copy Id to Clipboard in Preview Browser**  
The Preview Browser context menu now has an option to capture the document id of the element under the cursor and to copy that value to the clipboard as `#doc-id-value`.

* **Add Edit Image in Image Editor to Preview Browser**  
The preview browser now lets you right click on an image in the editor and just to the configured image editor to edit the image.

* **New `-newwindow` Command Line Option**  
To make Open in New Window work, a new `-newwindow` Command Line option to force a new Markdown Monster instance to open regardless of the `SingleInstance` configuration option. When this option is used the window is opened with a single document and does not re-open previously opened windows.

* **New `-nosplash` Command Line Option**  
You can now explicitly disable the splash screen via startup command. This startup switch overrides the `DisableSplashScreen` configuration setting.

* **[Addins: Updated RenderExtension Interface for Addins](https://markdownmonster.west-wind.com/docs/_5i30sba89.htm)**  
Addins now get a much simpler `IRenderExtensions` interface that makes it easy to create an addin that provides customization to the HTML output by inspecting either the pre-render Markdown or post-render HTML.

* **Addins: Consolidated Window.OpenFile() to Open Documents Generically**  
Addins have a new `Model.Window.OpenFile()` method they can use to open all sorts of documents in a generic fashion. Unlike `OpenTab()` which explcitly opens a new document tab, `OpenFile()` can open documents, images and external file formats. For example, `OpenFile()` can be used to open an `.mdproj` file as a project, a markdown document in the editor, an image in a preview tab, and a Word document via the Shell.

* **Addins: New Document.RenderHtmlWithTemplate() method**  
`MarkdownDocument` now has an additional method to create a full HTML document using the selected Preview Template into a string. This functionality was previously available only via parameter settings in `RenderHtmlToFile()`. Added for clarity.

* **Fix: Preview Browser Initial Refresh**  
Fixed bug where Preview Browser would occasionally not show unless another tab is activated. Traced to a selection timing bug and fixed.

* **Fix: Excel icon in File Browser**  
Add missing Excel icon in file browser.

* **Fix: Folder Browser doesn't display files with leading `.`**  
Fixed issue where files and folders starting with `.` were not displayed in the Folder Browser.

* **Fix: Add additional File Types for Window.OpenFile()**
Additional file format operations for the new generic `Window.OpenFile()` operation. Specifically catch common executables and show file in Explorer rather than directly executing as an extra 'verification' step.

* **Fix: Weblog Endpoint Discovery doesn't block any longer**  
EndPoint discovery was previously checking synchronously for discovery URLs. Apparently WordPress slows RPC/XML requests and these requests can be slow to respond. Switched to async code so UI doesn't lock.

* **Fix: Commandline now creates non-Existant Markdown Files**  
If you specify to open a Markdown file that doesn't exist in a command line filename parameter, MM now opens an empty file that has a filename and is ready to be saved with `Ctrl-S`. Only works with Markdown files. Other files are ignored causing the folder to be opened in the folder browser.

* **Fix: Spell Check Suggestions Dropdown**  
When showing spell check suggestions don't show the full context menu, only suggestions and Add To Dictionary. Fix issue with Add to Dictionary always showing on the context menu even when not on a spell check item. 

* **Fix: Task List Styling**  
Fixed task list styling to match GitHub's styling. Removed the bullet and extra indent.

* **Fix: Apostrophe Handling for Spellchecking**  
Fixed issue with *special single quotes* from Word Processors and other desktop application. Normalized special apostrophes so they are respected in word spell checking.

* **Fix: Browser Syncing not working after Preview Close**  
On occasion when closing the preview and re-opening the preview would not longer sync to the editor or vice versa. A restart was request. Fixed.

* **Fix: Preview Git Changes for files in subfolders**  
Fix bug with 'Open in External Gif Tool' failing for files in subdirectories.


### 1.17
<small>May 14th, 2019</small>

* **Image Dialog Enhancements**  
The image dialog now supports resizing images and opening images from the clipboard in your selected image editor. You can easily save images to disk multiple times (or with multiple filenames) and you can also re-paste images from the clipboard after saving. Editing automatically can pick up changes from the clipboard upon return to MM. You can also use Alt-I as an interactive alternative for Ctrl-V Image pasting.

* **Image Configuration Changes**  
Image configuration in the `MarkdownMonster.json` file now has a separate `Images` section to contain all image related settings like editor selection, last folder, last image size set etc.    
***Breaking Change**: you may have to reset your image editor/viewer in the configuration file if you had them previously set* 

* **Paste Image From Clipboard into Folder Browser**  
You can now paste an image from the clipboard directly to a file in the folder browser. This is in addition to the ability to paste clipboard images into documents, create a file on disk and embed the reference in the document.

* **Drag and Drop Files and Folders from Explorer into Folder Browser**  
You can now drag and drop files from Explorer into the folder browser to quickly move one or more file or folders.

* **[Drag and Drop Tabs into the Favorites Sidebar](http://markdownmonster.west-wind.com/docs/_58u0u6bnh.htm)**  
You can now drag a tab from the editor into the Favorites tab to create a new favorite shortcut more easily. This external drag and drop behavior augments the internal drag and drop that lets you re-arrange favorite entries.

* **Drag and Drop into Favorites from Explorer**  
You can now drag and drop into Favorites from Explorer in addition to dragging a tab, and the various context menu options.

* **Add New Favorite File Name Improvements**  
The Add New Favorite context menu option now fixes up file names by replacing `-` and `_` with spaces, and reversing Camel Case syntax. Also fixed focus issues.

* **[Favorites Improvements](https://markdownmonster.west-wind.com/docs/_58u0u6bnh.htm)**  
Favorites now have keyboard shortcuts for common tasks like deleting and editing. If a bookmark file or folder doesn't exist any longer the entry is marked as missing in the favorites list so you can fix it or remove it more easily.

* **Row and Column Display on the Status Bar**  
The current row and column position in the document now shows on the status bar in the stats section.

* **Improved Up/Down key Scroll Speed**    
MM monitors scroll operations in the editor in order to sync the preview as you navigate. Previously the threshold for updating the preview was too low causing scroll speed slow-downs. Bumped the threshold up a bit for much improved cursor scroll speed. Still not great as there are still checks for scroll changes, but they happen much less frequent now.

* **Add Auto-Save and Auto-Backup to Edit Menu**  
These allow setting the per document auto-save and auto-backup options. These values can override the default setting that's set in the configuration file.

* **Add Open on GitHub on Tab Context Menu**  
You can now open the current document on GitHub if the document is in a GitHub repository.

* **Tab Context Menu Context Sensitivity**  
The tab context menu is now properly context sensitive and displays only documents that are relevant for current operations. Options now also work correctly for preview tabs.

* **Editor Context Menu Combines Tab Menu**  
The Editor Context Menu now also displays the tab context menu options for easier access to those options. New users often don't think to use the tab context menu so options have been combined.

* **Add Markdown Link  Navigation in Preview**  
You can now click on a Markdown document link (typically in documentation solutions)  in the editor and open that document in a new editor document.

* **[`-close`: Close Editor File from the Command Line](https://markdownmonster.west-wind.com/docs/_5fp0xp68p.htm#close-filename-command)**  
Added new `-close filename` command line option that allows you to close a file via the command line. This allows a limited amount of remote automation of MM via command line operations to launch and close files. This allows for integrations like `Open in document in Markdown Monster` from external applications and change monitoring.

* **[`-autosave`: Command Line Option to automatically save Opened Files](https://markdownmonster.west-wind.com/docs/_5fp0xp68p.htm#autosave-mode)**  
Added a new `-autosave` command line switch When automating files it's sometimes useful to force MM to save output to file immediately as you write without explicitly setting the option inside of Markdown Monster. By specifying a file name to open with `*_autosave.md` post-fix MM will automatically force the file to be auto-saved as you type.

* **Improved support for Definition Lists**  
Definition lists are now rendered with header and indentation and are collapsible via header click in the preview styling. Definition lists by default render with a bold header and are collapsible:
    
    ```markdown
    Header Text
    :   Detailed content below
        More content on a new line
    ```

* **Remove Bootstrap Preview Theme Dependency**  
Removed the dependency on Bootstrap in the preview templates. MM never really used any of the Bootstrap features internally for previewing - it was more of a convenience for people explicitly embedding raw Bootstrap HTML into pages. You can still easily use Bootstrap by [creating your own custom preview templates](https://markdownmonster.west-wind.com/docs/_4nn17bfic.htm) and simply adding Bootstrap to the header of the `Theme.html`.

* **Remove unneeded FontAwesome Font Files**  
Removed unused FontAwesome font files only leaving the `.woff` (required for IE and the Preview) and `.woff2` (everything else). This reduces the size of exported, embedded HTML by a few hundred kbytes of font files that don't need to be embedded.

* **Explicit Menu Commands for Folder Browser, Outline and Favorites**  
Added menu commands on the View menu for these operations which make them keyboard navigable via `Alt-V-F`, `Alt-V-O` and `Alt-V-V` respectively. For now these are not mappable via keybindings.

* **New HTML to Markdown Parser**  
Switched to [ReverseMarkdown](https://github.com/mysticmind/reversemarkdown-net) parser for HTML to Markdown conversions that are cleaner and more reliable in the conversion process. This affects the `Ctrl-Shift-V` shortcut that lets you paste HTML as Markdown, and WebLog HTML imports.

* **[Add -presentation Command Line Switch](http://markdownmonster.west-wind.com/docs/_5fp0xp68p.htm)**  
Using the `-presentation` command line switch you can start Markdown Monster in presentation mode which shows the preview full screen.  
Example:  `mm file.md -presentation`


* **[Add Option to set `previewWebRootPath` to Document](https://markdownmonster.west-wind.com/docs/_5fz0ozkln.htm)**  
You can now specify a custom YAML header to specify a Web Root path that resolves `/` to the path you specify when rendering the Preview. This allows greater documentation systems to work with non-relative, site relative URLs and still render images and links properly in the previewer.

* **Editor.PreviewHighlightTimeout Configuration Switch**  
Add new configuration key for `Editor.PreviewHighlightTimeout` that controls how long the currently active line is highlighted in the previewer. Value is in milliseconds and 0 never hides it.

* **Preview Highlight now updated on Keyboard Up/Down Navigation**  
The preview highlight previously only updated on scroll operations or if the view ended up getting scrolled by keyboard operations. This change now hooks to the up/down key navigation to update the preview highlight.

* **Addins: MarkdownDocument.SetHtmlRenderFilename()**  
Added method that allows custom renderers to override the location of the HTML render filename. This allows for rendering HTML in a custom folder that has the proper base path for finding resources.

* **Development: Markdown Monster now uses SDK Style Projects**  
Under the hood Markdown Monster now uses .NET SDK style projects to build for all projects. This means MM requires Visual Studio 2019 and the .NET Core 3.0 Preview 5 or later SDK.

* **Development: Prepare Markdown Monster for .NET Core 3.0**  
Markdown Monster now **dual targets** for **.NET 4.62** (as always) as well as **.NET Core 3.0**. A lot of internal work was done to fix a number of incompatibilities for .NET Core 3.0 and MM can now run under .NET Core 3.0 Preview 5. You'll need to make sure you have .NET Core 3.0 Preview 5 SDK installed to compile and run MM at this point under 3.0.

* **Development: Improved AppInsights Error Logging**  
Error log now includes log level additional state to make it easier to group errors and failures by severity.

* **Fix: Local links in packaged HTML Exports**  
If a local file or other link is missing the export now properly continues instead of displaying an error. If a file or link is missing the export just skips over the file - this may cause a loss of document display fidelity in some cases, but it's better than failing to produce any output at all.

* **Fix: Link Preview Document Navigation Issues**  
Fix support for Preview link navigation when pointing back to a local Markdown file. Fixed for links with Hashes(`#`) and for wiping out dirty document changes when navigating back to a document that was already open.

* **Fix: Snippet Addin Slow First Activation**  
Due to our recent switch to using Roslyn for compilation, startup for first time snippet use can be fairly slow taking a few seconds. Offloaded initialization of Roslyn onto a background thread during startup, gives quick response on first use now.

* **Fix: Registration Dialog Title Update**  
Registration dialog now updates the title immediately after changing registration status. Previously a tab switch was required.

* **Fix: Path Cleanup**  
Due to a small bug MM would write multiple paths into the global user path when running MM in development mode piling on all paths that MM would run out of. Updated so only one path is written and updated.

* **Fix: Snippet Plugin Slow Initial Load**  
Recent updates to the underlying compiler used for snippet compilation have caused a slowdown during first time execution of snippets in the Snippets addin. We added preloading and time managing the compiler lifetime to improve startup and continued execution speediness.

* **Fix Dirty State Indicator when in Auto-Save Mode**  
Fixed issue where auto-save mode would not properly reflect the document dirty state. Auto-save now saves in 2 second intervals when idle.

* **Fix: [OpenInPresentationMode](http://markdownmonster.west-wind.com/docs/_4wn1ditb5.htm) Mode Setting**  
Fix `"OpenInPresentationMode": true` configuration setting to work properly. Due to timing issues this setting was popping up initially, then quickly reverting to default layout. Fixed.

* **Fix: Emacs and Vim Keyboard Emulation**  
Enable Vim and Emacs keyboard emulation which had been temporarily disabled due to the editor loading refactoring recently. It works again now.

### 1.16
<small>March 20th, 2019</small>

* **[Centered Layout for Editor Surface](https://markdownmonster.west-wind.com/docs/_5ed0rj891.htm)**  
Refactored the previous `Padding` and `MaxWidth` settings using a new **Centered Layout** option that can be quickly toggled from the **Views -> Toggle Centered Layout**. Centered layout applies a max width (default of 970 pixels) to limit the width of the editor content, so on very wide screens you don't get overwhelmed by a massive wall of text. Properties have been refactored to `CenteredMode`, `CenteredModeMaxWidth` and `Padding`.

* **Add `Shift-Enter` Key Combo for Soft Returns**  
You can now press `Shift-Enter` to insert a soft return which expands to two spaces and a return which is a Markdown Soft Return.

* **Open Content from Console StdIn**  
You can now open content piped from StdIn into Markdown Monster. You can use a command like `DIR | markdownmonster stdin` to open the output from the stdin directly in MM.

* **[Improved Math Support](https://markdownmonster.west-wind.com/docs/_59l0mv2uw.htm)**  
Added custom Markdig parser to support MathJax expression rendering. You can now enable the Markdown `UseMathematics` settings switch to automatically expand Math expressions using `$$` or `$` expressions, MathML, or `<div class="math">`. With the new extension, most math expressions are now rendering reliably. The `useMath: true` YAML header is no longer necessary - MM now scans the document for embedded math expressions automatically.

* **Update MarkDig for Math Parsing**  
Updated to latest MarkDig version that includes new Math expression wrapping from our PR that removes need for our custom Math MarkDig extensions. The behavior of our previous fix is now built into MarkDig directly. Yay!

* **Update Save As Encrypted File Dialog**  
Made the dialog easier to visually parse at a glance and work with. Add filename and path to the make the file you're encrypting or decrypting more obvious.

* **[New Console Addin Available](https://github.com/RickStrahl/Console-MarkdownMonster-Addin)**  
There's a new **Console** Addin available in the Addin Manager that lets you attach a Terminal Console window that is 'pinned' to the bottom of your Markdown Monster instance. As you move or activate MM the Console sticks with the application. You can customize what Terminal tool to use (Powershell, Command, Base, ConEmu, Commander etc.) and it defaults to PowerShell.

* **Open Blog Post in Browser**  
Added option on the Weblog menu to open the Weblog post in a Web Browser if the `permalink` YAML meta property is set. Perma link downloads with published post data now if available from the server.

* **Add Permalink to Weblog Meta Data**  
The Weblog addin now downloads and also sends the permalink of a Weblog post so you get a direct URL where your post can be accessed. If the engine supports it it's also possible to change the permalink.

* **Unblock Portable Mode DLLs in Addins Folder**  
When running for the first time in portable mode MM will try to unblock the DLLs in the `Addins` folder which otherwise fail to load if installed from a Zip file off the Internet or other unknown location. This should fix startup addin load errors for portable installs.

* **Change Branch DropDown on Git Commit Dialog**  
You can now change branches in the Commit dialog assuming there are no pending changes. Currently no support for creating new branches, we'll add that in a subsequent update.

* **Git Commit Dialog Remembers last Commit Operation**  
Remember last Git Commit Operation: **Commit** or **Commit and Push** and show last option used first and bolded.

* **Add Symbol Configurations for some Markdown Expansions**  
You can now specify a few options for how certain symbol shortcuts are expanded using `MarkdownOptions.Symbols`. Initial keys are `italic` and `softReturn` expansions which determine the `ctrl-i` and `shift-enter` default expansion formatting.

* **New Command Line Registration Option**  
You can now register Markdown Monster with `mm register regKey` to automate the registration process for larger organizations that need to install Markdown Monster on many machines.

* **Library Updates**  
Update all .NET dependencies to latest versions of libraries.

* **Fix: File Change Notifications**  
Fixed issue with file change notifications not properly clearing the dirty buffer flag after updating file from disk which resulted in repeated dialogs even if no changes were pending. Fixed.

* **Fix: Password Dialog When no Doc is Open**  
Fix issue that would crash if no document is open. Also key icon is no longer shown when no document is open preventing the issue in the first place.

* **Fix: Addin Error Handling**  
Fixed issue where a misbehaving addin had the ability to crash Markdown Monster during startup and quit without any errors or notice. Added additional error checks and additional logging to try to
pin down which addins might be causing problems.

* **Fix: Startup Rendering**  
Improve startup rendering by removing some unnecessary nested delay loading. Also fixed a couple of issues related to screen positioning which caused startup jank in some load scenarios. Fixed.

* **Addins: Expose Folder Browser**  
We've now made the Folder Browser more easily accessible through the `Model.Window.FolderBrowser`. You can also easily get the selected item in the folder browser via `GetSelectedPathItem()`.

### 1.15
<small>February 5th, 2019</small> 

* **[Add Split View for the Editor](https://markdownmonster.west-wind.com/docs/_5ea0q9ne2.htm)**  
You can now split the editor into two panes (Below or Beside) and view and edit the current document in two synced, but independently scrollable views.

* **[Add Editor MaxWidth Configuration Option](https://markdownmonster.west-wind.com/docs/_5ed0rj891.htm)**  
Added a optional `MaxWidth` Editor Configuration setting that lets you set the max width of the edit area. This can be useful for large displays and distraction free mode where you want to see a comfortable width of text surrounded by extra white space rather than very wide wall of text. The default is 0 which means text flows as wide as the window (minus the padding) - any positive value (recommend ~1000) will kick in padding as the editor content area exceeds that width.

* **[Add Editor Padding Configuration](https://markdownmonster.west-wind.com/docs/_5ed0rj891.htm)**  
You can now specify the padding for the editor work space of each tab. Previously this value was fixed but you can now provide wider (or thinner) horizontal margins to give you more white space while editing especially on larger displays.

* **Editor WrapMargin**  
Added support for specifying a `WrapMargin` when `WordWrap=true`. You can now specify column number for the `WrapMargin` to force the editor to wrap at that column to control the width of the editor content. 

* **Better support for [Mermaid Charts](https://markdownmonster.west-wind.com/docs/_5ef0x96or.htm) and [MathText/MathML](https://markdownmonster.west-wind.com/docs/_59l0mv2uw.htm)**  
Made improvements to how Mermaid charts and MathML/MathText expressions can be processed. You can now simply add mermaid content in `<div class="mermaid">` block and Math expressions by providing a `useMath: true` YAML header. Markdown Monster now automatically includes the required library dependencies and fixups for rendering output. Note: mermaid charts have to be previewed in external browser.

* **[Alt+G default hot key for Git Commit Dialog](https://markdownmonster.west-wind.com/docs/_4xp0yygt2.htm)**  
There's now a dedicated hotkey by popular request for accessing the Git Commit Window, which allows easy access to a number of Git operations in MM.

* **Improve Initial Document Loading**  
Improved load time for documents when they originally load and reducing flicker. Folder browser is now more responsive to 'preview' -> full edit view transitions which now occur without flickering.

* **Optimize ACE Editor Loading**  
Refactored some of the ACE editor startup code to reduce duplicated styling of the editor and background flashes while loading. Editor now renders in a single pass resulting in a much less bouncy first editor display.

* **Improve First Run Window Size Experience**  
Added logic to detect window size on first run and adjust the main window accordingly. Small monitors run near maximized while large monitors get a larger but not giant instance on first launch. New behavior also respects DPI settings.

* **Show In Folder Browser now selects File**  
Show in Folder Browser previously only opened the folder in the folder browser. If a file is passed it now opens the folder and selects the file passed in the folder browser's file list.

* **Add Open With... in Folder Browser File Context Menu**  
You can now use the Open With dialog to choose how to open a file from the folder browser in addition to opening in editor, or using the default Windows program for a given extension.

* **Improved Folder Browser Navigation**  
We've changed focus behavior in the editor to not automatically focus the editor for a number of tab change operations which reduce flickering and jumpy selections in the folder browser.

* **Add Support for latest C# features to [Snippets Razor Addin](https://github.com/RickStrahl/Snippets-MarkdownMonster-Addin)**  
The Razor engine in the Snippets editor now can utilize C# 7.3 features in scripts using **Roslyn** compilation. Snippet expressions continue to use the old C# compiler, as it provides much faster startup performance with no explicit gain from new language features.

* **New `RenderExtension` Interface for Addin Authors**  
*Internal and Addin usage* - You can now add RenderExtensions into the Markdown processing pipeline as a 'post-processing' step 
after the HTML was generated. A new RenderExtensionsManager can be accessed to add additional extensions that can post process rendered HTML output.

* **Add `editor-user-extensions.css` to allow Editor CSS Overrides**  
Like the script extensions added in the previous release, this allows making editor CSS overrides to affect how the editor renders code. Note there's not a ton of stuff that can or should be changed since most of the styling comes from themes, but it does allow for some rudimentary enhancements to the editor.

* **Add `editor-user-extensions.js` to allow Editor Extension**  
Added support for an optional `editor-user-extensions.js` file that can be used to create custom extensions to the Markdown Monster JavaScript ACE Editor wrapper. This allows addin and Commander Script authors to create custom ACE Editor functionality that can be called from .NET with `Model.ActiveEditor.AceEditor.MyCustomFunction()`.

* **New `previewUpdated` JavaScript event for PreviewHtml**  
*Internal and Addin usage* - Added a `previewUpdated` even that fires whenever the preview is updated without replacing the entire document. This allows `MarkdownRenderExtensions` to dynamically refresh a document without requiring script blocks to re-execute on each render. Used for Mermaid and Math support internally but available for any JavaScript based addins that require refreshing on rendering.

* **Weblog Configuration Updates**  
Made it a little easier to configure new weblogs by automatically jumping to the Weblogs tab when no Weblogs are available when the Weblog selection is clicked or when submitting the Web log form. Add password validation check to Weblog configuration form.

* **Improve Weblog Blog Discovery**  
Weblog discovery now looks for a few additional clues to try and discover blog Urls on a Web site in addition to RDS and standard Wordpress locations.

* **Open Weblog Posts Folder**  
New menu option that opens the Weblog Posts folder so you can look for and open post entries more easily.

* **Spellchecker Ignores URLs**  
The spellchecker now no longer tries to correct text inside of a URL, links for images or URL links (both Markdown and HTML links).


* **Fix: Enabled/Disabled State of Menus**  
Fix issue where first loaded documents would not properly enable/disable certain menu and toolbar items due to open document state. Fixed OnPropertyChange for the document to always fire even on existing selected document.

* **Fix: Weblog Management Form**  
Fixed a few small issues in the Weblog entry form. There's now an explicit save button and a new entry isn't added until the Save button is clicked. Existing entries are still live edited. Fixed a few navigation bugs in the form that could crash MM.

* **Fix: New WebLog File and Folder Names**  
Fix up logic that creates new folder and file names to remove `&` and `'` characters that can throw off relative URLs.

* **Fix: Image Dialog Image Preview**  
Fixed image preview for file and URL links so that the image displays in the preview area. Updated Image Editing link to open defined image editor.

* **Fix: Editor Focus on Dialog Operations**  
Fixed a number of places where editor focus was lost after content was inserted through dialogs (paste image, screen capture , href etc.). Keeps your fingers on the keyboard. Regression when tab focusing logic was changed recently.

* **Fix: User Agent in XMLRPC calls to Weblogs** 
Found that the default XML RPC user agent was invalid per spec and was causing problems with some Web servers. Changed user agent for all XMLRPC operations to `Markdown-Monster`.

* **Fix: Re-activation Focus**  
Fixed bug that wouldn't properly reactivate editor when navigating off of Markdown Monster editor. Regression fixed and added proper focus handling that remembers what control was focused before navigating off and resetting. Note this is a change of the pre-regression behavior which **always** reactivated the editor. Now the editor is reactivated only if it was previously active.

* **Fix: Open in GitClient**  
Fix paths with spaces not opening properly.

* **Fix: Recent Document Handling to remove missing files/folders**  
The recent document list and startup forms now properly won't show files and folders that no longer exist.

* **Fix: Mysterious Crashes that 'just exit'**  
Fixed crashes caused by Dispatcher errors when the dispatcher was disabled. This would cause odd crashes especially with status bar updates. Added extra checks around several frequently used generic Dispatcher operations that account for most Dispatcher operations.


### 1.14
<small>December 13th, 2018</small>

* **Open PDF documents from Previewer**  
Added logic to open PDF documents in the system configured PDF viewer externally, since the WebBrowser's preview can't display PDF documents.

* **Next and Previous Spell Check Error**  
You can now use a hotkey (F7 by default) to quickly jump through spell check errors in the document via keyboard navigation.

* **Fix: Window Close Crash Bug with Registration Dialog**  
Fix issue where MahApps is crashing due to an already closed window on shutdown. Fixed by moving the window as part of the dialog logic.

* **Fix: Image Dialog Invalid Filename Crash**  
Fix invalid filename crash in image dialog by intercepting invalid filenames and displaying an error.

* **Fix: Fix Window Preview Zoom Operation when clicking Slider**  
Fixed issue where the Editor/preview slider would not properly resize after double clicking back to split view. Proper behavior is to zoom preview on first double click, then restore on second double click, but only if the window is still zoomed, otherwise re-zoom preview.

* **Fix: Consolidate Shortcuts and Fix ShortCut Captions**  
Simplified internal shortcut management and consolidated various shortcuts with separate implementations. Also cleaned up Shortcut Gesture text on menus for consistency.

* **Many small performance Tweaks**   
Optimize loading of sidebars to not update when not visible on startup and otherwise. Optimize tab activation. Fix several small issues related to tab activation. Several preview optimizations. Much improved Table Editor performance.

* **Fix: Add Word To Dictionary Spellcheck Refresh**  
Fixed refresh when adding a new word to the dictionary, so that the new value is no longer highlighted as an error.

* **Optimize Image on Folder Browser Menu**  
A new menu for images allows optimizing images using Pinga which optimizes PNG and JPEG images very quickly. Can produce quite radical image size improvements with minimal loss of quality.

* **Update to Ace Editor 1.4.2**  
Update to the latest version of Ace Editor which addresses a handful of small issues related WebBrowser control editing.

* **Optimize Snippet Rendering in Large Documents**  
When rendering very large documents (in excess of 500k) with lots of code snippets the preview can become very slow due to the refresh overhead of rendering and then showing syntax highlighting. Due to some limitations in the Web Browser (IE) control used, rendering blocks the UI thread, so very large documents can interfere with the editing experience. Added some optimizations to only render snippets that are visible and if there are more than 500 snippets snippets aren't highlighting in the preview and just apply a default style. This provides some relief for documents in the 300-500k range with lots of code snippets. For anything larger it's recommended to turn off the Html Previewer while editing these huge documents which works well.

* **Miscellaneous Rendering Optimizations**  
Remove various operations from the preview cycle that are unnecessary. Perform additional checks for active state for the Document Outline, Favorites and Folder Browser.

* **Clean up Menu Command Bindings**  
Cleaned up menu options by consolidating commands and ensuring consistent formatting. Added new shortcuts for toggle sidebar (Ctrl-Shift-B), View in Web Browser (Shift-F12), Toggle Previewer (F12). Keybindings are configurable, so to see the new formatting for shortcuts for existing key bindings you have to delete `MarkdownMonster-KeyBindings.json` in the config folder.

* **Update GitHub Theme**  
Fix up GitHub theme with a few adjustments to match the GitHub online theme. Slightly wider width, adjusting fonts and font-sizes and update list behavior.

* **Fix: Spellcheck Refresh**   
Fix spell checker refresh after replacing a word in the document. Properly refresh the spell checked content immediately. Previously it took a keystroke or other remove operation to refresh the spell check error highlight.

* **Fix: Git Commit Dialog Closing**  
Fix dialog that is closing even if there are no more pending changes. This allows for performing other non-commit operations.

* **Improve Favorite Editing**  
Favorite items now have an improved editor that uses less space and uses icons on the favorites title bar. Save and cancel operations are fired off the RETURN and ESC keys respectively.

* **Per Document LastImageFolder Setting**  
When images are saved the last image folder is now saved with the internal document settings updated and each document has its own folder it tracks, which means multiple windows can each have their own image save locations which is useful when working on multiple unrelated documents. Previously there was a global setting for this. The default is the same location as the document when the last image folder is not set.

* **Updated Image Optimization for pasted and file images**  
Updated image optimization by using new [Pingo tool](https://css-ig.net/pingo) to optimize images. Pingo is significantly faster at optimization and reduces size considerably more than the old [optipng](http://optipng.sourceforge.net/) based implementation. We now also optimize jpeg images thanks to Pingo.

* **Fix: Image relative Paths**  
Fixed bug that wouldn't properly create a relative path for pasted images - the relative path would always revert to the current path of the document. Paths are now properly adjusted.

* **Fix: Document Outline Accuracy and Jitter**  
Document outline selection previously was jittery in that selection would often jump to a new location due to recursive scroll capture. Fixed scroll capture behavior and removed extra editor navigation.

* **Fix: Two-way Editor Preview Synching**  
Fix a number of issues with two-way editor preview syncing, that would cause excessive jumpiness in the editor and in some cases inability to select a specific area. This update fixes navigations by preventing recursive editor and preview navigation for much smoother and reliable syncing between editor and previewer.

* **Fix: Additional Error handling logic**  
Added additional error handling logic to capture errors that would simply exit MM. Additional error handlers have been added to handle most error scenarios. Also improves ability to continue running in the current state for many errors without an explicit shutdown.

* **Fix: Document Outline Rendering**  
Fix document outine nesting levels by parsing original Markdown text and levels. Outline can now properly skip outline levels and properly deals with headers in sub-components like block quotes or definition lists.

* **Fix: Silent Shutdowns**  
Fixed issue where occasional crashes would shut down MM without any messages or errors or log entries. Regression in error handling logic. Added extra error handling that should capture more unhandled hard errors than before.

* **Fix: Tab Close Button Sizing**  
Fix invalid sizing of the tab close button.

* **Fix: Folder Browser New Folder Double Display**  
Fixed issue with new folders in folder browser showing two items.

* **Improved CSV Imports to Table Markdown**  
Added an additional dialog that allows importing CSV from file or the clipboard (ie. Excel or other spreadsheet Copy). You can now also specify/override the CSV delimiter.

* **Fix: Paste Operations where both Text and Image is provided**  
Fix issue where certain paste operations would try to save an image instead of text. For example, pasting from Powerpoint which apparently copies both text and an image of the selected slide text. Switched to prioritize text.

### 1.13 
<small>September 27th, 2018</small>

* **Fix Application Shutdown Release Issues**  
In recent releases MM would occasionally hang when shutting down leaving multiple processes hanging around orphaned. Refactored unload code, explicitly release browser instances, addins and various services.


* **Link References in Link Dialog**  
You can now created reference links that are referenced at the bottom of the document rather than embedded directly. There's a new checkbox option in the Link Dialog that lets you embed links as references. References are automatically updated based on order in the document. There's also a new `UseReferenceLinks` setting that determines the default state of the checkbox.

* **New Vertical Sidebar Tab Layout**  
After a number of requests moved the sidebar tabs to the left of the File Browser, document outline and favorites. This should make these features also more discoverable and it allows for more addin tabs without crowding the display.

* **Add Option to replace Weblog Image Urls to Absolute Urls**  
Added `ReplacePostImagesWithOnlineUrls` option in the Weblog Configuration to allow relative image links to be replaced with Absolute URLs after they've been uploaded to a blog. Useful to avoid uploading the same images repeatedly for blog engines that won't replace existing images of the same name for posts.

* **Fix: SpellChecking for Brackets and Slashes**  
Fixed bug where certain characters were not excluded for error checking. Missed a block of symbols from the break list. Fixed.

* **Fix: Missing HighlightJS Syntaxes**    
Recent changes in how our custom HighlightJs bundle is built resulted in some languages missing. Explicitly added required languages. Complete list of languages available now: **css less javascript html json xml diff cs cpp ini java makefile php markdown http python typescript fsharp vbnet dos powershell bash dockerfile swift dns yaml diff sql pgsql yaml text go rust text foxpro**


* **Fix (maybe): Address Shutdown Issues**  
There have been a number of errors in the analytics logs related to shutdown of MM. Simplified the shut down routine by removing all explicit `Hide()` operations to avoid potential circular shutdown events.


* **Updated: Html Sanitation when using `AllowRenderScriptTags: false`**  
Updated the HTML sanitation logic when script tags (and script execution in general) is not allowed in the generated document. This update strips out any tags that can load javascript, `javascript:` tags and any event handler code embedded in the resulting HTML content.

* **Add Toggle to allow Turning on Script Execution in Markdown**  
Added a menu option in the View menu to **Allow Script in Markdown** to turn HTML Sanitation on or off.

* **Developer: BeforeDocumentRendered and DocumentRendered Events**  
There are two new events on the `MarkdownDocument` object, that allow you to capture the document's markdown and HTML before and after the document has been rendered using the `BeforeDocumentRendered()` and `DocumentRendered()` events. These events can be intercepted in the Commander and Snippets addins or in your own Addins (although full Addins have dedicated handlers for this).

* **Fix: Embedded Document Outline Indentation**  
Fixed issue with the *Insert Document Outline* feature that can embed a table of content style link list into the Markdown document. Fixed error where indentation was not reflecting the actual hierarchy.

* **Fix: User Registration Display**  
Add Registered notice into the About form, so users can see when they are registered. Changed verbage for Registration form access to Software Registration to make it less ambiguous that you can both register and unregister.

* **Fix: CommonFolder Startup Issue**  
If the **CommonFolder** configuration setting is set to a non-existing folder, MM now defaults to the default `%appdata%` location. This fixes a severe startup failure that would cause MM to launch and disappear previously.

* **Fix: Save As Html with Packaged Documents with Untitled Docs**  Fixed issue where untitled documents would not properly generate packaged output due to a missing base folder. Changed behavior to use the temp folder which may or may not produce desired results. Recommendation is to save document before exporting.

* **Fix: Change default Save As Html output to Raw HTML**   
Changed the default option when using the Save As Html dialog to save to the Raw HTML output from markdown, with the packaged document options getting shifted down.

* **Fix: Remove Rename Delete Option in Folder Browser for non-files** 
Removed the Rename and Delete options from root folders and non-edit/delete items in the folder browser.

* **Fix: Custom Dictionary Not Creating**  
If no non-installed dictionaries have been installed previously, adding new words to a custom dictionary failed because the folder didn't exist. Fixed.

* **Fix: New File Keyboard Shortcut in Folder Browser**  
Changed to a separate keyboard shortcut (shift-f2) in the folder browser to avoid behavior confusion between New Document behavior. Folder browser adds file in browser, while new document creates an **Untitled** document.

* **Fix: Plain Text Code Fences Rendering**  
Fix issues with plain text rendering when using `text`,`plain`,`txt` or `none` for code fence blocks. Previously `text` and `plain` would incorrectly render as invalid languages. Fix uses JavaScript interception to correct highlightJs issue.

* **[New KeyBinding Manager](https://markdownmonster.west-wind.com/docs/_59l0izpoe.htm)**  
You can now remap a number of keyboard shortcuts using the `MarkdownMonster-KeyBindings.json` file. In this file you can assign new shortcut keys to a number of commands.

* **Save All File Menu Option and Shortcut**  
You can now use **File->Save All** or `alt-shift-s` to save all open documents. Existing documents are saved in place, any untitled documents prompt for a filename to save to or to cancel.

* **Add TabSize and UseSoftTabs Configuration Options**  
Allow configuration of how tabs are handled in the editor by adding config options for setting the TabSize and whether to use hard or soft tabs (spaces instead of tabs).

* **Add Open on Github in Folder Browser**  
If a file or folder is in a Github repository there's now a link in the folder browser to open the file on Github.

* **Add KeyBinding for `TogglePreviewBrowser` (F12)**  
Added configurable key binding for toggling the Preview Browser visible/hidden with a default key of F12. Also fix shortcut for `PresentationMode` (F11) key binding.

* **Folder Browser Git Context Menu now Context Sensitive**  
Fixed the Git options on the Folder Browser context menu to be context sensitive on whether the file/folder is a Git repo, and whether it's a Github repo.

* **Update to Ace Editor 1.40**  
This update provides a number of new features to the editor along with a number if nice performance improvements. Among the improvements are: Better support for Asian languages, better (but still limited support for Right to Left languages) and a number of fixes in the markdown syntax editing features.

* **Fix: Favorites Search to Open Folders above Found Items**  
Fixed regression bug that would not properly open parent folders when a match was found inside of a folder hierarchy.

* **Updates to Startup Screen**  
Updates to the Recent File and Folder List display and added Theme switching to the Startup screen.

* **Reduce Minimum Window Size**  
Reduced minimum Window size to 390x220 per user requests.

* **Change from Copy Folder Name to Copy Full Path**  
Changed all instances of Copy Foldername to Clipboard to instead copy the full path which is usually more useful for pasting into Explorer or command lines.

* **Fix: Window Menu Shortcuts**  
Fixed a number of the Window menu shortcuts that are toggle switches. DistractionFree mode (Alt-Shift-Enter) and Presentation mode (F11) and Help (F1) now work properly again.

* **Favorites**  
You can now select files and folders as favorites for easy access. Favorites are accessible from the toolbar and the Recents menu and you can add Favorites from the tab header. A new **Favorites** sidebar lets you select and add/edit/delete favorites as well as select and open favorites.

* **Add Weblog Menu to Main Menu**  
Added Weblog menu for easier access to individual Weblog related features and better keyboard navigatability and also to better reflect actual status for each of the available operations.

* **Jump to Anchor Context Menu**  
New context menu option when hovering over a Markdown link that is an anchor to a auto-header link. Makes it easy to jump around the document and follow internal links as an alternative to the Document Outline.

* **Import CSV files into the Markdown Table Editor**  
You can now import CSV files into the Markdown Table editor using a button on the toolbar.

* **Updated Startup Screen**   
Updated layout for the startup screen. Shows more visual and scrollable Recent Files and Folder list. Cleaner layout for logo background. Better fit in small window sizes.

* **Addins: Simplified Adding of Tab Panels**   
Updated the `AddLeftSidebarPanel()` and `AddRightSidebarPanel()` routines to use consistent tab styling and make it easier to simply provide header text and an icon, rather than explicitly setting up the tab header (which is still supported if the header text/icon are left empty).

* **Fix: Table Editor Issues**  
Fix issue with **Edit Table** in Markdown. Detection algorithm didn't recognize certain grid tables. Fix paste issue where there was an extra linefeed at the begging of the edited table resulting in a shifted display. Fix document dirty status properly updating now.

* **Fix: Dead Menu Options**  
Fix a number of menu options that were briefly inert due to the recent menu code refactoring. Should all be hooked back up.

* **Fix: Add to Dictionary File Location**  
Fixed issue where Add to Dictionary was adding to the wrong file and wouldn't persist properly. Updated logic so that after adding the document immediately is updated to reflect the corrected word in the spellchecking view.

### 1.12
<small>June 21st, 2018</small>

* **Improved Folder Browser Preview editable documents**  
The Folder Browser now supports 'previewing' of Markdown documents in place with editor and preview. Single click opens the editor and previewer in 'preview mode'. If another document is accessed the tab goes away. If you edit the 'preview' tab (italic tab header) the tab is converted into an active tab that behaves like other editable tabs. 

* **Improved Image Preview**  
Image previews now show when you single click an image which displays the image in a new document preview tab. Images are displayed in scaled mode and include file information - file name, dimensions and file size in the preview.

* **Updated Recent File List to be easier to view/navigate**  
Consolidated Recent List display and use a image icons and bold text for the filename and show full path low-lighted below. End result is a much more usable recent file list especially if you opt for a long list (configurable in Settings).

* **Updated Save as HTML Options**  
Save As HTML can now save HTML either as raw generated markdown fragment, fully self-contained, (very large single file HTML file, or saved into a folder with HTML and all resources downloaded as files into the folder.

* **Explicit Paste Image option on Editor Context Menu**  
The editor's context menu now shows **Paste Image** if an image is on the clipboard. Text shows as Paste and if no content is present the option is disabled.

* **Fix Image Refresh for 'cached' images**  
MM previews Markdown in HTML and HTML by default caches images, so if you embed images and then replace or edit images that are already display, the images didn't refresh in the preview, unless you reloaded the browser. Added logic to force the browser to explicitly hard refresh on image update operations (paste, image dialog, drag and drop).

* **Refresh Browser in Preview Context Menu**  
Related to Image Refresh: Added explicit option to refresh the browser preview window to force updating changes in images or other resources on disk in the Preview browser.

* **Additional Menu Options for the Preview Browser**  
You can now use the Save As Html, Save As Pdf and Print options from the Preview Browser's context menu.

* **Create Git Repository and Add Remote**  
Added support for creating GitHub repositories and adding a new repository as a remote to an existing local repo. Options are available under the **File->Git** submenu.

* **Add Push to Git Repository to Commit Dialog**  
In addition to the the **Commit and Push** button in the dialog, you can now also explicitly push to the remote, when there are no files to commit.

* **Open Git Remote in Browser**  
The Commit dialog now has another option - if there's a Remote associated with the current repository, you can now open the remote in the Web browser. Jumps to the repository root on Github or Bitbucket etc. as long as the URL can just remove the HTTP URL `.git` extension.

* **File Operation in Commit Dialog**  
You can now open a file in the commit dialog in Explorer and delete the file on disk. The context menu also is actually context sensitive. Commit dialog now also has new button to link to the 

* **Better SpellChecking Dictionary UI**  
The top window box spell check toggle now displays the active dictionary language (ie. en-US, de etc.). Toggling spellchecking on and off now displays a message on the status bar so it's easier to tell when the icon is toggled (not so easy to see due to the coloring). 

* **Excessive Spellcheck Errors now disable the SpellChecker**  
Excessive spelling errors in the current view now automatically toggle off the spell checker with a message in the status bar. This is very useful when the language in use doesn't match the documents language which generates a gazillion errors, which can be very slow. Messages suggests that an unmatched language is used and suggests to change language. Changing languages or toggling the setting explicitly re-enables spell checking.


* **Fix: Spellchecking Ignored items**  
Links, image links and inline code blocks are now no longer spellchecked. Text inside of single quotes and any kind of custom quote characters is now properly spellchecked inside of the quotes. Overall you should see a lot less false positives for spell checking (still a few use cases the parser isn't catching but a lot less).

* **Dictionary Downloads**   
You can now download dictionaries. Dictionaries are downloaded into the MM AppData common settings folder in a `DownloadedDictionaries` folder. This folder now also holds custom dictionaries for added words. Done so dictionaries and common words can be shared and for easier management of the dictionaries. You can now opt to remove all downloaded dictionaries to allow reinstalling dictionaries in case of updates.

* **UI Enhancements**   
Lots of small UI improvements. Fix dialog focus issues with various pop up windows. Cleanup window inconsistencies for dialogs. Statusbar code consolidation. Icons on the Left Sidebar tabs. Git icon on the toolbar.


* **Preview Tab to preview rendered Markdown and Images on click**  
There's a new `MainWindow.OpenBrowserTab()` method that allows for opening a preview tab in the browser that can display local or Web based content in a browser as a Preview window. The Preview tab is temporal - it's visible only until navigating to another file and then released. The preview tab is now used internally for previewing images, Markdown and HTML files in the Folder browser.

* **Snippets Addin integrated into MM Core**   
Moved the Snippets addin into the core MM Solution so it's always up to date and synced to matching dependencies.

* **Improved Addin Removal**  
You can now remove addins from the Addin's drop down menu. This also works for manually installed addins and test scenarios where addins aren't installed through the Addin Manager.

* **Addins: Failed Addins Removed**  
If addins fail to load they are removed from the addin list and removed on the next pass. This is a temporary situation as we work out the changeover to new dependencies, so that the same warnings don't keep popping up each time you launch if you didn't uninstall explicitly.


* **Back to 32 bit mode**    
Version 1.11.15 briefly was distributed to run in 64 bit mode. Unfortunately we're seeing lots of instability with hard WPF crashes and slower performance overall, so we're reverting back to 32 bit.

* **Markdown Monster .NET Minimum Version is now 4.6.2**  
Due to some API changes in third party dependencies along with issues in DPI Scaling, Markdown Monster now requires .NET 4.6.2 to run. Previously versions down to 4.5.2 were supported.


* **Fix: Paste Markdown to Clipboard as HTML**  
Fixed operation of Markdown selection to Clipboard, so that HTML is generated both for formatted output (ie. for RTF pasting into Word or Outlook or other HTML editors) or HTML as plain text. Previously the plain text paste produced the original markdown. HTML plain text is usually the desired option for manually pasting into other applications since you can always grab the raw from the editor with a plain copy.

* **Fix: Git Commit Username/Email text box layout**  
Fix layout bug with the username and email textboxes that are overlaying the comment box.

* **Fix: Remembered Documents and Startup Position**  
Fixed issue where remembered document on startup would not remember their line position through multiple starts.

* **Fix: Tab Order Preserved for Open Documents on Restart**   
Fixed issue where tabs were not ordering the same as during shutdown when restarting MM.

* **Fix: Dirty State when Spell Checking**  
Fixed dirty state update when selecting a misspelling correction on a clean document.

* **Fix: Blank Preview on Startup**  
Fix occasional issue with blank previewer when MM starts. Force focus.

* **Fix: Clipboard Assignment Crashes**  
Logs indicate a number of people have issues with Clipboard access, specifically setting values on the clipboard - both during editing and also from explicit clip assignments for URLs, commands etc. All set operations are not exception bracketed so while ops may still fail they won't crash MM.

* **Fix: Document Outline Crash when empty Doc is open**  
Fixed issue with the Document Outline crashing when an empty document was open.

* **Fix: Window Menu Shortcut Keys**  
Fix Window menu mnemonic keys that didn't allow for shortcuts to work.

* **Fix: RPC Weblog EndPoint Discovery for Medium**  
Fix endpoint discovery for medium with a fixed URL. Also adjusted Wordpress endpoint discovery.

* **Internals: StatusBar Consolidation**  
MM uses status bars on a number of forms and there was lots of duplication. Consolidated all statusbar operations into a helper which is called from each form to handle status bar operations in one place resulting in a large code reduction.

* **Internals: Update depencenies**  
Update all dependencies to latest releases - except for LibGit2Sharp which switched to .NET Core 2.0 assemblies and results in a splattering a huge amount of runtime dependencies into project. Waiting to target .NET 4.7.2 to not require those dependencies.

<h4 style="color: firebrick">1.12 Breaking Changes</h4>

 * **Portable Version Changes affect Configuration**   
If you are runing the portable version of Markdown Monster installing 1.11.16 is going to lose your existing configuration settings for Markdown Monster and start with a fresh configuration. If you would like to use your old configuration make sure you shut down MM first, and then copy the existing configuration from `%appdata%\Markdown Monster` (or a custom location if you've configured one) to `<installFolder>\PortableSettings`. This will restore the old settings. Make sure you shut down MM before updating the files. If you used a custom location for configuration also remove the `CommonFolder` key from `MarkdownMonster.json` so it resets to the new portable location.

* **Addins: Update your addins if you're using 1.11.10 or earlier**
This update has breaking changes related to addins. When updating to a newer version you may see addin-load failures. If that's the case uninstall and reinstall the affected addins.

* **Addins Providers: Addins have to be updated**   
There have been a few underlying API changes and support libraries have been updated that require all addins to be recompiled. Note that the WebLog, Screen Capture and Snippet addins are built-in and not affected.


### 1.11  
<small>May 16th, 2018</small>

* **Add Git Commit Dialog**  
Added a new Git Commit dialog that allows committing and pushing the active file as well as all pending files to a Git repository. This replaces the previous hotkey only command with a more visual approach that provides many more options. Ctrl-Enter in the dialog can be used to quick commit similar to the old behavior.

* **Git File Status in File Browser**  
The Folder browser now shows Git status information via icons. There are also a new option to undo changes on the file context menu.

* **Open Git Client From Document or Folder Browser Folder**  
You can now configure an external Git Client and open it from the Folder Browser or the Open Document's Tab Context menus.

* **Add Clone Git Repository**  
Added option to clone a Git repository to a local folder to make it easier to retrieve Git content for local editing. Also added a new **File -> Git** menu that houses this option and **Commit to Git** (same behavior as the Tab context menu). **Requires:** that Git and Git Credential Manager (for private or authenticated repos) are installed.

* **Add Git Pull Support**  
Add the ability to pull data from the remote origin into the current Git repository. There aren't any options for specific branches and pull does a merge commit. Feature is available on the Git Commit Dialog.

* **Add Commit Dialog Option to Leave Window Open after Commit**  
There's now a persisted option that optionally allows you to leave the Git Commit window open if there are still pending files to be committed. This makes it easier to selectively commit files into multiple commits.

* **Add VS Code Dark Editor Theme**  
Added a new VS Code Dark editor theme that is similar to VS Code's default editor theme. Due to differences in the rendering engines between ACE and Monaco the styling isn't identical but fairly close.

* **Open Markdown From Url**  
Added the ability to open a Markdown document from the Web via a URL. This feature understands and fixes up Github and BitBucket Markdown documents, Gists and Microsoft Docs Documentation URLs and allows for de-referencing of relative image links as an option. Also optionally fixes up image links to absolute Web URLs so images can display in the preview browser.

* **Add support for DocFx/Microsoft Docs Include Files in Preview**  
The Preview can now optionally render DocFx style includes in the form of `[!include[title](fileName)]` linked files to render. Relative files will automatically be included and are rendered inline. There's an option that turns this behavior on and off: `markdownOptions.ParseDocFxIncludeFiles`. More DocFx features will be added in near-future updates.

* **Add Drag and Drop File Moving in Folder Browser**   
You can now drag and drop files to new folders in the folder browser.

* **Allow Selection and Downloading of Spell Check Dictionaries**  
The Spell Check selector option in the control box now allows switching of spell checking dictionaries. It also allows for downloading of alternate dictionaries that are not pre-installed from an online list.

* **Main UI Theme Switching Improvements**  
Switching between light and dark themes now automatically assigned a default editor theme to match the light or dark theme. Theme switches prompt for a restart and if you opt in automatically shuts down and restarts Markdown Monster.

* **Option to turn off Markdown Bullet AutoCompletion**  
There have been quite a few complaints around the auto-completion bullet editing in Markdown text and now there's an option to disable it. In fact, bullet auto-complete is now off by default and has to be enabled explicitly with `Editor.EnableBulletAutoCompletion`.

* **Add MaxDocumentOutlineLevel Configuration Value**  
You can now specify the Max Outline level displayed in the Document Outline panel. This value also affects the embeddable Table of Contents that can be generated from the document outline. The value can be interactively set in the Document Outline's Context Menu.

* **Add Undo/Redo to Editor Context Menu**  
Added Undo and Redo options to the editor's context menu as they were missing.

* **Open Markdown files linked in Preview in Editor**   
If you have a link in your markdown to another Markdown file the previewer now detects that the file is a local Markdown file and opens it in the editor.

* **Addin: Intercept Preview Link Clicks**  
Addins now have a new `Addin.OnPreviewLinkNavigation()` to intercept navigation and handle custom link processing in the preview. Return `true` to override or `false` to let the default link processing work.

* **Editor Enhancements**  
Updated to the latest version of ACE Editor which is noticeably faster and more stable. A number of cursor and scrolling related issues are addressed. Updated Twilight and the new VS Code Dark Theme.

* **Add Editor LineHeight Configuration Switch**  
Added configuration switch to allow setting the editor text line height to separate or tighten up the spacing between text. Default line height has been bumped up to 1.3 from 1.2 which gives a little bit more space between lines.

* **Editor Configuration Consolidation**   
Internally consolidated all the editor styling options into a single `Editor` configuration section that forces the editor to restyle based on editor configuration settings. Previously all of these settings were individually set. This makes it easier for addin authors to modify editor settings and simply call `Model.Configuration.RestyleEditor()` after making changes to the configuration settings.

* **Fix Git Clone with Path with Spaces**  
Fixed bug for paths with spaces in Git Clone operation.

* **Fix: Clear document stats when last tab is closed**  
Fixed issue where when the last tab was closed the document stats were not cleared.

* **Fix Folder Browser New File/Folder Editing**  
Fixed regression introduced by file search in the list that would interfere with adding new files and folder. New files and folders now properly get inserted into the hierarchy order, and show the appropriate icon and git status.

* **Fix: Code Snippet Wrapping for Printing and PDF Generation**  
Fixed Code Snippet wrapping so that code snippets wrap rather than scroll when printing or generating a PDF file.

* **Fix: Miscellaneous Addin-Manager Issues**  
Fix update button display. Fix installed version number display.  
Fix list rendering issues in the Addin list. Fix version comparison logic to determine whether an update is available.

* **Fix: Binding issues with various Menu options when no Documents are open**  
Fixed various issues with the menu when no document is open so that a number of options are not available. All commands are now explicitly rechecked when documents are opened or closed (via Tabs).

* **Fix: UrlEncoding in Paste URL Dialog for Local Files**  
Fix the Paste URL dialog when embedding relative file links to be URL Encoded.

* **Fix: UI Inconsistencies**  
Fixed a number of left over UI inconsistencies that made it into earlier 10.x published releases. A few places where background colors and control heights were off. Refactored a number of common control settings to global scope for better overall consistency.

* **Fix: Document Outline Visibility**  
Document outline would still be visible after unchecking the Document Outline option in the menu if enabled on startup. Outline is now properly hidden.

* **Fix: Folder Browser New File Adding**  
Fixed new file editing behavior that would get corrupted by the file search behavior in the tree.

### 1.10
*<small>March 27th, 2018</small>*

* **Document Outline**  
There's a new Document Outline feature (preview) that provides a two-way sync between the active document and the outline. The outline shows headers (h1-h4). You can click on bookmark links, and the outline stays in sync when you scroll the document.

* **Table of Content Generator**  
As part of the Document Outline display, there's an option to generate a TOC and embed it into a Markdown document at the cursor position. The TOC is created a Markdown list with links and can be re-generated on demand.

* **Document Outline Context Menu to get Link ID onto Clipboard**  
There's now a context menu on each document outline item to copy the hash ID to the location in the document. Useful if you need to link to another part of the document.

* **Fix up spaces in Editor Markup Operations**   
Editor markup operations like the bold, italic, underscore, small etc. now automatically handle fixing up leading spaces so if you select ` selected words ` (note the leading and/or trailing spaces that are selected) the tag is updated as ` **selected words** ` (space before and after the `**`) effectively transposing the unintended spaces in the markup.

* **Add Syntax Selection Dropdown on Status Bar**  
The status bar now displays the active syntax scheme and allows you to select a different syntax for a given file.

* **Keyboard Auto Search for starting Letters in Folder Browser**  
You can now type in a few letters to jump to the first matching file similar to the way Explorer finds files.

* **Folder Browser File System (File Watcher)**  
The folder browser now displays file system updates in the folder browser. If you externally add, delete or rename a file the external changes are reflected in the Folder Browser.

* **Folder Browser `IgnoreFolders` and `IgnoreFileExtensions`**  
Options to filter the folder browser display for certain folder and file extensions. Folders default `.git, node_modules` and extensions to `.saved.bak`. Values can be customized in Settings.

* **Double Click Editor/Preview Separator to Zoom Preview**  
You can now double click the separator between the Editor and Preview to zoom the preview for a quick way to browse read/only content. The Sidebar remains on the left of the display to double click and restore the original width.

* **Edit Preview Template from Preview Browser**  
You can now directly jump to the Preview template that's active and edit the HTML/CSS that makes up that template in the Preview Browser using the Preview Browser's context menu. The link also opens the Preview Theme Editing documentation.

* **Main Content Area UI Refactoring**  
The main content UI area has been refactored to encapsulate the editor and preview pane in a single layout panel. Tabs now stretch across the entire content and preview area to avoid excessive tab crowding especially on small displays. Internally the render logic has been refactored to make the UI easier to manage in code via bindings and simpler property access and allow for more modular layouts that can be driven from Addins including support for adding UI panels.

* **Addin Support for adding Left Sidebar Tab Items**  
Addins can now implement a `AddSidebarPanelTabItem()` method to add a new sidebar panel which becomes a tabbed item alongside the File and Folder Browser. This allows for custom list panels for additional functionality like Git interaction, custom documentation solutions and document navigation and so on.

* **Left and Right Side Bar Hamburger Menu for Slideout Menu**   
The left sidebar (Folder Browser) and right sidebar - if it has content - now have dedicated hamburger icons on their respective sides to slide out the sidebars when active. This makes the sidebars opening operation more discoverable. Additionally you can now drag to open and close the sidebars. Also remove the open icons from the main toolbar and window control menu since the icons are more logical.

* **Addin Support for a Right Tab Sidebar**  
There's now also a new Right Side bar which can contain tabs to allow another avenue of providing additional UI for documents. Like the left sidebar, you can add tab pages to the layout to add UI.

* **MarkdownDocumentEditor Identifier and Properties Collections**   
The MarkdownDocumentEditor instance now has a collection of Properties that can be attached to the editor to allow you to associate custom state to an editor instance. This allows Addins to capture state when a specific tab is activated via `Model.ActiveEditor.Tag.Properties["state"]`. `Identifier` is a new string field that allows adding a custom designation for easy checking.

* **More Markdown Rendering Configuration Options**  
Added additional configuration switches to the Markdown configuration to allow for more complete Markdown extension support as provided by [MarkDig](https://github.com/lunet-io/markdighttps://github.com/lunet-io/markdig). Added support for [Custom Containers fenced `<div>` blocks](https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/CustomContainerSpecs.md) and [Generic Markdown Attributes](https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/GenericAttributesSpecs.md) and added explicit properties for most of MarkDig's extension features.

* **Refactored the Preview Browser to allow pluggable Preview Controls via Addins**  
Consolidated the preview rendering via an `IPreviewBrowser` interface and a control that hosts the preview. This greatly reduced code duplication for preview handling in the internal and external viewers, but now also allows pluggable previewers in Markdown Monster. There's a new Addin function: `GetPreviewBrowserUserControl()` that allows replacement of the stock preview browser control with a custom control that implements `IPreviewBrowser`.
Example: [Chromium Preview Addin (preview)](https://github.com/RickStrahl/ChromiumPreview-MarkdownMonster-Addin)

* **Post Date for Weblog Post Meta Data**  
The Weblog post meta data now stores the original post date, which also allows for modifying the post date to a new date when reposting **if** the Weblog service supports that feature.

* **Addins: Support for Read Only Editor**   
You can now open a new editor tab in Read Only mode which can't be edited but can still be viewed and scrolled. Also added a double click handler in read only mode that triggers an `OnNotifyAddin` event for `ReadOnlyDoubleClick` that can be used to take action on the double click (like open a new window).

* **DisableSplashScreen Configuration Setting**  
You can now optionally disable the splash screen from firing up when starting Markdown Monster.

* **New Markdown Monster Logo**  
Yup we have a new image logo for the startup banner and about page.

* **Fix: Quicker Save Behavior**   
Save operations previously were slightly delayed and didn't show the document as saved immediately. Made file save explicit rather than triggering on dirty flag update for much more responsive save and UI feedback.

* **Fix: Sort Order in Folder Browser**  
Fix sort order to work with lower case sorting so folders or files that start with `_` sort to the top and lower and upper case file names are not mixed up in sorting.

* **Fix: Weblog Post Download with FrontMatter Header**  
Fix issue where Weblog posts that contain FrontMatter headers would double up the FrontMatter and title headers. Fix checks for FrontMatter in downloaded post and if found just display the raw post retrieved with the original FrontMatter and Markdown formatting.

* **Fix: Preview Theme Code Block Rendering Wrap**   
Fixed preview themes so that code blocks do not wrap in the previewer and exported HTML output. Affected Darkhan, Github and Westwind themes.

* **Fix: Table Importer with HTML Tables**  
Fix table importer context menu to find tables inside of the Editor. Fix import behavior if table uses upper case tag names.

* **Fix: File Modified Dialog Behavior**  
Fixed prompt behavior when a file was changed when not focused on the editor and returning. Previous behavior **always** popped up a dialog for the file change. New behavior automatically updates the document **if there are not changes pending in the editor**. This avoids numerous dialog popups that are effectively unnecessary.

* **Fix: Scroll Stutter at the top of the Document**  
Fixed issue where initial scroll operation from the top of the document would get 'stuck' and require explicit cursor or slow scroll movement. Fixed by changing the top window scroll detection logic.

* **Fix: Find/Replace Box Formatting**   
Fix styling of the Find/Replace box which was broken after a recent update of Ace Editor. The box now also reverts closer to Ace Editor's default behavior which includes responsive auto-sizing which was previously disabled.

* **Fix: #Hash Links when generating PDF Output**  
Previously hash links to HTML Anchors or Ids (such as a table of content) were not working in PDF output. Output now properly supports internal document links.

* **Fix: Fix ignored files in Folder Browser**  
The folder browser now properly ignores on the `ignoreFileExtensions` setting, for new, externally created files that are updated in the browser view.

* **Fix: Recent Files Menu Clears**  
Fixed issue where the Recent Files menu would clear if more than a certain number of files are present.

* **Fix: Weblog Keywords and Categories Trimming**  
Fixed issue where keywords and categories on posted blog posts where not properly trimmed if spaces were present in the comma-delimited list.

* **Fix: Internal Links for PDF Output**  
PDF output now can properly display local links inside of the document. Changed behavior of the exported HTML document by exporting to the .md file's folder and removing the `Base` tag which was interfering with local links.

* **Fix: Code Wrapping in the Previewer**   
Fixed how code displays by overriding default Bootstrap styles that wrap code. Override forces horizontal scrollbars onto code blocks.


### 1.9
*<small>January 24, 2018</small>*

* **Version Rollup Release**  
This release is a version Rollup release that combines all the recent additions into a point release.

* **Add Table Editor support for Paste Table**   
You can now paste a table from HTML, or Pipe or Grid Tables into the table editor. Note if the table is heavily formatted it'll likely end up as mostly HTML, but simple formatted tables with links, images and simple markup are converted. 

* **Edit Table in Editor now also supports HTML Tables**  
The editor context menu now allows editing of HTML Tables inside of the table editor. Inside of a table in the editor, right click (or use the Windows drop down key with the cursor selected inside of table) to open the table in the editor. Support for complex HTML inside of cells is limited to plain text, links, images, bold and italic.

* **Performance improvements in Table Editor**  
Reworked the custom layout used to edit table rows that makes rebinding of table rows and columns much more efficiently than previously.

* **Start Screen Improvements**  
Fix recent file list on the Start Screen to refresh and accurately show recent files. Also tweaked the layout of the Recent Files list to be more easily readable and added tooltips to links to get full file names.

* **Add Open Documents Dropdown for Tab Overflow**  
Changed the tab layout so when there are more tabs than can be displayed, a drop down shows a menu with all open files that can be selected.

* **Add Window Menu to show all open Documents**   
Add standard Window menu to show all open documents and allow manipulation of the open documents (Close open tab, Close all, Close all but).

* **Editor Zoom Level Improvements**  
Changed the Editor zoom level settings to use both a default size and zoom-level percentage. There's now a percentage zoom level indicator on the status bar that allows quickly adjusting to common values or typing in a percentage value. Double clicking resets to 100%.

* **PDF Generation Enhancements**   
Added option to not generator a Table of Contents. Print PDF asynchronously so UI doesn't freeze up. Cleaner error messages. Fix headers/title by removing the right header and rendering only a single header. Add option to copy last used command line. Cleaned up the Save As PDF form UI.

* **Fix: Folder Browser Bug Fixes**  
Fix a number of small folder browser inconsistencies related to adding new files and renaming. Fix edit color theme on light application theme.

* **Fix: Command Line Startup with Folder**  
Fix bug where command line startup with a folder name would not open the folder in the folder browser properly.

* **Add Recent Folder List in Folder Browser**   
The folder browser now has an icon to show and re-select recently opened folders. Recent folders are also shown on the Recent Files drop down.

* **Add Find Files in Folder Browser**  
You can now press Ctrl-F in the Folder Browser to open a search box that will search files in the current folder, or optionally down the folder hierarchy (which can be potentially slow if the tree is deep).

* **Folder Browser Support for %EnvVar% in Paths**   
When entering paths directly into the folder browser path dialog you can now use environment variables in the format of `%envVar%`. So, `%appData%, %programfiles%, %tmp% all expand to full paths.

* **Auto-Complete for Folder Browser Path Textbox**   
The Folder Browser's path selection entry combobox now auto-completes paths as you type them for easier path selection.

* **Add Folder Navigation to Folder Browser**   
Added a few features to make the folder browser more navigation friendly. You can now **double-click on folders** to re-open the folder browser as the top level folder in the folder browser. A new **Open Folder Browser** here context option is also available. A new **parent folder `..` node** is now added to allow navigating back up the tree by double clicking on this node.

* **Improved Folder Browser Performance**   
The folder browser now lazy loads sub-folders, rather than loading entire folder hierarchies up front. As a result the Folder browser should load most folders much more quickly. Still slow with very large numbers of files in a single folder (500+ files).

* **Open in Folder Browser from Document Tabs**  
There's a new context menu option to open the document's current folder in the folder browser.

* **File Tab Icons**  
Added icons to the tabs for each of the open files in the editor.

* **Fix Shift-Delete Behavior**   
Fix additional issue with `Shift-Del` operation when text is selected which now properly **cuts** selected text to the clipboard. Previous behavior just removed the selection.

* **Fix: Table Reformatting if Column Count exceeds Columns defined**  
Fixed issue where if a table row had more columns than the header defines the table would blow up on rendering. Common scenario when 'editing' a table and then repasting it. Fixed trailing whitespace (which caused extra columns) and fixed so that extra columns if provided are ignored.

* **Fix: Drag Images into 'Untitled' Documents**  
Fix bug where dragged images from the folder browser into 'Untitled' documents would not do anything. Fix now works and prompts for images in the last saved image folder. 

* **Fix: Folder Browser Icons when Adding/Renaming**   
Fix folder browser when adding or renaming files so that file icons are appropriately updated. Also fixed tab icon and re-opening the renamed file if it was already open in the editor with proper filename and icon.

* **Partial Fix: Drag images from Explorer**   
Images dragged from Windows Explorer now drop at the current cursor position in the editor (not the dragged mouse position). Due to security limitations Drag and Drop into the browser control from external doesn't translate the mouse position into the control so the only relevant place we can drop is at the last known cursor position. Previously images dropped at the end of the document.

* **Fix: Incorrect Drag Behavior and Scrollbar Interaction in Folder Browser**   
Fix funky drag behavior when an item is selected and trying to scroll the scrollbar on the file list. Fixed by explicitly checking for the item being selected being dragged rather than the entire tree.

* **[New two-way Table Editor](https://markdownmonster.west-wind.com/docs/_53a0pfz0t.htm)**  
Added table editor dialog that allows for creation of Markdown (and Html) tables interactively. A new dialog provides column based data entry with tabbable fields along with the ability to easily add and delete rows and columns. Output can be generated as either Markdown or HTML and you can use the Editor context menu's 'Edit Table' option to edit or reformat a table.
 
* **New .mdown, .mkd and .mkdn Extension Mapping**   
Added additional Markdown extensions to be recognized by Markdown Monster as markdown text.

* **Add `Ctrl-Tab` and `Ctrl-Shift-Tab`  for moving between open Document Tabs**   
You can now use `Ctrl-Tab` and `Ctrl-Shift-Tab` to flip more easily across the open editor windows on the editor pane.

* **Paste Code Dialog Picks up Code from ClipBoard**   
The Paste Code Dialog (alt-c) now tries to pick up code from the clipboard and displays it for editing and then pasting - if no code selection is active in the editor. This can provide an optimized workflow: Copy code to clipboard from code editor, switch to MM, press `alt-c`, type syntax code (ie. css, html, csharp), press enter and code is pasted properly into markdown. 

* **Remove Markdown Formatting**   
Added menu option and Ctrl-Shift-Z shortcut to remove Markdown formatting from a selection of text in a Markdown document.

* **Shift-Del to delete current Line**   
Added support for `Shift-Delete` to support removing the current line like Visual Studio/Code.

* **Add Folder Icons to Folder Browser**   
The folder browser now displays folder icons in addition to the expand/collapse arrows to avoid confusion over indentation level.

* **Fix: Multi-page Selection Issue fixed**  
Fixed regression bug caused by recent addition of scroll syncing from editor to preview. Code fix now checks if a selection is active, and if it is doesn't scroll preview.

* **Fix: Add to Dictionary for SpellCheck when no matches**   
Fixed issue where if there are no items to display for spell check suggestions the **Add to Dictionary** menu option wasn't showing. Fixed now the **Add to Dictionary** menu shows to allow adding unmatched words.

* **Fix: Links to #Hash Ids**  
Links to hash tags as IDs wasn't working. Change preview script to navigate both `name` and `id` refs in the document.

* **Fix: Shift-Del - Delete Line only when no selection**   
Fixed behavior, so that if a selection is active, Shift-Del only deletes the actual selection. Only if there's no selection does Shift-Del delete the entire line (behavior matches VS and VS Code now).

* **Fix: Crashes related to File Save Operations failing**   
Fixed issue where async save operations would interfere with various file checks (for encryption and auto-save operations). Fixed with single code path and lock to prevent thread cross talk.

### 1.8
*<small>December 4th, 2017</small>*

* **Version Rollup Release**   
This release is a version rollup release that combines all the recent additions into a point release.

* **Show Invisible Characters in Editor**   
Option on the Tools menu and the configuration to turn on displaying of invisible characters (spaces, tabs, line breaks etc.). Thanks to a PR from [Thomas Freudenberg](https://github.com/thoemmi). 

* **Auto-install Snippets Template Expansion Addin**   
In new installations Markdown Monster now installs the Snippets addin by default. This addin is useful for creating text expansions (macro strings) that can include embedded code snippets. Supports simple `{{C# Code}}` expansions or Razor syntax scripting.

* **Fix: Window Title on first Load**  
The Window title on first load wasn't showing the active file name until switching tabs.

* **Fix: Create new File/Folder in Folder Browser when in Empty Folder**  
Fixed issue where the New File/Folder options would not work in an empty folder.

* **Fix: Preview Window Flash**   
Fixed errand preview window refresh when switching tabs and during shutdown.

### 1.7.8
*<small>November 28th, 2017</small>*

* **Add External Preview Window**   
You can now toggle between the internal Preview window pane or an external window that can be moved independently of the main Markdown Monster window. This often requested feature allows you to move the preview window to a separate monitor or moving it to a small docked Window.

* **Change Default Terminal Client to Powershell**  
Updated the default Terminal Client to use Powershell instead of Command. Also updated documentation to fix disk drive navigation in Command mode (not Powershell) by using parameters: `"/k cd /D \"{0}\""` for Command shell args.

* **Refactor Preview Rendering**   
Internal change that modifies the internal rendering logic for the preview into an isolated class. This will make it easier to add alternate browser rendering targets in alternate windows.

* **Minor Improvements for Startup Speed**   
Refactored various bits of startup code and changed initial preview behavior to be slightly delayed for faster 'to first cursor' operation.

* **Async WebLog Uploads**   
We've updated the Weblog uploader to run asynchronously without locking the UI thread. While Weblog publishing can be blazing fast if the server responds quickly, for some sites uploads were blocking the UI hard (the main WordPress site in particular). This update works around the synchronous Xmp-Rpc library explicitly creating a task based wrapper.

* **Fix: Duplicate Tab Names not displaying Path correctly**  
When multiple files with the same name are open, MM displays the last path segment to differentiate the documents. This code was not universally working with various locations not properly updating the tab headers.

* **Fix: Double Click File/Folder Editing in Folder Browser**  
Fix double click editing in the folder browser when actually selecting a file which often would open a file and then also make the filename editable. Selection now properly resets the edit double click timeout so file does not become editable.

### 1.7.6
*<small>November 19th, 2017</small>*

* **Addin OnModifyPreviewHtml() added**  
Added `MarkdownMonsterAddin.OnModifyPreviewHtml()` which is allows addins to modify the preview output html used for displaying the preview. Thanks to Jim McClain for providing the PR for this addition.

* **Change Preview Sync Default**  
Preview sync default changed to **Editor -> Preview** to avoid potential sync jitter issues with the **Both** mode.

* **Weblog Publishing Updated**  
Minor tweaks to the Weblog download feature and HTML to Markdown conversion. Small UI updates in the download UI and better UI responsiveness when uploading posts to a blog.

* **Updated Gist Integration Addin**   
The [Gist Integration Addin](https://github.com/RickStrahl/GistIntegration-MarkdownMonster-Addin) has been overhauled with better integration into Markdown Monster. You can now open and save documents from Gists right on the File menu and also from the main **Paste Code As Gist Form**. You can now also delete Gists directly from the Open and Save lists.

* **Switch back to 32 bit**   
When we moved to Markdown Monster to the `%LocalAppData%` folder recently, Markdown Monster temporarily went back to running in 64 bit mode. While this worked fine, performance was noticeably more sluggish in 64 bit mode, so we're going back to forcing Markdown Monster to run in 32 bit mode which has overall responsiveness of the UI considerably.

* **Addin Icon Interface Enhancements**   
Add better icon and image icon options to make it easier to create icons and modify the icon after loading if necessary. Added `FontawesomeIconColor` property to allow specifying a color, and simplify the loader logic for loading image icons. There's a new `Addin.AddinMenuItem.MenuItemButton` property that gives you access to the actual menu button that was created for customization.

* **Fix: Recent Menu Files with `_` in Name**  
Fixed issue files that have underscores in filenames in the Recent menu, where underscores were treated as shortcut keys by WPF.

### 1.7.4
*<small>November 15th, 2017</small>*

* **Add Scroll Sync to Preview**  
Editor syncing now works when scrolling without having to explicitly click into the document to capture the current mouse position. Scrolling now moves the preview selection to near the top of the editor content (4 lines down).

* **Duplicate Filename Tab Header Display**  
Fix tab header display for duplicated file names (ie. display multiple files named `Readme.md`). Tab now shows filename plus last folder part (ie. `Readme.md - MarkdownMonster` or `Readme.md - MyProject`) to differentiate multiple files with the same name. 

* **Fix: Image File Names with Spaces**   
When saving images with spaces in filenames on the Image Dialog, via clipboard pasting or drag and drop operations, the image file name is now embedded with spaces encoded as `%20`. Although Browsers support HTML with spaces, CommonMark (and MarkDig) does not support it, so url encoding the file - while ugly - fixes this problem.

* **Fix: Recent File Menu Flash**   
Fix UI issue where selecting a file off the recent menu drop down causes the menu to fade in a funky way that fades in and out. Menu is not hidden immediately before opening the new tab to avoid the ugly behavior.

### 1.7.2
*<small>November 5th, 2017</small>*

* **Fix: #Hash navigation in Preview**  
Due to browser limitations related to local file URLs and base tags, native #Hash navigation doesn't work in the preview browser or when viewing in an external browser with a local file system URL. Added logic in the page to intercept hash navigation and explicitly navigate the document to the specified element on the page.

* **Fix: Installer Unicode Paths**  
Fix Unicode support in the installer that was causing problems when using file paths that contain extended Unicode characters. Fixed by compiling with the proper Unicode version of Inno Installer.

* **Fix: Image Upload with Spaces**  
Fix previously broken image upload when image names contain spaces or other encoded characters.

### 1.7.0
*<small>Oct. 19th, 2017</small>*

* **Version Rollup Release**   
This release is a version rollup release that combines all the recent additions into a point release.

* **Cleanup Project References for built-in Addins**  
Fixed project references in the provided WebLog and ScreenCapture addins to use package references. The references are marked to not copy local, and are used in lieu of referencing current assemblies in the main MM project. This was causing bad project references when cloning at times. Fixed.

* **Remove Web Project from main Markdown Monster Solution**  
Removed the Web project from the main project as that was causing version issues when the Web Roslyn tooling versions are out of sync. Web Project now separate. 

* **Fix: Addin notification for `OnDocumentActivated()`**   
The addin notification previously fired after the preview was updated, causing potential problems for addins that modify output and want to display the updated information in the preview. Fixed.

* **Fix: Save Image Crash**  
Fixed issue where trying to save an empty image from the Embed Image dialog would crash MM. Checks to not allow saving with no image, and safeguard if save does make it through without a memory image.

* **Fix: Windows Settings Crash in Zoomed scenearios**  
In some situations when MM is running with maximized window or in Distraction Free mode, the window settings values were indeterminate causing a startup crash in MM. Fixed.

### 1.6.8
*<small>Oct. 11th, 2017</small>*

* **Add JpegImageCompressionLevel for Pasted or Captured Images**  
There's a new `JpegImageCompressionLevel` configuration option that lets you specify what compression level Jpeg images are saved with. Values are 0 to 100 where 100 is the highest fidelity and 0 the lowest. The default is 80 which is typically just above the level where artifacts become noticeable in most pictures.

* **Fix: Custom extension mapping to Markdown and HTML Files**  
Fixed logic for extensions mapped to Markdown and HTML so that the Preview window properly shows when files with custom extensions are edited.

* **Fix: Overwrite Cursor not Visible**  
The overwrite (insert) cursor formatting only showed a dot previously. Fixed to display an underline cursor.

* **Don't spellcheck active Word in Editor**  
Changed spell check behavior to not spellcheck the word directly under the cursor as it may not be completely typed out yet resulting in annoying spell check errors. Spellcheck now ignores the current until you move off.

* **Fix: Delete in Folder Browser**   
Delete from the folder browser would not work under some circumstances due to a byte alignment issue. Fixed.

### 1.6.7
*<small>Oct. 4th, 2017</small>*

* **Fix: External Preview freezes MM in background**   
When previewing the Markdown Preview in an external browser Markdown Monster would hang for 60 seconds. Regression bug due to an API change. Fixed.

* **Fix: External Browser Preview not using CSS in FireFox**  
When previewing rendered content using FireFox, the rendered HTML fails to apply the CSS Stylesheet. Firefox requires `file:///` moniker in order to find embedded resources and doesn't work of raw OS filenames. Fixed.

* **Minor Preview Theme Updates**  
Added a Westwind preview theme that matches West Wind site. Handful of small adjustments to the Github theme to more closely match Github's styling especially for headers and lists.


### 1.6.6
*<small>Sept. 28th, 2017</small>*

* **New Startup Page**  
Add startup page that links to common operations when no tabs are open in the editor. 

* **Application Theme Selector on Statusbar**   
You can now switch the application theme between **Light** and **Dark** themes using a dropdown on the status bar. Selection optionally prompts to restart for changes to be applied.

* **Spellcheck for Weblog Posting Dialog**  
You now get spellchecking when posting new Weblog posts for abstract and title text.

* **MarkdownMonsterAddin.OnModelLoaded() Handler**  
Added another lifecycle event that notifies you when the App model is ready to be accessed. This event is fired before the form has fully rendered the model so it allows you to intercept the model before the initial form is rendered.  This event fires after `OnApplicationStarted()` (which has no model access) and before `OnWindowLoaded()` which fires once the form is active and all Addins have loaded.

### 1.6.5
*<small>September 20th, 2017</small>*

* **File Icons for Folder Browser**   
Files in the folder browser are now displayed with their associated application icons to make it easier to navigate the tree and recognize files. Additional icons can be added by adding a png file for the file extension in the `Editor\fileicons` folder.

* **Drag and Drop Files from Folder Browser**  
You can now drag non-image files into the editor to open them. This is in addition to context menu options, double clicking, enter/space selection of files. Images are treated special and are either opened in image editor, viewed with the default viewer, or when dragged into the document are embedded as images inside of markdown documents.

* **Slow Double Click Editing of Filenames in Folder Browser**  
You can now do a slow double click (Explorer style) in the Folder browser to edit and rename filenames in the folder browser. This is in addition to the F2 hotkey and context menu options.

* **Fix: Spellchecker Performance Tweaks**   
Fixed a number of event leaks in the spell checker logic that would reattach spell check events multiple times. This fix speeds up editing documents when loaded for long periods. Checker now also is more efficient about refreshing spell check info to have less impact while typing.

* **Fix: Open In External Browser Disabled**  
Fixed issue where Open in External Browser would not work open documents when the preview browser was not active. Also fixed so that rendered HTML output does not use pragma line mapping.


### 1.6.2
*<small>September 10th, 2017</small>*

* **Keyboard support for Context Menu**  
You can now pop up the context menu via keyboard using the Windows context menu key (or equivalent). The menu is now cursor navigable. This brings spell checking and various edit operations to keyboard only use.

* **Fix: `UseSingleWindow=false` no longer opens Remembered Documents**   
When not running in `UseSingleWindow` mode, the `RememberLastDocumentsLength` setting has no effect and no previous windows are re-opened. This is so multiple open windows won't open the same documents all the time. In `UseSingleWindow` mode last documents are remembered and opened when starting MM for the first time.

* **Fix: YAML parsing for Blog Post**  
Fixed bug where the YAML header on the meta data was not properly inserting a line break after the parsed YAML block.

### 1.6
*<small>September 6th, 2017</small>*

* **Version Rollup Release**   
This release is a version rollup release that combines all the recent additions into a point release.

* **Edit and Remove Hyperlink Context Menu Options**  
Added menu options to edit hyper links in the link editor or to remove the hyperlink and just retain the text of the link.

* **Fix: Command Refactoring**  
The various Command objects used to define menu options have been refactored in the code with seperate configuration wrapper methods to make it easier to find and edit the menu options.

* **Fix: Addin Loading Issue**  
Looks like a regression bug slipped through in 1.5.12 that would not allow loading of certain addins (Gist, Pandoc addins specifically).

### 1.5.12
*<small>September 1st, 2017</small>*

* **Updated Editor Context Menu**  
The Editor's context menu has been updated to forward all menu handling to WPF rather than HTML based menu display. Currently the new menu handles Copy/Cut/Paste and spellchecking duties only, but additional menu options/features can now be added more easily.

* **Edit in Image Editor Context Menu**  
You can now right click over an image embedded into a Markdown document and use the Context menu to select **Edit in Image Editor** to open the document for editing in your configured image editor.

* **Edit Image Link Context Menu**  
You can now hover over an image embedded in Markdown and use **Edit Image Link** to reopen the image in the Image Embedding Dialog.

* **Copy Selection as HTML now formats Clipboard as HTML Data Type**  
You can now use Copy as HTML from the Edit or Context menus to export your Markdown selections to HTML (as before). In addition the new behavior adds the text using HTML Clipboard formatting so you can more easily paste the HTML into rich text editors (like Outlook or Gmail's editor for example) with formatting intact. Text editors still see the raw HTML output.

* **Support for Pandoc YAML headers ending in `...`***  
Thanks to a fix in the Markdig Pandoc parser, Pandoc alternate style YAML headers that end in `...` rather than `---` are now stripped from rendered Markdown output.

* **Fix: Addin Markdown Parsers now are selectable immediately**  
When installing an addin that exposes a new Markdown Parser is now visible and selectable immediately from the Markdown parser selection drop down on the toolbar.

* **Fix: Pandoc Addin FrontMatter Support Change**  
The Pandoc Addin now handles YAML FrontMatter using Pandoc's native rendering, which works differently than the default [Markdig](https://github.com/lunet-io/markdig) parser. Pandoc picks out the `title` property from the YAML and automatically injects a `<h1>` tag with the title into the document which is not the case with the default Markdig parser. The reason for the change is to ensure you get to see the raw Pandoc output that you will see if the document is explicitly run through Pandoc.


### 1.5.8
*<small>August 24th, 2017</small>*

* **Save Encrypted .mdcrypt Extension Default**  
When you save files as encrypted files, Markdown Monster now defaults to the new `.mdcrypt` extension to signify an encrypted file. MM still can open plain `.md` files that are encrypted, but the explicit extension makes it easy to see that the file requires a password to open.

* **New Context Menu in Preview Browser**  
The preview browser now provides a few options to show content in an external browser, and show source code in an editor window (with live HTML preview of the HTML text).

* **Addin [AddMenuItem()](http://markdownmonster.west-wind.com/docs/_4zv0o3r46.htm) method**  
You can now create new menu items from an addin more easily by using the new `Addin.AddMenuItem()` method. Allows insertion of a menu item before or after an existing item by name or caption.

* **Addin OnInstall() and OnUninstall() methods**  
Addins now get a couple of additional methods to handle post installation and pre-uninstallation tasks. These methods allow installation and removal of additional resources.

* **[Update Github Gist Integration Addin](https://github.com/RickStrahl/PasteCodeAsGist-MarkdownMonster-Addin)**  
Added the Gist Integration Addin to allow loading and saving of documents from Gists. New File menu options for `Load from Gist` and `Save as Gist` let you manage both markdown and other code documents through online Gists. 

### 1.5.5
*<small>August 16th, 2017</small>*

* **Alt-z shortcut for WordWrap in Editor**  
You can now use Alt-z for toggling wordwrap in the Markdown/Code editor.

* **Updates to the Preview Active Line Indicator Highlight**
When previewing Markdown Monster highlights the active line in the previewer using custom styling. Those styles have been updated to be more context sensitive and a little bit bolder to make preview cursor position more obvious.

* **WebBrowerPreviewExecutable Configuration Option**  
You can now explicitly specify an executable to use for externally previewing HTML in a Web Browser. Since Windows seems to have a problem with amnesia when it comes to remembering file associations to the Web Browser and reverts to Edge frequently, you can explicitly specify a browser executable. Defaults to **Chrome** in `Program Files (x86)`. If executable is not found or empty, MM uses the system default (previous behavior).

* **Fix: File association not working**  
Fixed issue where clicking on a file in Explorer or using the command line wasn't opening the file in Markdown Monster.

* **Fix: Image Preview from Files or Editor**  
If selecting images in the file selector, the preview now shows local file images and images selected from within the editor via selection of an image Markdown tag.

* **Fix: Preview sync for first two lines displays top of page**  
Preview sync for the first two editor lines now show the top of the document adding no scroll offset as is done for other pages. This ensures that the top of the document displays more easily.

### 1.5
*<small>August 4th, 2017</small>*

* **Version Rollup Release**   
This release is a version rollup release that combines all the recent additions into a point release.


### 1.4.10
*<small>July 20th, 2017</small>*

* **Uninstall Command Line Option**  
You can now run `MarkdownMonster.exe -uninstall` to remove all registry settings that Markdown Monster makes during a portable (ie. non-installer) installation. To do a full, clean, manual uninstall of Markdown Monster run `MarkdownMonster.exe uninstall`, then delete the install folder and `%appdata%\Markdown Monster`.

* **Tab Headers properly hidden in DistractionFree Mode**  
DistractionFree mode now properly hides the tab headers when including `tabs` in the `DistractionFreeModeHideOptions` configuration option. Previously the tabs were not visible but the tab panel still showed.

### 1.4.8
*<small>July 6th, 2017</small>*

* **HTTP Links now rendered in external Browser**  
Embedded HTTP links in the document that point to external sites with explicit URLs (not relative URLs) are now opened in the default system browser rather than inside the previewer or - for links with explicit targets - Internet Explorer. MM intercepts navigation to http links and displays the content in the default browser.

* **Save as Encrypted File**   
You can now use a password to encrypt files when they are saved to disk. Using the new **Save as Encrypted File** option files are encrypted using TripleDES encryption using a password you provide. Encryption works for any text file you can open in MM, not just markdown files.

* **Installer no longer requires Admin Privileges**  
The full installer no longer requires admin privileges and installs into `%AppLocalData%\Markdown Monster`. MM now sets all registry keys - including file associations - in the Current User store so no admin access is required for installation or running MM.


* **Portable Install now supports Markdown File Associations**  
We've added support code to add `.md` and `.markdown` file extension association to Markdown Monster using Current User registry keys, so admin rights are not longer required. This makes all MM features available to the portable install.

* **Fix: File Browser Drag Image into Document**  
Fixed issue with File Browser image dropping. Works again as well as text selection drag and drop in editor.

### 1.4.5
*<small>June 26th, 2017</small>*

* **Word Wrap and Line Number Toggles on Menu**  
You can now toggle Word Wrap and Line Numbers from the **View** menu. Previously you could only set this setting globally in **Tools->Settings**.

* **Add Ctrl-Q hotkey for Quoting**  
You can now use the Ctrl-Q hotkey to quote a text selection. Select the text you want to quote and press Ctrl-Q to turn the text into quoted text. Same behavior as toolbar with a hotkey.

* **Fix: Commandline File Loading and MD File Load**  
Fixed regression bug that caused files to not load when running in single window mode. This affected loading from `.md` files in Explorer or from the command line via `mm readme.md`.

* **Fix: Dragging Explorer Files Copying from external Locations**  
Fixed issue where files dropped from Explorer were not getting copied properly into the selected project local folder.

* **Fix: Better support for Text Drag and Drop**  
Reworked drop handling in Editor so that Text dragging and dropping within the document works. Operations now work for both Move and Copy operations, but there's still an issue with the drag image being an image of the entire editor.

### 1.4
*<small>June 12th, 2017</small>*

* **Version Rollup Release**   
This release is a version rollup release that combines all the recent additions into a point release.

### 1.3.25
*<small>June 8th, 2017</small>*

* **[New Light Theme](https://markdownmonster.west-wind.com/docs/_4rg0mw5c2.htm)**   
Added support for theme swapping with a new light theme in addition to the original dark theme.

* **[Commit to Git for Active Document and File Browser](https://markdownmonster.west-wind.com/docs/_4xp0yygt2.htm)**  
You can now quickly commit the active document or a file from the file browser to Git in a single operation. Use the new **Commit to Git and Push** operation from the tab context menu, the tools menu for the active document, or on any single file or folder in the Folder Browser to commit and push to Git. 

* **F1 key links to Online Documentation**  
Pressing F1 now opens in the online documentation. For built in features the documentation is context sensitive.


### 1.3.21
*<small>June 2nd, 2017</small>*

* **Theme Adjustments**   
The Github and Dharkan Preview themes have been adjusted for lists and blockquote spacing and a few other minor adjustments to closer match their derived from templates.

* **Tweak Tab highlighting**   
Added slightly more contrast between active and inactive tabs while trying to preserve dark theme semantics.

* **Fix render bug in Find/Replace Editor popup**   
Fixed breaking window when using huge font sizes in the editor (for those use High DPI and no scaling). Find window no longer spazzes out for size, but also does not scale up when the rest of the editor is zoomed.

* **Fix Command Line Launch with Relative Path**  
Fixed regression bug with file opening for relative paths or folder on a new instance. Fixed.

* **Many documentation updates**   
The documentation at https://markdownmonster.west-wind.com/docs has been updated with a simpler help structure and updated topic content and FAQ. Still some work to do here but mostly there.

### 1.3.20
*<small>May 30th, 2017</small>*
 
* **Startup Screen Position**   
MM now checks startup screen position and adjusts the position to the main monitor if the saved start position is off screen.

* **Startup Speed Improvements**  
Refactored editor and preview load operations should result in slightly better startup performance. Removed several redundant preview refreshes during startup.

* **Open Folder from Command Line**  
You can now open Markdown Monster with a folder name as an argument and the FolderBrowser will be opened in the specified folder - ie. `mm .` or `mm .\subfolder`.

* **Fix: Weblog Custom Fields Display**  
Fix display bug that didn't show new items when clicking the add button. Items were added but the list didn't expand. Fixed.

### 1.3.16
*<small>May 22nd, 2017</small>*

* **Save as Pdf**  
You can now save Markdown and HTML documents directly to PDF. A new dialog provides a number of layout  options for margins and headers. The generated Pdf output can be immediately previewed.

* **Add Windows MRU List Support for Taskbar Shortcuts**   
If you pin Markdown Monster to your taskbar, you'll now get Most Recently Used (MRU) files in the file pop up list. Both files opened directly from disk or opened from within Markdown Monster now show up in this list.

* **Private Post Support for Wordpress**  
You can now select from **Publish**, **Draft** and **Private** modes when posting to your Weblog using a new drop down on the Weblog Post window.

* **Fix: FeaturedImage in Weblog Posts**   
Fixed broken featured image embedding which was temporarily broken. Image or image ID (if download from server) are now send with posts when the **Don't infer Featured Image** checkbox is not checked.

### 1.3.10
*<small>May 14th, 2017</small>*

* **Add Dragging of Images from Folder Browser into Markdown**  
You can now drag an image from the Folder Browser into your Markdown document. If the document is in the current path or below the image is embedded with a relative link, otherwise you get a file re-save dialog to optionally save the file in a local relative location.

* **Spellcheck Popup Enhancements**  
The spell check dialog now closes only by clicking on either a selection or outside of the popup window. The window no longer auto-closes when moving out of the popup. Minor styling changes to make the pop up stand out more against background.

* **Fix: Find Box Not Working**  
Fixed regression bug that cause the Find and Search and Replace functionality to not be available. Fixed.

### 1.3.8
*<small>May 12th, 2017</small>*

* **Folder Browser Image Enhancements**  
Images are now previewed as you hover over them in the folder browser. Double clicking shows the image in the configured image viewer. The context menu now has options to **View** and **Edit** images, using the configured image editors.

* **Image Editor and Viewer Configuration Changes**  
There are now two separate configuration settings for **ImageEditor** and **ImageViewer**. Both default to empty strings which bring up the default editors (PhotoViewer and Paint by default on Win10). You can configure these two keys with paths to applications. Setting up editors like Paint.NET or the SnagIt Image Editor is great for making quick edits to images. These editor and viewer settings are used in the Folder browser, in the Paste Image and Screen Capture dialogs, and the right click Image selection in the editor.


* **Custom Editor Extension Mapping**  
The extensions that are supported in the editor are now configurable via a collection of configuration values in the **EditorExtensionMapping** key in Settings. You can map any file extension to any of Ace Editor's support edit formats.

* **Fix: Untitled Documents not using Markdown Highlighting**  
Regression bug regarding the way MM looks up file extensions and didn't properly handle untitled.

### 1.3.7
*<small>May 9th, 2017</small>*

* **New File Browser Panel**  
Added a new file browser panel that allows browsing for files. You can open files from this view as well as preview images, and open others with default programs. Basic image editing and management for adding, deleting and renaming of files and folders.

* **[Custom Fields Editor for Weblog Publishing](http://markdownmonster.west-wind.com/docs/_4wq1dbsnh.htm)**
The Weblog Publish Dialog now allows interactive editing of Custom Fields to send to MetaWeblog and Wordpress Weblog engines. This is in addition to explicitly editing the post YAML meta data at the beginning of a post which can also be edited as plain text.

### 1.3.4
*<small>May 4th, 2017</small>*

* **Edit Images from Markdown Image Link**  
You can now bring up the Image Dialog from an image in the Markdown editor by selecting the image, and then right clicking. From there the full Paste Image Dialog is available and a new option there allows for editing of the image in your configured image editor.

* **Improved Paste Image Dialog**  
The Paste Image dialog has been updated with a few enhanced features that allow pasting and copying to the clipboard more easily. You can now also open the configured Image Editor from this form.

* **New `ImageEditor` Configuration Switch**  
You can now create a configuration setting for your favorite Image Editor which is used in the Paste Image dialog and by default in the Screen Capture and Paste Image to Azure Addins. Point at your favorite editor which allows for editing the current picture in the editor.

* **Fix: Spellchecking Popup overrun on bottom or right of Screen**  
Fixed issue where the spell checking context menu (right click) is not fully visible at the bottom or right for the screen. Pop up now pops up or to the left when it would otherwise be partially off-screen.

### 1.3
*<small>April 18th, 2017</small>*  
**<small>Version Rollup Release</small>**

* **Improved Spellchecking Performance**  
The spell checking logic has been updated to spell check only visible text instead of the entire document. You should now be able to efficiently use the spell checker even on very large documents. This should also improve overall performance of the editor while typing and provide consistent typing speed regardless of document size.

* **Search Emojis**  
You can now search the Emoji list or text contained in the Emoji name key using a search box above the emoji list. Emoji under cursor is now zoomed and displayed at 2x base image size.

* **Copy Foldername to Clipboard**  
Added option to the tab context menu to copy the document's folder name to the clipboard. Useful if you need to access or save files in the document folder from other applications or the command line and makes it easy to get the folder name quickly to the clipboard.

### 1.2.22
*<small>April 14th, 2017</small>*

* **Add AppInsights (experimental)**  
Added support for AppInsights to provide better telemetry on usage and exception logging. Currently running under a flag that is enabled by default.

* **Updated Toolbar icons for Code and Inline Code**  
Changed toolbar icons for code and inline code to make it clearer which is which when inserting.

* **Editor support for opening Additional File Types**  
You can now also open `.nuspec`, `.wsdl`, `.config`, `.asp/x` documents for editing. 

* **Additional Addin Helper Methods**  
Added several new common operations to the addin base class to make it easier to access common functions: `SetEditorFocus()`, `RefreshPreview()`, `OpenTab()`, `CloseTab()`, `ShowStatus()`, `ActiveEditor`, `ActiveDocument`.

* **Updated Markdown Monster Addin Template**  
The Markdown Monster Addin template for Visual Studio has been updated to add a default configuration class, as well as a few additional base addin members that make it easier to access common MM features from an addin directly off the Addin object.

### 1.2.19
*<small>April 4th, 2017</small>*

* **Added Emoji Picker**  
There's now an Emoji Picker on the toolbar that lets you select emoji from a visual selection list to embed into the document. Shortcut is **ctrl-j**. 

* **mmApp.Model AppModel now globally accessible**  
If you're building Addins, or otherwise extending MM you can now get global access to the App Model via `mmApp.Model`.

* **Fix: New Weblog Post to add initial Yaml Meta Data**  
Added default Yaml data to new weblog posts. Add logic to automatically convert old XML based meta data to Yaml data when opening or downloading existing posts.

### 1.2.18
*<small>March 31, 2017</small>*

* **YAML Weblog Meta Data**  
The Weblog publishing Addin now stores all meta data using YAML Front Matter at the top of the document rather than the pseudo XML meta data stored previously. This is more inline with markdown meta data usage in general. Existing meta data continues to work and is converted to YAML when posting posts or updating meta data.

* **Recent Document Scroll Position Remembered**   
When you shut down MM, and then re-open it, MM now remembers the last scroll position of the recent documents you stored so when opened you can continue to edit at the last position you were working on. 

* **Weblog Meta Data DontInferFeaturedImage Flag**  
You can now opt out from inferring a featured image to send to the server when publishing a Blog post. By default MM sets the first image it finds a post as the featured image and sends that to the host. The `InferFeaturedImage` flag allows opting out of this behavior so you can manually set the featured image on the server.

* **Weblog Meta DontStripHeaderText Flag**  
By default Markdown Monster finds an `# header` tag in a blog post and strips it out of the post. The header is used for figuring out hte title if no other meta data is provided. Most blog engines render the title of the post separately from the content, so the header needs to be stripped to avoid duplication. However, some platforms (Medium and some Markdown based Blog engines) don't explicitly extract and display their own header so an option is required to leave it in when required.

### 1.2.16
*<small>March 23th, 2017</small>*

* **New Pandoc Markdown Parser Addin**   
You can now plug in Pandoc support for Markdown Parsing through Pandoc in Markdown Monster. Additionally this addin adds Pandoc conversion recipies for creating a variety of output formats (PDF, Docx, Epub, ODT and more) from Markdown and HTML content. :smile:

* **Addin Custom Menu Icons**  
Addins can now provide a custom ImageSource on the Menuitem for providing an icon that displays on the menu bar via the `menuItem.ItemImageSource`. This removes the limitation of using only FontAwesome icons. It's recommended you create icons that fit a dark theme and use simple colors or better yet white and black high contrast images.

* **Addin BaseAddinConfiguration Class for Addin Developers**  
There's now a **BaseAddinConfiguration<T>** class that easily allows addin developers to set up a configuration settings file. Create a new class and subclass and add properties that are automatically persisted and call `.Write()` to write configuration changes out to the JSON configuration file. The Visual Studio Addin template has been updated to reflect this new class in a stock project.

* **Support for CustomFields.ID Values on Weblog Posts**  
When posting Custom fields as part of a Weblog Post you now get back the server generated custom field ID. This ID is store in meta data and resent when updating a post on the server to properly keep custom fields in sync - especially on WordPress.

* **DistractionFreeModeHideOptions to customize Distraction Free Mode**  
The new configuration flag lets you specify what's hidden when entering distraction-free mode. The value accepts a comma delimited string of UI features to hide: `"toolbar, statusbar,menu,preview,tabs,maximized"`. Each value specified is hidden. `maximized` is a special case - if specified causes the form to maxmize in distraction-free mode.

* **Fix: Recent document loading**  
Recent document loading on startup now properly preserves the last loaded documents up to the `RememberLastDocumentsLength` configuration setting, and selects the last active window. Previously the selection of the last document did not always occur.


### 1.2.14
*<small>March 13th, 2017</small>*

* **Drag and Drop Images from Explorer into Editor**   
You can now drag images from Explorer directly into the editor and have an image link embedded in the document at the mouse cursor. MM will prompt to save the file in a local folder if the image is in a non-relative path or relative path 1 level below the document's folder. You now have 3 ways to get images into documents: Image Dialog, Pasting from Clipboard and drag and drop from Explorer.

* **Drag and Drop Documents from Explorer**  
You can now drag Markdown, HTML and other text documents directly into the editor and have MM open that document as a new tab. Previously this would only work if you dropped documents on the Window header. You can drag multiple documents as well.

* **Paste Images as embedded Base64 Content**  
You can now embed images inside of the rendered HTML document using base64 content image content. A new **Paste as base 64** checkbox on the in the Image dialog pastes the content as base64 encoded raw image content into the Markdown document and the rendered HTML. Useful for creating self-contained, single file, but potentially very large HTML documents that don't have external image dependencies.

### 1.2.10
*<small>March 7th, 2017</small>*

* **[Emoji Autocomplete](https://markdownmonster.west-wind.com/docs/_4v503ck7q.htm)**  
Added auto-complete support for Emoji's. Type : + plus a letter or two for the emoji to embed to get a drop down that shows available Emojis. Note: The IE WebBrowser control shows monochrome Emoji, while most other browsers use colorful versions. To see what the actual Emoji look like in your favorite browser preview pages. :smile:

* **[Weblog Custom Fields Support](http://markdownmonster.west-wind.com/docs/_4v500dkm9.htm)**  
You can now specify custom fields in the meta data of a Weblog post and a Weblog to send to the server. MM already sends the rar markdown to the server as custom data, but now you can specify your own custom values that you want to send as part of the `customFields` key value collection to send to the Weblog server.

* **SnagIt 13.1.1 fixes SnagIt 13 Failure**  
Techsmith has finally released an update to SnagIt 13 that fixes the COM server bug that invalidated the SnagIt integration in MM. If you're using SnagIt 13 and want to use it with Markdown Monster make sure you upgrade to v13.1.1 or later. Thanks to Adam Marks from Techsmith for following up on this.

* **Fix: Window Title for new documents**  
Fixed bug where the window title would not update after a new untitled file was saved.


### 1.2.8
*<small>February 28th, 2017</small>*

* **[Add support for publishing to Medium Blogs](http://markdownmonster.west-wind.com/docs/_4uw03tmcu.htm)**  
You can now post your Markdown to a Medium Weblog. Due to [limitations in the Medium API](https://github.com/Medium/medium-api-docs) however there is no support for re-posting or downloading of posts. The Medium support is limited to **one-time posts**. Any subsequent editing has to be done on the Medium site.

* **Medium Preview Theme**  
Also added a Medium Preview theme that approximates the Medium default Story template.

* **[CommonFolder is now configurable](http://markdownmonster.west-wind.com/docs/_4uw16rvzj.htm)**  
You can now configure the **CommonFolder** configuration setting to point to a custom location for your Markdown Monster configuration and Addin files. This allows you to use a cloud drive to share you configuration. 

* **UseMachineEncryptionKeyForPasswords Configuration Setting**  
In light of the ability to share your configuration you can now turn off MachineKey requirement for encryption so that encrypted data can be shared across machines. Less secure but allows for sharing. Set the property to false to share configuration information.

* **Show character count**  
The title bar now shows the character count in addition to the word and line counts. This can be useful if you're using the editor to compose tweets or other character count sensitive text snippets.

* **[Custom Markdown Parser Support for Addins](https://markdownmonster.west-wind.com/docs/_4ut0j7xoe.htm)**   
You can now create Markdown Monster addins that expose custom Markdown parsers for Markdown -> HTML parsing. 

* **Improved MarkDig Configuration**   
MarkDig is Markdown Monster's default Markdown Parser and thanks to a PR from [Thomas Levesque](https://twitter.com/thomaslevesque) you can now override Markdig instantiation and rendering more easily in Addins.

<h4 style="color: firebrick">v1.2.8 Breaking Changes</h4>
This update has a few breaking changes for users and addin developers.

* **Encryption keys have been reworked**  
In order to support custom configuration folders the logic for managing encryption has changed, so all passwords for MM registration and also for the Weblog Addin are broken and have to be re-entered.

* **Configuration Structure for Addins**   
The configuration structure for MM has been refactored a bit, breaking out a number of configuration settings into object groups. If addins relied on some of these moved properties, the addins are likely to break. Recompilation of the addin for the latest version should fix these issues.


### 1.2
<i><small>February 22nd, 2017</small></i>   
<small>Version Rollup release</small>

* **New Commander C# Script Execution Add-in**   
Added a new [Commander Add-in](https://github.com/RickStrahl/Commander-MarkdownMonster-Addin) that allows easy creation of automation scripts that can be tied to hotkeys. Use C# script code to launch external applications, load data and merge it into the document, or otherwise manipulate the active document. You get access to the same features as Add-ins, but without having to create a full project. Use the Addin Manager to install this preview.

* **Updated Markdown Monster Addin Project Template for Visual Studio 2017**   
We've updated the [Markdown Monster Addin VSIX Template](https://marketplace.visualstudio.com/items?itemName=RickStrahl.MarkdownMonsterAddinProject) in the Visual Studio Gallery to work with the upcoming Visual Studio 2017 release. We've also made a few small tweaks to the template to include references to all the required WPF dependencies to make it easier creating UI based add-ins without having to explicit add assembly references.

* **Add Keyboard Shortcut support to Addins**   
Addins can now choose to add a keyboard shortcut in order to activate their Execute method. Ideally addins should implement a separate configuration setting to allow the shortcut to be configurable. Added to Screen Capture, WebLog, Commander and Snippet addins. Look for `KeyboardShortcut` configuration settings in the respective addin configuration files/settings.

* **Addins are now delay loaded**   
We've pushed loading of addins later into the startup process to asynchronously load at the end of the Window load operation. This should speed up start up speed slightly (we were already loading in the background)

* **Addin.GetMarkdownParser() to allow Customizing Markdown Parser**  
Added a new Addin method to allow addins to override loading of a markdown parser or customize how MarkDig is configured. Addin method when overridden can simply return a `IMarkdownParser` interface.

* **Fix Browser Preview for User Accounts with Accented Characters**  
Fixed bug where extended characters in user name would fail to render the preview to encoding issues. Fixed.

* **Fix Assembly Resolving to map to latest versions**   
Added additional Assembly resolution logic to allow loading latest versions of assemblies when Addins are bound to older versions. Previously, if assemblies got updated in MM addins would fail to load - now addins will load as long as there are no breaking changes in the interfaces.


### 1.1.28
<i><small>February 13th, 2017</small></i>

* **Image Pasting via Image Dialog**  
In addition to pasting images directly into the document, you can now also use the Image Dialog to paste and preview images. If images are on the clipboard, the image is shown with an option to save it to disk before embedding into the document. You can also use the paste icon or Ctrl-V to paste an image into the dialog and then save it to disk with optimization. Added to make image pasting more discoverable. Also added Alt-I shortcut to the image dialog.

* **DisableHardwareAcceleration Configuration Key**   
Added this configuration key to allow disabling hardware GPU rendering acceleration. This option is available for the very few people that run into rendering problems with all white or all black initial screen loads.

* **Fix: Addin Button When No Documents are Active**  
Fixed bug where addin buttons would never enable when MM opened with no document active.

* **Fix: Image Relative Path when saving Images**   
Fix relative paths for images saved via Paste operations. Image path slashes fixed and image is now **always** pasted as a relative image if possible never using a physical path. Note: this may look ugly if the document hasn't been saved with a filename yet.

* **Fix erroneous version check Notifications**  
Due to the way we've handled versions in the past occasionally you could get stuck in a version update loop. Fixed version to use full version info  internally for display, processing and the online version file.

### 1.1.26
<i><small>February 10th, 2017</small></i>

* **Optimize Images pasted into Editor**  
When you paste images into the editor and save them to disk, images are now better optimized. jpeg images are saved with medium jpeg compression, while PNG images are asynchronously optimized in background with OptiPng.

* **Add Emoji Support**   
You can now embed common emoji tags like `:smile:` :smile: or `:camera:` :camera: as well as smiley syntax like `:-)` :-) or `B-)` B-) (the latter of which is not natively supported by GitHub) into Markdown documents and get them rendered. Note that actual output of these icons may vary in various browsers and the previewer. Here's a list of [emoji symbols and shortcuts](https://gist.github.com/rxaviers/7360908).


* **Markdown Renderer Options Configuration**  
We've enabled better fine tuning of the Markdown rendering options used in the Markdown conversion process. These settings largely map to the underlying [MarkDig parser](https://github.com/lunet-io/markdig), but you can now enable/disable various rendering features via the the `MarkdownOptions` settings in the Markdown configuration. For more info on options and how they work check the [MarkDig Features section](https://github.com/lunet-io/markdig).

* **Fix File Open Dialog Hotkey (ctrl-o)**  
Fix bug with File Open dialog which popped up browser dialog. Fixed.

* **Fix backup File and Cleanup Operation crashes**  
Make backup file loading and saving more resiliant in order to not cause a hard failure. Errors during recovery operations are now handled and logged.

* **Fix jumpy font sizing when editor first displays**   
Fixed issue with editor fonts resizing after initial display producing unwanted jitter. Initial load is handled at higher priority now so initial font setting shows on first load. 

### 1.1.24
<i><small>February 2nd, 2017</small></i>

* **DPI Scaling Updates**  
Change DPI scaling options during application startup which should result in cleaner High DPI scaling when running on a second monitor with different DPI settings. Best results are with .NET 4.6.2.

* **New Addins now don't require a Restart**   
Added logic to allow new addins to be loaded immediately after installation. Updates and deleted Addins still require an MM restart. MM now offers to restart for you when an addin requires a restart.

* **Addin Toolbar Dropdown Button now shows both Main and Config options**   
Apparently there was some confusion on the behavior of the Addin buttons where many took the drop down to mean the main functionality where it really provided configuration. Main button provides main addin (Execute) functionality, while the drop down now shows a menu of both the main functionality and configuration links.

* **Startup Speed Improvements**  
Moved additional Addin loading logic into background threads to improve startup time. Slight improvement in load time.

* **Paste Code Dialog Focus Hint for Language**  
The Paste Code dialog now sets focus to the language when code is pre-selected for quicker selection. In order to direct user attention to the language selection added visual hint to try draw focus more easily and add explict highlight for initial focus.

* **Weblog Post Image Uploads Replace Folder Name Spaces with -**  
When uploading images to Weblogs the parent folder name now replaces spaces with dashes. We're finding that featured image links by Facebook and Twitter will often not find images where spaces are found in the path. Replacing spaces with dashes provides more accepted URLs.

* **ActiveDocument.Title Property**  
For addin developers there's now a `ActiveDocument.Title` property that pulls the title from a # header, a Front Matter Title header, or the de-camel cased filename.

* **MarkdownMonsterAddin::OnWindowLoaded() Handler**  
You can now get notified when the Markdown Monster main window has loaded if you create an Addin. This makes it possible to add additional behaviors and things like Keybindings when the addin starts.

### 1.1.20
<i><small>January 24th, 2017</small></i>

* **Updated Addin Manager**   
Addin Manager now displays more information about each addin in a separate panel on the right. Long description and screen shot (if available) are displayed in addition to summary, version, and author information.

* **Paste Code Dialog Code Editor**  
The Paste Code dialog now shows a full code editor that makes it easier to preview and edit code you want to embed. The dialog also defaults to the code style selection when code is supplied to the editor when the code is empty.

* **Addin Interface Updates**   
A number of improvements to the the .NET Addin interface. Ability to move the cursor left and right, ability to find and select text and setting editor syntax all as part of `MarkdownMonsterEditor`. Addins now can have a `minVersion` attribute that specifies a minimum version of Markdown Monster required.

* **Fix: Addin OnCanExecute() Triggering**  
`OnCanExecute()` was not triggering through the commands bound to the menus. Fixed with explicit change notification bindings.

* **Fix: Edit Toolbar not active when no document is active**  
The editor toolbar now is disabled when no document is active. Previously the toolbar always stayed enabled.

* **Fix: Editor Triggered Hotkeys execute Out of Band**  
Some hotkeys are triggered from within the editor into the WPF application, which then could trigger code like a dialog popping up. This could cause race conditions if other editor code was called and also result in odd UI behavior especially if another editor was invoked. Changed code to execute out of band to avoid the nesting of editor code. This fixes a number of minor UI bugs in the various editor pop up dialogs.

### 1.1.16
<i><small>January 19th, 2017</small></i>

* **RecentDocumentsLength Configuration Option**  
You can now configure the length of the recent document list. Some people have requested to see a longer list of previous documents. The default remains at 12, but is now changeable via **Tools -> Settings -> RecentDocumensLength**

* **[Reusable Snippets Addin](https://github.com/RickStrahl/Snippets-MarkdownMonster-Addin)**  
We've added a new **Snippets Addin** to the Addin registry that allows you to create reusable text snippets. Snippets can contain C# Expressions or @Razor script and can be invoked via the UI or via expandable shortcut words.

* **Create Chocolatey Portable Install Package**  
Created a new Chocolatey package you can install with **choco install MarkdownMonster.Portable**. Package installs without admin rights into self-contained chocolatey folder without running an installer. All features except: .md and .markdown file associations, and mm.bat shortcut.

* **Remove external FontAwesome Font Dependency**  
Removed dependency on an external copy of the FontAwesome font. Fixed all internal references to point at the embedded FontAwesome resource in FontAwesome.WPF library.

* **Fix: Full Document Updates now retain their rough cursor position**  
Previously on the few occasions when Markdown Monster refreshed the entire document by reassigning the full markdown the editor cursor position was lost. Cursor position is preserved now.


### 1.1.14
<i><small>January 14th, 2017</small></i>

* **New Visual Studio Extension for Markdown Monster Project Template**  
If you want to create a custom Markdown Monster Addin, the process just got a lot easier with a [Visual Studio Markdown Monster Addin Project Template](https://marketplace.visualstudio.com/items?itemName=RickStrahl.MarkdownMonsterAddinProject) that you can pick up from the Visual Studio Extension Manager or the Visual Studio Gallery.

* **WebLog Endpoint Discovery (RSD)**    
Implemented Real Simple Discovery protocol in the Weblog Addin when specifying WebLog Publishing endpoints. RSD allows for links to an RSD resource to be embedded in a home page via metadata. The RSD file can be read and discover publishing endpoints. Integrated on the WebLog configuration page via a new Search button that tries to find the endpoint url and blog id.

* **Fix Ctrl-O and Letter O Insertion Bug**  
Fixed annoying problem when using Ctrl-O (Open Document) which would inject the letter O into the open document. Worked around this IE key handling with blurring/re-focusing. Fixed.

* **Force full Page Refresh when `<script>` is embedded in Content**  
If the Markdown content contains literal script tags that need to be executed the Previewer reverts to using a full page refresh. Normally the previewer merely updates the content which is much faster and less janky when redisplaying the preview content. This fixes problems for Addins like the [PasteCodeAsGist](https://github.com/RickStrahl/PasteCodeAsGist-MarkdownMonster-Addin) that embed script code into HTML to display their content.


### 1.1.12
<i><small>January 10th, 2017</small></i>

* **Spellchecker to skip Code Snippets**  
Made change to have the spell checker not check code snippet blocks. This makes code centric markdown much cleaner and also puts less load on the spell checker's background processing while typing, since code blocks often have many spelling errors.

* **WordPress Featured Image**   
WordPress post uploads now select the first image and set it as the featured image on the post. This feature has been much requested, but might need some tweaking to get the right workflow for selecting images. If you have ideas on how to handle this from a UI flow, please post an issue.

* **Force Focus to Editor when Activating Form**  
Changed behavior so when the editor window is activated, focus is always forced into the editor so no matter where you click on the window you can just start typing again. This also avoids the occasional funky focus bug where the document 'overselects' the entire visible area just by moving the mouse over it.

* **Add support .markdown**  
Add support for .markdown files as Markdown files that can be previewed and edited with the full Markdown experience.

* **Save WindowState on Exit**  
The WindowState (Maximized,Normal,Minimized) is now saved when exiting and restored when returning.  
*(credit: Alex Wiese)*

* **Title to filename when saving Untitled documents**  
When you save new documents the filename defaults to the first line's text that start  `#` (header).  
*(credit: Alex Wiese)*

* **Fix: Tab Activation Editor Focus**  
When activating a new tab focus is now properly set into the editor at current cursor position.

* **Fix: Native Screen Capture on Secondary Monitors**  
High DPI window selection on secondary monitors previously failed to select windows properly due to High DPI issues in WPF. Fix requires .NET 4.6.2. 4.6.2 supports per monitor DPI scaling in WPF and that will now be respected but only if the machine is running 4.6.2. Older version of .NET continue to get the buggy offset behavior previously seen. Note: Primary monitor captures should always work fine.

* **Fix: Spell Checker Churn**   
Spell checker was activating too frequently when switching between tabs as the check interval was doubling up. Fixed.

* **Fix: Save And Edit Screen Capture**  
Fix issue where files wouldn't be referenced properly to open in the specified external editor not dealing with the extended paths. Fixed.

* **Fix: Html to Markdown Conversion**  
Due to security changes pasting Html to Markdown and downloading of Html based blog posts was failing temporarily. Fixed.

* **Fix: Reduced Binary Size**   
Reduced the main executable size by 40% by removing a number of unused and duplicated resources. Should slightly improve startup time. `MarkdownMonster.exe` now clocks in ~1mb, down from nearly 1.8mb.

* **Switch to .NET 4.6.1 Runtime**  
.NET 4.6+ includes a number of stability and high DPI enhancements for WCF and 4.6.1 been a recommended update for over a year now, so distribution should be wide. Ideally we'd use 4.6.2 which would fix additional problems with screen capture, but distribution of 4.6.2 is just too low at the moment.

> ##### .NET Runtime Breaking Change: Addin Authors
> This version switches to .NET 4.6.2. While we expect that most users won't be affected by this switch as they have 4.6.2 installed, any previously built Markdown Monster add-ins have to be recompiled against 4.6.2 to compile properly. Installed addins should continue to work however, so this only affects **addin authors** at compile time.

### 1.1
<i><small>January 2nd, 2017</small></i>  
<small>Version Rollup release</small>

* **Add Presentation Mode**   
Added presentation mode that previews Markdown in a full screen preview browser. You can toggle Presentation Mode with F11 or by using menu and title bar buttons.

* **OpenInPresentationMode Configuration Flag**  
You can now set MM to open in presentation when you open MM or when click on a markdown document in Explorer. Useful for those that just want to view Markdown documents rather than editing them by default.

* **Weblog Addin Configuration Button**  
The WebLog addin configuration button now directly opens up the `weblogaddin.json` file to allow customization.

* **New About Window with Acknowledgements**  
Added acknowledgements and &copy; for libraries used to the About form. Without these tools MM would not be what it is today!

* **Auto Save Documents**   
There's now an option to automatically save documents as you type. Documents on disk are updated in nearly real time when you stop for more than a second of typing. This option is disabled by default and can be enabled by setting the `AutoSaveDocuments` configuration setting to `true`. Note this setting overrides the `AutoSaveBackups` configuration option.

* **Add Front Matter to new Blog Post**   
Weblog Addin configuration option `AddFrontMatterToNewBlogPost` and `FrontMatterTemplate` that is prepended to the beginning of a post.

* **RememberLastDocuments Configuration Setting**  
This new configuration setting determines the number of open documents to remember and pre-open when MM is restarted. The default value is 3 which opens the last 3 documents that were open when MM was shut down last. Any number can be used and documents up that number will be remembered. This option replaces the old `RememberOpenDocuments` setting which opened all previously open Windows. To retain the old behavior, just set the number to a large value like 10. Lower numbers will improve load time and keep your MM window better organized.

* **Default Save Location to Libraries Folder**  
The default save folder now is set to the Windows Libraries folder that shows your Roaming profile documents/pictures/cloud Drive folders. Used as last resort - preferred location for save is last saved file location.

* **Save As Html Additions**  
There's now a new option that purports to save Html and all dependencies. Currently this option defers to View As Html and then using **Save As...** in the browser to save Html and all dependencies. We'll add true direct saving of full dependency files later. This is a stopgap to provide information on how to accomplish this task for now.

* **Addins now install to %AppData% Folder**  
Addins now install to the `%AppData%\Markdown Monster\Addins` folder to make it easier for the Portable version to use Addins without requiring special permissions. Existing Addins are moved to the new location.

* **Fix Weblog Image Folder Naming**   
Fixed several issues related to invalid image names sent to server stripping out `#` and other invalid path characters from the image 'path'.

### Version 1.0.30 
<i><small>December 20th, 2016</small></i>

* **Switch Exe to 32 bit Application**  
Switched MM to run as 32 bit application for interop stability and better performance. 32 bit is faster and more reliable in the COM Automation tasks used to interact with the WebBrowser controls used in MM. Editor performance should be slightly improved and preview syncing should be considerably more reliable.
> **NOTE**: When upgrading from a version below 1.0.30 please uninstall the old version before installing this one, to ensure the older 64 bit version in `C:\Program Files\Markdown Monster` - is cleaned up as the 32 and 64 bit versions use separate install signatures. The new version now lives in `c:\Program Files (x86)\Markdown Monster`. Otherwise you can separately uninstall the 64 bit version explicitly from Windows' **Programs and Features**.

* **Improved Dirty Document Handling**   
Dirty document detection (* next to file name) now properly checks actual file content rather than just scanning for doc changes. Document status is reliably updated and back up files are kept more up to do date with backup files deleted once document is no longer dirty.

* **Preview Sync Enhancements**  
Add a number of optimizations and tweaks to the preview sync functionality to provide more consistent cursor location for editor->preview syncing. 32 bit helped significantly here and also added additional logic to find lines by reading forwards and backwards to find a close location match.

* **Add `<loadFromRemoteSource>` to Config**   
Add this configuration option to allow addins to be loaded from network or non-installed locations. This makes it possible for a portable MM install to run off a network drive which was previously failing to load addins.

* **Addin Dialog Improvements**  
The addin dialog is now modeless and can run in the background. Add in listings now show a 'more info' link to take to GitHub site of publisher.

* **Logging Improvements**   
Log now properly marks the last chance exception handler (DispatcherUnhandledException) to log this info - these are typically unrecoverable errors and are now identified more clearly (and can hopefully be addressed more urgently).

### Version 1.0.28
<i><small>December 16th, 2016</small></i>

* **Experimental Auto-Backup Implementation**
We've implemented Auto-Backup for now behind a `AutoSaveBackups` configuration flag that defaults to `false`. When set MM creates a `*.save.bak` file that shadows the actual edited file. The file is updated whenever text changes and acts as a live backup in a crash. When the original file is reopened in the editor MM notices the backup file and opens both files for comparison.

* **Option to disable all Addin Loading**  
You can now disable loading of all addins in Markdown Monster, which might be useful for debugging any load failures or to improve startup time. More of a debugging feature. Keep in mind that the ScreenCapture and Weblog features are implemented as addins and are not loaded when this option is enabled.

* **Fix Editor Focus Bug related to Preview**   
Occasionally the editor refused to properly get focus as it got stuck in an endless Preview refresh loop. Fixed by properly reloading the entire page in the browser to ensure LoadCompleted is fired. Fixed.

### Version 1.0.26
<i><small>December 13th, 2016</small></i>

* **Print Preview Html**  
You can send the HTML output of the preview HTML viewer to a printer or PDF writer (if installed) using the **File -> Print Html Preview**.

* **EditorAllowRenderScript Configuration Flag**   
Added a `EditorAllowRenderScript` configuration flag that allows you to determine how embedded `<script>` tags are rendered in the HTML output. If `true` script tags render as raw script and the script executes, if `false` script is rendered as encoded text.  
***Note:** Regardless of this setting, both inline or block based code snippets always render `<script>` code as encoded text.*

* **Add support for Featured Blog Post Image**   
You can now add a featured image to your post that creates a `wp_post_thumbnail` custom field when data a post is made to the server. There are 2 ways to get a thumbnail url posted: The first image is in the post used by default. Otherwise you can explicitly provide a URL in the `featuredImage` meta data field for the post. The latter is especially useful for existing posts.

* **New Addin Manager** (experimental)  
Added a basic Addin manager that hooks up to an Addin repository to display available addins. Currently, there are on a couple of addins but hopefully that will change in the future. Updated addin install to use separate folders for each addins so existing addins and existing addins installed in the `.\Addins` root are removed.

* **Fix: Broken Icon Images**  
Fixed installer to provide proper Fontawesome font install.

> #### Breaking Change
> * **Existing Addins removed**  
> Due to the new Addin manager and install process addins now install into separate folders. If you have custom addins you've created or installed move them to a folder with a unique name and make sure the addin assembly is named with an `<addinName>Addin.dll` pattern. The **Weblog** and **ScreenCapture** addins are automatically installed into their respective folders by the newer version installer but all other addins need to be reinstalled or moved. 
>
> * **System PATH Bug**  
> Earlier installations of MM appended the install path to the system path every time it was installed resulting in many MM path references. To clean up this accidental mess, we've provide a `FixSystemPath.ps1` you can run as administrator from a Powershell console to clean up the paths. We apologize for not catching this sooner. 

### Version 1.0.22
<i><small>December 8th, 2016</small></i>

* **Distraction Free Mode**  
Added distraction free mode option on the window menu. When applied removes the toolbar and and preview window.

* **Portable Version**   
Added build step to produce [a portable version in a Zip file](https://markdownmonster.west-wind.com/download.aspx) that can be unzipped anywhere and run without a full installation. Some limitations apply: No .md file association, no command line PATH mapping for install folder (no `mm` or `markdownmonster` from command line), no desktop icon and - depending on where you run from - inability to install addins using the Addin Manager (you can install manually or run as Administrator to override permissions).

### Version 1.0.21
<i><small>December 5th, 2016</small></i>

* **Add support for Vim and Emacs Keybindings**  
Added new `EditorKeyboardHandler` configuration setting that lets you switch editor input handlers using `default`,`vim` and `emacs`.

* **Fix: Href Dialog External links to use _blank**  
When choosing to paste an external link with the HREF dialog the generated HTML now uses `target="_blank"` instead of `target="top"`. Result is that links are opened in a new tab/window **each** time the link is clicked.

* **Remove Binaries from Source Github Repo**  
Removed all binary builds from the source code repo as the repo was getting massive in size. Filtered out all exe and zip files from history reducing repo download to 30M vs close to 300M.

### Version 1.0.20
<i><small>December 3rd, 2016</small></i>

* **WebLog Publishing/Download Updates**   
Add better support for DasBlog which has a few funky MetaWeblog quirks. Fix MetaWebLog API post handler to allow string or numeric ids. Fix 

* **Fix: Screen Capture on Scaled DPI**  
Previously the native screen capture form had issues on scaled displays and would not properly highlight windows. Fixed.

* **Fix: File Updates on Disk Dialog Issues**   
Under some circumstances with certain hardware combinations, files changed on disk while editing were throwing repeated message boxes to notify of changes in an endless loop. Fixed - props to Peter Kellner for reporting and stipulating the problem.

### Version 1.0.18
<i><small>November 28th, 2016</small></i>

* **Fix High DPI Font Scaling for Editor and Preview**  
The preview browser now uses standard system settings for automatically scaling the HTML preview based on Font Scaling settings in Windows. The Editor also adjusts the hard editor font settings based on DPI and adjusts with a ratio so that when you move your window to a different monitor the font display size should stay relatively the same regardless of the difference in font scaling factor.

* **Preview Scroll Sync Updates**  
The preview scroll syncing now can quickly set the four available options via a new dropdown on the statusbar:
![](http://markdownmonster.west-wind.com/docs/images/PreviewModeSelection.png)

* **Add EditorFont Configuration Options**  
You can now set the editor font in the Markdown Monster Settings file `MarkdownMonster.json` (Tools -> Settings). Note that fonts **have to be proportional** or severe cursor positioning issues will arise. This moves all font manipulation of the editor to the configuration to ensure that cursor position corruption does not occur.

### Version 1.0.15
<i><small>November 25th, 2016</small></i>

* **New Window Scroll Options**  
You can now set the new `PreviewSyncMode` setting in the configuration to specify `EditorToPreview`, `PreviewToEditor`, `EditorAndPreview` or `None` to determine how the preview and edit panes react to scroll operations. Default is `PreviewToBrowser`.

* **File Encoding Support**  
Markdown Monster now preserves original file encoding of documents for UTF-8 with and without BOM and for Unicode files. Default if no BOM is available is always UTF-8.

* **Active Line Highlight**  
Added a `EditorHighlightActiveLine` configuration setting to set the active line highlight in the editor which makes it easier to see where the cursor is when scrolling is synced. New setting is true.

* **Add Image Preview to the Image Link Dialog**  
When you link images, the image dialog now previews selected images when you pick a file from disk, or when you type or paste a URL into the edit box. The editbox now also automatically picks up image Urls if they are on the clipboard prior to accessing the Image form.

* **Inline Code Menu Option and Shortcut**  
You can now markup inline code inside of a paragraph using `<code>` syntax via new toolbar option using the **ctrl-`** keyboard shortcut.

### Version 1.0.9
<i><small>November 18th, 2016</small></i>

* **Add support for Front Matter**  
When rendering Front Matter headers (`---` delimited block at beginning of Markdown text) is now stripped for rendering. When posting to a Blog, the title if available is pulled from Front Matter content.

* **Updated UI for Screen Capture**  
Updated the UI to capture screens with single clicks rather than drag and hold the mousepointer over a window. Simply move the mouse when in capture mode and the selected window/control will highlight. The image preview now auto-resizes better for small images.

* **Delayed Screen Captures**  
If you specify a delay value for screen captures the native screen capture now respects it by displaying a count down counter in the lower right corner of the screen before the capture. This allows opening menus and other operations that normally would require mouse clicks. Screens are also pre-captured and displayed from an image. 

* **Capture Cursor for Screen Captures**  
You can now optionally capture the cursor when capturing the screen using the native screen capture tool.

* **Add OnSaveImage to Add-in Interface**  
The add on interface now supports notification whenever an image is to be saved. You can now intercept image save operations and hook your own custom operation into the save. Return true to consider the save operation handled or false to have the default processing take place.

* **Fix Draft Posting of Blog Posts**  
Posting Weblog posts as drafts now works as expected. The draft setting is also persisted into the post meta data stored with a Weblog post so that subsequent reloads automatically reflect the setting until explicitly changed.

* **Preview Sync fixes**  
Fixed a number of issues related to the new preview sync feature that would occasionally cause the preview and editor to erratically switch focus and slow down when rendering content. Fixed `<script>` tag rendering to always encode un-encoded script tags (which caused all sorts of havoc due to JS errors). Ignore scroll sync errors that previously forced a full page refresh. Fixed a few edge cases where a full preview re-render was triggered to still inject updated HTML.

### Version 1.0.3
<i><small>November 8th, 2016</small></i>

* **Preview syncing to Editor**  
The preview now syncs to the editor position by default. When you move or type in the editor the preview now scrolls to the active text and highlights the text. This behavior is controlled via the new `SyncPreviewToEditor` config setting.

* **Paste Images from Clipboard**  
You can now paste images from the clipboard into Markdown Monster. When an image is pasted a dialog prompts to save it to disk and if saved, is linked into the document.

* **Fix Wordpress Import**  
Fix importing from Wordpress blogs where the **Read More** option is active (for abstracts).

### Version 1.0 
<i><small>November 4th, 2016</small></i>

* **RTM Release**  
We're happy to announce that Markdown Monster has gone to RTM!
