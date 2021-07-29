# Markdown Monster Change Log

[![download](https://img.shields.io/badge/Download-Installer-blue.svg)](https://markdownmonster.west-wind.com/download)
[![Chocolatey](https://img.shields.io/chocolatey/v/markdownmonster)](https://chocolatey.org/packages/MarkdownMonster)
[![Web Site](https://img.shields.io/badge/Markdown_Monster-WebSite-blue.svg)](https://markdownmonster.west-wind.com)

### 2.1

<small>not released yet</small>

* **Update: PDF Generation Tools**  
Update to latest tool version of wkhtml2pdf for PDF generation which fixes a number of small rendering quirks in the PDF generation engine. Also added some rudimentary support for emoji rendering in PDF documents since that seems a common theme.

* **Fix: Favorites Tree Data Entry**  
Fix various issues with the Favorites list where entering a new item would add two items and focus would switch unpredictably. You can now also navigate the list with the keyboard with `Enter` opening the item with editor focus and `Space` opening without editor focus.

* **Fix: Startup Menu Folder Opening**  
Fix issue Startup Screen folder opening issues where clicking on the folder icon next to the file, would not reliably open the Folder Browser. Fixed.

* **Fix: Search Settings in Settings Dialog**  
Fix issue with Search Settings entry which was hanging up and not immediately refreshing the entered text. Reduced delay and ensured that operation occurs in dispatched mode to see the change.

* **Fix: WordPress Publishing Thumbnail Image Failure**  
Fix issue with the WordPress publishing mechanism where if thumbnails are published the addin would fail due to a data type error.

* **Fix: EnableBulletAutoComplete**  
Fix this setting so it works on initial document load. Previously the setting only worked after the document was 're-activated'. Fixed.

* **Fix: Edit CenteredMode Pixel Width in Menu**  
Fixed focus issue in the Centered Mode editor document width that determines how wide the editor column to actually edit is. This allows creating of white space around text if the size of the overall edit pane is larger. Fixed issue where focus could not be set easily in the menu option, which made it hard to edit the value. Fixed.

### 2.0.3 - Official release of v2.0

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
Support binaries are now moved out of the root folder into a `BinSupport` subfolder to avoid ending up on the User's path and causing naming conflicts. Only applications that should be visible on the user path now are: `MarkdownMonster`, `mm` and `mmcli`.

* **Make Settings HelpText Selectable**  
You can now select the help text associated with a configuration setting in the Settings window. This allows picking up URLs and other fixed values more easily. (#817)

* **Dev: Add Debug Editor and Preview Template Paths**  
Added configurable Editor and Preview Template paths that are configurable and allow pointing the template folders to the original development folder, rather than the deployed applications' folders. This allows making changes to the Html/Web templates without having to recompile code. Settings are `System.DebugEditorHtmlTemplatesPath` and `System.DebugPreviewHtmlTemplatesPath` and they default to `.\Editor` and `.\PreviewThemes` which are fixed up at runtime.

* **Fix: Remove WebViewPreviewer Addin from 1.x Installs**  
Added logic to remove the WebViewPreviewer Addin from v1 which throws an error. If found this addin is now explicitly removed at startup since the code has moved inline.

* **Fix: PDF Generation Errors**
Fix issue where repeated output to PDF would report an error for PDF generation even when the PDF was generated.

* **Fix: PDF Code Snippet Background**  
Fix issue where the PDF output for code snippets was not properly applying the background color. Works in HTML but not for the PDF generator. Added custom print stylesheet overrides for `pre > code` style explicitly to match highlightjs color theme.

* **Fix: Folder Browser Click and DoubleClick Behavior**  
Fix issues where clicking would not allow keyboard navigation after click, folder opening wasn't opening folders on first click, and preview operations could hang.


### 1.28.4

<small>July 2nd, 2021</small>

* **Fix addin loading for new Addin Repository Urls**  
Due to the upgrade to v2 all the Addin repository URLs have been broken for v1. This update fixes these URLs and can now again load v1 versions. Note for all older versions addins will no longer load from the addin manager as all addins have been updated for v2.


### 1.28 
<small>June 8th, 2021</small>


* **Create Link from Document Outline Anchor**  
You can now create a link in selected text from a header link in the document outline. A new context item creates a markdown link from the current text selection with the header ID for the link navigation.

* **Hierarchical Configuration Settings for Project, Marker Files**  
You can now override global configuration settings via JSON file settings in Markdown Monster Project Files (in the `Configuration` node), or in the `.markdownmonster` marker file. In both cases you can use JSON to override default configuration settings like font-size, editor and preview themes, formatting, line numbers etc.

* **Add Default Search Engine Configuration Option**  
For Web lookups you can now select a Search Engine (DuckDuckGo, Google, Bing) for opening the browser on a search page. Set via the new `WebBrowserSearchEngine` configuration setting in settings.

* **Set Table Type when using Edit Table**  
When editing Markdown or HTML tables, the table type is now properly set when the table editor is opened. Previously the default Pipe Table was used.

* **Updates to the Table Editor**  
Fix some navigation issues. Update editing field height to provide more consistent display of new fields when they are injected. Smaller font size - previously the font-size was larger than the default template.

* **Fix: Open From Url GitHub Repo Fixups for `Main`**  
When extrapolating repository URLs MM now checks for `main` in addition to `master` branches for default documents and for repo URL fixups.

* **Fix: Table Editor Autosizing for Table Cell Editing**  
Table cells now auto-grow in height as you are editing them when adding linefeeds or overflowing at the end of the line.

* **Fix: Table Editor Crash**  
Fix Table Editor crash when reformatting mismatched table column counts. If a table had fewer columns for a row it would crash in some situations. Missing columns are now auto-created as empty columns.

* **Fix: Drag and Drop Selection in the Folder Browser**  
Fix issue where selecting an item would not always drag the correct item (due to invalid tree item selection). Items are now explicitly selected before dragging.

* **Fix: Search Panel Result Selection Behavior**  
Search panel result clicks now open search results in preview-only mode, only if the tab is not already open. Double click now opens the document as non-preview document. Fixes issue where a search result in an existing window might close an already open window.

### 1.27
<small>April 30th, 2021</small>

* **[Rewritten Table Editor](https://markdownmonster.west-wind.com/docs/_53a0pfz0t.htm)**  
Completely revamped the Markdown Table Editor to better support larger tables and quicker editing support. Editing now uses the current theme in the editor, and there's an optional previewer which also uses the current Preview Theme. There's new support for sorting and alignment of columns, as well as improved output and parsing support, plus much more.

* **[Search Web and Search Web and Link on Editor Context Selection Menu](https://markdownmonster.west-wind.com/docs/_4xs10gaui.htm#embed-web-links)**  
New option to allow searching for content on the Web by opening the browser from the selected text and another option that performs a search and displays a list of matches with URLs on a sub menu that can be auto-linked to the selected text.

* **[Web Search and Search Web Links on the URL Dialog](https://markdownmonster.west-wind.com/docs/_4xs10gaui.htm#embed-web-links)**  
The same two search options from above are also available in the `ctrl-k` URL Link dialog as links below the text input. Clicking the links searches the Web with the display link. For the Browser search any URLs on the clipboard automatically replace the URL text if it's empty. For the inline search, a list selection fills the field.

* **Search Weblog Folder**  
New Menu option on the Weblog folder that lets you search the WebLog folder for posts using the built-in [Find in Files Search](https://markdownmonster.west-wind.com/docs/_5y715t8co.htm#find-in-files) functionality.

* **Preview Sync Improvements**  
Refactored some of the logistics in Preview Sync which should improve responsiveness of preview refreshes. There should now be fewer odd instances where preview is not refreshing automatically. Also updated  `Left-ctrl` which forces an immediate refresh, spellcheck and stats update.  

* **Drag and Drop Files into Editor as Links**  
When you drag and drop a document file (Markdown, html, pdf, zip) from Explorer or the File Browser into the editor, the file is now linked rather than 'opened'. The link is created as `file.ext` with a document relative path. 

* **[Document Outline Line Selection and Navigation](https://markdownmonster.west-wind.com/docs/_55o1bd4n1.htm)**  
Document Outline Navigation now moves the cursor position to the selected line in addition to scrolling to the appropriate Viewport position. When navigating by keyboard, ENTER and SPACE selects, and TAB moves the focus into the editor.

* **Add Folder Opening to Recent File List**  
There are now small folder icons next to files in the Recent Files dialog to allow opening of the associated folder in the Folder Browser to facilitate quicker access to files.

* **Markup for Empty Selections puts Cursor into Selections**   
When using shortcuts for Markdown markup like `ctrl-b` or `ctrl-i` for empty selections, MM now moves the cursor into the generated empty Markup. So `Ctrl-b` generates `**~**` where `~` denotes the cursor position (`~` not generated).

* **Improved Large Document Support**  
Added improvements to make MM work better with very large documents, by reducing preview refresh overhead, dynamically expanding the refresh timeout and tweaking the update process. Also - Using the built-in Chromium addin as the previewer now refreshes off the UI thread so that refreshes no longer freeze the editor while updating the preview for large documents.

* **Add mmCli Path Expansions for Environment Vars and ~ Home Path**  
Fix `mmcli.exe` to respect current folder and relative files for input and output files as well as expand Environment Variables and `~` for the Home Directory folder.

* **Add Editor Option for No AutoCompletions**  
You can now optionally disable all editor completions via a new `NoAutoComplete` Editor configuration option. This option disables all key expansions like quote and bracket completions, list bullet expansion etc. for a raw editor experience.

* **Fix: Browser Executable using Default Browser**  
Fix *View in External Browser* when the default browser executable is left blank - defaults to the system browser but executes using command line to allow for URLs with `#hash` extensions.

* **Accessibility Updates**  
Some adjustments to the accessibility features in the file browser and search features.

* **Fix Cut Behavior from Context Menu**  
Fix broken Context Menu Cut behavior which was deleting the text but not putting it on the clipboard. 

* **Fix: Preview To Editor Sync in IE Mode**  
Fix issue where preview to editor sync mode jumps the cursor to the top of the editor when typing.

* **Fix: Open Folder Browser when no Document Active**  
Fix issue where the various folder browser menu options weren't enabled when no document was active. Also fixed hotkeys.

* **Fix: PreviewWebRootPath in `.mdproj` Files not respected**  
* Fix issue where the `PreviewWebRootPath` value in the `.mdproj` files wasn't respected in some situations. The file is now checked on every render operation so changes in the `.mdproj` are immediately reflected.

* **Fix: Multi-Selection Issues in Folder Browser**  
Fixed several issues with inconsistent behavior with multi-selected files in the Folder browser. Reduced selection jitter and better handle accidental starting drag and drop operations.

* **Fix: Chromium Initial Preview Refresh Issue**  
Fix issue where the Chromium Previewer would occasionally not display content when first loading Markdown Monster or when switching from IE view to Chromium Preview. Caused by async load of the Preview browser, we now explicitly check for completion and if not ready wait for the browser to be ready to preview.

### 1.26
<small>February 4, 2020</small>

* **Chromium Previewer Browser Addin**  
Added a built-in addin that can toggle between the native IE based preview browser and a new WebView2 based Edge Chromium  Preview browser. This addin provides better compatibility with common browsers and allows support for some related technologies like Mermaid Charts to render natively.

* **Folder Browser Find Files Search Box**  
Changed the Find Files Search Box to be always visible in the Folder browser above the browser instead of an explicit dropdown panel. This search finds and filters by file name in the tree and shows a tree based match filter. The search box sits above the directory tree is now always visible and accessible via Ctrl-F from within the browser. You can optionally specify to search in sub folders.

* **Update Find in Files to Incremental Search**  
Change the Find in Files sidebar to use live searching instead of using an explicit Search button. Better handle results display and navigation in the result view.

* **Updated Find in Files Operation** 
Find in Files searches content inside of files to find matching content. The result displays a straight list with match counts for the files. Selected files are opened in the editor with the search term selected in the Find box (and Replace box if you specify Replace text).

* **Update Open Document Change Detection**  
Updated logic to handle change detection on open documents. When documents are open and unchanged, the document is immediately updated with changes from disk. If the open document has changes and the underlying document changes, nothing happens until you save. A new dialog allows you to choose between your version, their version or to run the configure Git Diff tool to compare versions and merge changes.

* **Add PDF Page Sizes to Save To PDF**  
Added all additional supported PDF print formats to the paper type dropdown on the PDF Export dialog. Adds all Ax and Bx types as well as various named types.

* **Favorites Click Behavior Update**  
Favorite clicks now open documents initially in preview mode until you type into the editor. Preview mode documents close as soon as another tab is opened or accessed. This behavior is now the same as the main folder browser. Double clicking forces the opened document to be a permanently open document. Unlike the folder browser, a single click forces the cursor into the Favorite document selected.

* **[Markdown Monster Web Server Enhancements](https://markdownmonster.west-wind.com/docs/_5s1009yx1.htm)**  
Markdown Monster includes a local Web server that can now be used to open new or existing documents and retrieve document content from the active document. Added support for retrieving the active document's content. Added Web page examples that interact with Markdown Monster. There are also new `-startwebserver` and `-stopwebserver` flags to start and stop the Web server to ensure that the local server is running. Added Web Browser HTML samples in  `SampleDocuments\BrowserIntegration` that demonstrate how to interact with MM from a Web page. 

* **Update Mermaid Rendering in the Preview**  
Mermaid support in MM has always been minimal since it uses the Internet Explorer engine for the preview pane. Mermaid recently removed their already terrible support for IE completely, so MM now renders a placeholder rather than the Mermaid chart in the default (IE based) previewer. The placeholder includes a clickable link that opens your default browser and displays the document containing the diagram and navigates to the Id of the mermaid diagram. Note in the Chromium addin previewer, Mermaid charts are properly rendered inline.


* **Fix: Preview Refresh for Ctrl-key operations**  
Fix issue where some common ctrl-key operations (ctrl-z, ctrl-x etc.) would not immediately update the preview until manual key entry into the editor is performed. Changed operation that the `ctrl` key now triggers a preview refresh for all ctrl-key operations. This also means that the `ctrl` key becomes the effective 'immediate refresh' key that you can use to force a preview refresh even when no preview sync is in use.

* **Fix: Weblog YAML Parsing for customFields**  
Fix bug where an empty `customFields` value would cause the Weblog Entry to not pre-load values back into the Weblog Publish dialog for repeated publishing. Fixed issue and changed defaults to not render empty field in the first place.


* **Fix: Links in Headers for Document Outline**  
Fix bug where links in headers where showing incorrectly in the Document viewer. Fix parses links and retrieves the text properly for display in the Document Outline. Also fixed in the TOC embedding logic.

* **Fix: Blog Post MetaData**   
Fixed a regression in posting to a blog where server generated values on new posts - the PermaLink and FeaturedImageUrl - were not updated in the meta data. These values are now updated and written back into the FrontMatter meta data again. Fix bug where PostId was not updating in captured meta data after posting.

* **Fix: Split View Editor Styling**  
Fix bug where split view would not open with the same styling as the main view if the theme was not the default theme.

* **Fix: Theme Switching Issues**  
Fixed bug that would crash MM when switching themes in MM Multi-Window mode. Also fixed timeout delay for forcing a restart after theme switching which previously would launch the updated instance too quickly and so fail to load in Single Use mode.

* **Fix: Various Open In Explorer Operations**  
Fixed various **Open In Explorer** operations where folders would not open properly in Explorer when the path contained inconsistent path delimiters. This broken in the Git Explorer as well as manually entered mixed paths.

* **Fix: Preview to Editor Sync**  
Preview to Editor sync was not working correctly due to an omission check.

* **Fix: Chromium Previewer Addin ScrollSync**  
Fixed scroll issue in the WebView2 control interop that would cause scroll failures when scrolling the preview and trying to sync the editor. Fixed with non-cached interop object instances to avoid potential operational overlap (WebView2 bug).

* **Fix: mmCli Html Package Exports**  
Fix issue where HTML self-contained Package exports were failing due to an assembly binding error. Added `.config` file to ensure correct bindings are used.

* **Fix: mmCli Relative File Paths**  
Fix file path translation for relative paths.

* **Fix: Jekyll Publish Flag**  
When publishing to a Jekyll flag, fix the publish flag to correctly set the publish status as published or hidden.

### 1.25
<small>November 12, 2020</small>

* **Multi-File Selection in Folder Browser**  
You can now select multiple files in the folder browser and perform many of the common operations on multiple files. You can open multiple files, copy and paste and drag and drop multiple files among folders or to Explorer, delete multiple files and so on. Also updated FolderBrowser API for addins to allow `GetSelectedItem()` and `GetSelectedItems()`. Multi-select also works with keyboard using `Ctrl-arrow` or `Shift-Arrow` for multi-select and range selects respectively.

* **Updates to Folder Browser Operations**  
Cleaned up folder browser file management tasks: New file now focuses the editor with the new empty document. Folder browser navigation now stays focused on the active selected item in more situations so you can perform another task like edit and then jump back to the Folder Browser (`alt-v-f`) and continue navigating the tree. Improved keyboard navigation functionality. `Ctrl-N` is now used as the default key in the Folder browser for new files. New files are created with a name editor in the folder browser and are then opened in the editor. This differs from `Ctrl-N`/**File -> New** in the editor which creates an `Untitled` document which prompts for filename only when saving.

* **File Stem Selection in Folder Browser**  
When selecting files for renaming in the folder browser, the filename without extension is now highlighted when first entering the name editor. Makes it quicker and more reliable to rename a file. Also fix file renaming message when edited filename is not changed.

* **Default Image Location for Untitled Doc Images**   
When saving images into a new document that has no filename, the default image save folder now defaults to the open folder browser location (instead of *Documents*). Subsequent saves for the same document remember the last image save location.
  
* **New command line option to open MM document at specific Line**  
You can now use the new `-line` command line option to open a document loaded from the command line at the specified line number.

* **FrontMatter in Blog Posts: Preserve Custom Properties**  
Added support for preserving top level properties in FrontMatter when Weblog Meta data is created. Previously the FrontMatter was serialized directly into the Weblog Meta structure and written back out using just that schema which lost custom props. They are now maintained.

* **Improve Jekyll Post MetaData Handling**  
Related to the FrontMatter improvements for blog posts, the meta data for Jekyll blog content has been updated to better support the category and tags lists.

* **Allow for `.markdownmonster` as Root Folder Indicator and External Configuration**  
Set up addin handlers that can find `.markdownmonster` file and use it for custom project level addin configuration. For example this JSON file can contain custom, project level configuration that can be used to stored for an addin. For example, a Deployment addin might hold server/auth configuration.

* **Add `pagebreak` Default Snippet to Snippet Manager**  
Added a `pagebreak` default snippet to the Snippet Manager Addin, so it's there by default when MM first creates the Snippet Manager defaults. This is optional and can be removed but is one useful use-case of using a snippet.



* **[Addins: Updated Markdown Monster Add-in Project Visual Studio Extension](https://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm)**  
The [Markdown Monster Addin Project Extension](https://marketplace.visualstudio.com/items?itemname=rickstrahl.markdownmonsteraddinproject) now creates much simpler, SDK style .NET projects. Projects now run and debug out of the box after running the New Project Template without additional manual configuration, as was required by the older version. It's also considerably easier now to configure for custom MM install locations using the raw XML Project file.

* **Addins: Add `ContextMenuOpening` Events to various Context Menu Renderers**  
Added static  `ContextMenuOpening` events to the various context menus that are dynamically generated such as the editor, tab, folder browser preview context menus. These events can be hooked and allow adding (or removing) of menu options at runtime, typically from Addin code. Handler is passed the ContextMenuWrapper class and the actual `ContextMenu` instance as Event parameters.

* **Fix: Icon Placement on multi-monitor Setups**  
Fix odd bug where the MM icon would not show on the correct screen in some multi-monitor scenarios. While at it also fixed the startup icon flash when loading MM onto a non-main screen.

* **Fix: Set Editor Position offset to be properly 1 based**  
The editor position now properly shows the Line and Row position in the status bar as 1 based. ie top of the document is `Ln 1, Col 1` rather than `Ln 0, Col 0`.


* **Fix: File Encoding bug in the Embed Url Dialog**  
Fixed issue where files with spaces in the filename would not URL encode correctly. New behavior doesn't encode but replaces spaces with `%20` in order to allow the Markdown Parser to properly parse Urls.


* **Fix: Window Outline showing on multiple Windows desktops**  
Provided a workaround for a problem whereby MM casts an 'outline frame' onto any other Windows desktops on which MM is not actually running. Fixed by removing the 'glow window' outline functionality and using a flat frame 1 pixel border instead.

* **Fix: Window Focus when externally activating**  
Fix issue where externally opened documents would not receive focus if MM was already running. Fixed so that focus now always goes into the editor when opening documents from Explorer or other shell executed applications.


### 1.24
<small>September  3rd, 2020</small> 

* **Add additional Code Fencing inline Syntax Colorings**   
Added additional inline syntax colorings for the following languages: `csharp`, `typescript`, `json`, `typescript`, `powershell`. You now get syntax colored text as you type or paste inside of triple backtick fenced code blocks.

* **Additional File Browser Icons and Editing Syntax Support**  
Added additional icons that can display in the Folder Browser including `Dart` and `Dockerfile` files. Both file types can now also be edited in MM. Also fix additional icons for various external document types.

* **Add Menu Shortcuts for Document File and Shell Operations** 
Added shortcuts to quickly navigate the Folder Browser to the active document's location, open the Terminal or Explorer on the Tools menu. Added shortcut mnemonics to make them easier to be  discoverable.

* **Better File Encoding Support**  
You can now set the file encoding for documents. Additional non UTF/Unicode encodings have been added. You can use **Load additional Encodings...** to load up all encodings available. Choosing an encoding will try to reload the document with the new encoding .

* **Improved Code Snippet Syntax for Html to Markdown Conversions**  
Worked with [ReverseMarkdown](https://github.com/mysticmind/reversemarkdown-net) to add better support for capturing syntax for many common services including GitHub, highlightJs, Atlassian and Confluence. For most snippets the syntax should now be applied to code-fenced blocks.

* **Refactor Preview Context Menu UI**  
Change the order of context menu items to show the most context sensitive items on top. Options like *Copy/Edit Image* and *Copy Id to Clipboard* now show on top of the menu if relevant.

* **Copy Image to Clipboard from Preview**  
There is now a context menu options to copy an image to the clipboard from the Preview Browser.

* **Updated Heading ID to Clipboard**  
You can now use the context menu in the previewer or the editor to copy the generated document html `id` attribute value to the clipboard. This makes it easier to paste relative links. As a reminder you can also get this Id from the Document Outline sidebar's context menu.

* **Open Image in Image Editor or in Image Link Form from Editor**  
You can now right click on an image link in the editor and use the option to open then image in your preferred image editor, or open the image in the link dialog (from which you can copy the image to the clipboard or resize the image).

* **Improved Application Title Bar Configuration Options**  
The title bar now has a new `TitlebarDisplayMode` configuration property that has options for displaying, just the filename, the full path, or the filename plus the parent path on the title bar. Tabs continue to display the filename by default and the filename plus parent path *if multiple files with the same name are open*. The new option to display filename plus parent path makes it easier to differentiate documents in the task bar.

* **Paste Improvements**  
Updated the editor paste behavior to use native editor paste behavior for text while deferring images and file paste operations to the Editor shell. This improves paste performance and reliability of text copy and paste operations.

* **Statusbar Text Tooltip**  
The status bar now also shows a multi-line tooltip for extra long messages that might overflow the status bar area. 

* **Switch to MahApps 2.1**  
Switched MM to use latest MahApps Metro UI framework. This provides a number of enhancements plus better stability and consistency for many UI operations.

* **Light Theme Enhancements**  
In light of the MahApps update the light theme has seen a number of updates for better consistency and color contrast in a few areas.

* **Fix: Preview Window Disappearing when editing non-Markdown Files**  
Fix bug that caused the active document syntax type to be turned to an invalid syntax when opening a non-Markdown document. Refactored `EditorSyntax` onto the document itself rather than on the Editor which would cause assignment issues.

* **Fix: SnagIt Screen Capture and File Saving**  
As of SnagIt 2020 the SnagIt Addin from TechSmith was broken due to a bug in the file saving mechanism. Modified the way file saving works by capturing to clipboard and saving the clipboard content. This works around the SnagIt bug and should hopefully future proof the SnagIt regression that keeps cropping up. As a bonus we now get better file location support using MM's document/project file location settings. 

* **Fix: Screen Capture Timer for Built-in Capture**  
Fix the screen capture timer used with with built-in capture when delaying captures.

* **Fix: Add hot key for Add-ins Menu on the Toolbar**  
Added a keyboard mnemonic to the **Tools -> Add-_ins** menu item so it becomes possible to navigate to the add-in list via keyboard. Some addins have been updated with keyboard shortcuts so for example the Azure Blob Addin is accessible via `alt-t-i-b`

* **Fix: Images from Clipboard not showing in Image Previews**  
Fixed issue with WPF clipboard that failed to retrieve images from the clipboard properly. Used alternative image retrieval. This fixes missing preview images in the the Paste Image and Azure Blob Storage Addin (update required to see the fix).

* **Fix Recent Documents Dropdrown Layout**  
Cleaned up the layout of recent menu lists on the Recent Files menu, and on the startup page.

* **Fix: File Encoding and Save Bug**  
Fixed issue where file saving under certain circumstances with non-UTF8 encodings would cause the file not save. Fixed by assigning default encoding and fixing up encoding lookup failures as well as adding additional logging around encoding for notification or save problems.

* **Fix Jekyll Image Publishing**  
Fix bug with images published to a Jekyll Blog. All but the last image were deleted by the post handler previously. Related: Also updated the Preview URL generator used to navigate to the generated URL which should work much better than before for the post title as well as the categories to match the Jekyll snake case conversions.

* **Fix: Light Theme Crashes**  
Fix several bugs related to light theme and missing resources. 


### 1.23 
*<small>June 9th</small>*

* **[Local Jekyll Weblog Publishing Support](https://markdownmonster.west-wind.com/docs/_5rv00rx4i.htm#setting-up-the-jekyll-publishing-configuration) (Preview)**  
Added support for 'publishing; blog posts to a local Jekyll installation. Works by letting you write your blog content as a MM Weblog post and publishing the content into the Jekyll `_posts` folder structure and creates images in the `_assets` folder by post name. Simplifies: Post creation, asset management, re-editing and re-publishing to other blog platforms, makes posts more portable.

* **[Support for Opening Empty/Untitled Documents with Preset Text](https://markdownmonster.west-wind.com/docs/_4x313dneu.htm#open-a-new-document-with-pre-filled-text)**  
You can now open a new untitled document with preset text by using a custom filename format on the command line. Use `mm untitled.base64,base64text`, `mm "untitled.text,This is a new document"`, `mm untitled.urlencoded,this+is+new` to open a document with the specified encoding format. Base64 is recommended due to the need to encode line breaks and extended characters but for simple string text and urlencoded can also work.

* **[New `mm -base64text` Command Line Option](https://markdownmonster.west-wind.com/docs/_5fp0xp68p.htm#base64text)**  
This is an alternate syntax for the `mm untitled.base64,base64Text` option, and provided mainly to provide a clear and obvious documentation point that might be easier to remember and look up. Allows opening a new document with preset text. If launching from the command line or using `CreateProcess` from another application this is the recommended approach for passing new document data to MM.

* **[Open Markdown Monster from a browser with `markdownmonster:` Application Protocol](https://markdownmonster.west-wind.com/docs/_5rj1cknrj.htm)**  
Markdown Monster now installs a `markdownmonster:` Application Protocol Handler which allows opening MM from a within a browser. . You can use `markdownmonster:untitled.text,New Document Text` as well as the other new options using the `mm untitled.` syntax mentioned above.

* **[Built-in local Web Server to allow Browsers Open Text Markdown Monster](https://markdownmonster.west-wind.com/docs/_5s1009yx1.htm)**  
Added WebSocket support to allow opening Markdown text in MM via a browser connection. Socket server listens to incoming document requests and if sent opens a specific document. This is similar to the `markdownmonster:untitled` functionality recently added, but unlike Application Protocols which are limited to 2046 bytes of data, this mechanism allows for large Markdown content to be opened in MM. The WebSocket Server  is disabled by default and can be auto-started whenever MM starts via the `WebSocket.AutoStart` configuration switch.

* **Document Syntax Improvements**  
The Document object now internally tracks the editor syntax associated with it. It is assigned based on the filename extension and mapped to editor associations - just as before. But the Syntax is now separately tracked from the doc type, so that you can change the syntax and affect editor and preview behavior. It's now possible to use the Preview with with `.txt` files for example, if the syntax is set to `markdown`.

* **Improve Configuration Backups to Folder**  
Updated folder backups to choose the Configuration folder `.\backups` sub-folder for folder backups. You can now pick a path and the backup is created as a subfolder **below** that folder in the format of `yyyy-MM-dd-Markdown-Monster-Backup`.

* **Text Only Linking (Ctrl-Shift-K) Improvements**  
When using the text only link shortcut Markdown Monster now automatically pastes and selects URLs from the clipboard. If there's a URL on the clipboard (any https link) it will be automatically injected.



* **Switch to embedded Debug Symbols**  
Debug information is now embedded in the Exe. Removes the original pdb file and reduces distribution footprint by a 1.8mb.

* **Fix: Several Table Parsing Issues**  
Fixed several recurring issues with invalid table imports from unbalanced or mis-formatted tables. Unbalanced tables (with rows that have more columns than headers) are now adding additional headers as needed to balance the table. Added a number of additional out of bounds checks when parsing incoming column data.

* **Fix: Jekyll Pathing issues**  
Fixed a path issue Jekyll publishing if path was entered with trailing slash.

* **Fix: Recursive Loading Issue with Shell Mapped Files**  
Fix issue where a shell mapped file would cause infinite load loops when opened from the shell or the command line. Fixed.

* **Fix: Link Dialog Spaces to %20**  
Automatically fix up any spaces in a typed in url to `%20`. We're not URL encoding the entire URL because more than likely a URL pasted into the textbox (or auto-injected) is already URL encoded so we don't want to end up double encoding, but spaces are one of the most common 'encoded' values that will break Markdown rendering of a URL.

* **Fix: Fonts in Preview Themes**  
We had recently switched to Emojii versions of common fonts as the primary fonts (like Segoi Emojii) but it turns out these fonts are inferior in quality to the regular fonts (like Segoe UI). This should bring font rendering back to smoother text in the preview and also for rendered HTML output in external browsers and exports.

### 1.22
*<small>April 22nd, 2020</small>* 

* **[New Markdown Monster CLI](https://markdownmonster.west-wind.com/docs/_5fp0xp68p.htm#cli-commands)**  
Added new `mmCLI.exe` executable that automates a number of Markdown tasks. You can convert Markdown to HTML in several ways, import HTML into Markdown (with some limitations), convert markdown to PDF as well as perform a number miscellaneous operations. This replaces some of the original Markdown Monster command line switches, but the old executable retains some of the original switches that pertain to administrative tasks like registration, launching etc.

* **[New MarkdownToHtml, MarkdownToPdf and HtmlToMarkdown Command Line Commands](https://markdownmonster.west-wind.com/docs/_5fp0xp68p.htm#cli-commands)**  
As part of the new separated command line tool the command line now supports converting documents between Markdown and HTML and PDF as well as converting documents from HTML to Markdown. Uses `mmcli.exe`.

* **[Additional Site Relative Root Path `/` Overrides](https://markdownmonster.west-wind.com/docs/_5fz0ozkln.htm)**  
Added additional site overrides for root `/` path resolution in the previewer. The new additions look up the folder hierarchy for a `.markdownmonster`, `_toc.json` or `docfx.json` file, or any `<yourProject>.mdproj` file. These overrides happen after the existing checks for YAML `previewWebRootPath` header, and the `PreviewWebRootPath` in an open project file.

* **Close Tabs To Right**  
Added option to the Tab Context and Window Menu to close tabs to the right of the currently active tab.

* **Folder Browser Drag and Drop Improvements**  
You can now drag and drop files from the folder browser into external applications using the standard Windows file dragging protocol. You can also drag open document tabs as files into other applications using the same mechanims. Files dragged from the folder browser into the editor surface either open the document, or embed an image as relative links or ask to save for external paths.

* **Remember PDF Export Settings**  
The PDF export window now remembers its settings so the next time the window is opened the existing settings are preserved.

* **Additional Paper Size Options for PDF Output**  
Added additional paper sizes for PDF output: Letter, Legal, A4, B3.

* **Remember common settings in Link Dialog**  
The link dialog now remembers the last folder used to select a file, for the 'Open in New Window' and 'Use Link References' and checkboxes. These values are stored in the configuration and saved across shutdowns.

* **Download and Embed Web Image Link as Local File**  
New context menu option for image links in the editor allows to convert a Web image URL into a local image by downloading and saving the file to the local disk and re-linking the new relative (if possible) path.

* **Text only Link and Image Embedding**  
Added additional hotkeys to add links (`ctrl-shift-k`) and images (`alt-shift-i`) using text only insertion. Select text and the link/image is wrapped around the text with the cursor inside of the link brackets (similar to the way Github does this).

* **Change h1..h6 Header Insertion Behavior**  
Header insertions for `h1`..`h6` now prefix the current string with the header prefix instead of just prefixing the current selection. It also strips off existing headers so you can change a header value to a new header type.

* **Don't allow usage of non-fixed Width Fonts**  
Changed behavior of Editor Font assignment so that only fixed-width fonts are allowed. The editor used in MM doesn't support propportional fonts, so a fixed-width font has to be used in order for the editor to track correctly. Also updated the description for the Options setting window.

* **Show Linefeed Mode in Statusbar**  
The statusbar now shows the active Linefeed mode. You can click on the mode and jump to setting the global setting to edit in the visual Settings Editor.

* **Statusbar re-arrangements**  
Move around some of the toolbar status items by grouping like items together and making common things a more visible. Also added additional tooltips to those items that didn't have them.

* **Fix up Copy to Clipboard with CR/LF**  
Fix up Ctrl-C copy behavior for text to force CR/LF to the clipboard  even if the editor is running LF only mode. This fixes issues for editors that don't support LF only and paste single lines of text.

* **Color Emoji Fonts in Preview Themes**  
Added specific emoji fonts for the preview themes so emojis now show in color instead of the default black and white.

* **Initial Support for RTL Text (experimental)**  
There is initial support for paragraph based RTL editing by setting the **Enable Right To Left** Editor Setting, and using `Alt-Shift-R` and `Alt-Shift-L` to toggle between RTL and LTR modes respectively. This is still rough and under consideration but if you want to play with this please check out [this issue on Github](https://github.com/RickStrahl/MarkdownMonster/issues/646).

* **Update Registration File Storage for better Shared Drive Operation**  
Modify storage location of `Registered.key` to be stored in the install folder if permissions allow with a backup in the common folder. Install folder storage gives each machine accessing a potentially shared configuration its own machine specific registration file which fixes an issue where shared configurations using DropBox, OneDrive etc. would override the machine specific registration files, resulting in installations as showing not registered.

* **[Improved DocFx Handling in Default Markdown Parser](https://markdownmonster.west-wind.com/docs/_5750qtgr2.htm)**  
Enhanced support for `[!include]` and `[!lang-javascript]` syntax to support the recently added PreviewWebRootPath sniffing, so paths to `/` and `~/` can be resolved. Also improved display of Info/Warning/Note etc.

* **Preview: Added dedicated DocFx Markdown Parser**  
Added the official DocFx parser as a markdown parser you can use to render documents. You can now pick that parser (which also uses Markdig as MM) to render DocFx documents. Rendering features for DocFx features are still limited and there are also problems with this parser. Adding for preliminary experimentation with DocFx content. Feedback welcome.

* **Markdown Rendering Error Display**  
Markdown rendering errors will now display an error page, rather than displaying either a blank page or not clearing the previous page. The error page shows the active Markdown Parser and a detailed code trace for potential debugging.

* **Improve Windows Placement on Application Startup**  
MM now will check if MM is rendering off screen when starting up. It's possible to get MM to start in 'negative' space if you size down from a large 4k display to a 1080p display for example, and MM will move the window into the viewable ViewPort area if it's hidden or even partially offscreen. 

* **Snippets Addin Window Placement**  
The Snippets addin now by default remembers its last window position and ensures that - like the main form - it's visible on the desktop. Initial startup will launch centered in the main app window.

* **Addins: Support for Colors in the MM Console**  
The new Markdown Monster Console that was recently added for addin developers, now implements coloring of console output. The parameters were there previously but didn't do anything.

* **Fix: Save Dialog on Shutdown on wrong Screen**  
Fix issue where the Save dialog with changed content will pop up on the wrong monitor during shutdown by forcing the owner before the main window gets released.

* **Fix: Open from URL**  
Fixed timing issue with Open From URL that caused open operation to not show a document. Fixed with new `EditorDocument.TabLoadingCompleted` event also available to addins to manipulate the document after startup.

* **Fix: UseMathematics and AllowMarkdownScriptTags**  
Add additional notes to the Configuration for `UseMathematics` and `MermaidDiagrams`  that point out that script execution has to be enabled in order to work.

* **Fix: Empty Context Menu on Various Window Controls**  
Fixed bug related to an empty context menu popping up on right clicking in various areas of the main form.


### 1.21
*<small>January 27th, 2020</small>* 

* **Major Overhaul of Editor/Preview Sync in Two-Way Sync Mode**  
Remove a number of issues that caused editor jankiness due to recursive editor and preview syncing. The preview is now more conservative in scrolling the editor so that any two-way recursion issues have been minimized. This fixes jumpiness at the top and bottom of the document (especially in code snippets) as well as unexpected cursor movements during keyboard scrolling. Made a few additional tweaks to the document syncing and scroll functionality which improves editor performance for plain editing and provides more reliable preview sync updates for code blocks and other large block objects in the markdown text.

* **Scroll, Click and Keyboard Navigation Performance Improvements**  
All navigation operations no longer update the preview but only scroll the preview and highlight the relevant new location in the preview which improves navigation performance especially in very large documents. It's now to edit multi-megabyte documents with the preview enabled (although the actual refreshes will still be slow).

* **Large Document Editing Performance Improvements**  
Large documents (2500 lines or more) now throttle the amount of preview refreshes that occur since that can significantly affect the editor performance blocking the editor while the preview refreshes. Timeout is automatically bumped so you can continue to edit at full speed, until stopping for several seconds (instead of the 120ms default timeout on 'normal' sized documents).

* **[Navigation Only Preview](https://markdownmonster.west-wind.com/docs/_5ch1bv9en.htm)**  
Added a new preview **NavigationOnly** preview sync mode, which does not implicitly update the preview on changes, but still allows navigation of the preview as you move through the editor. In this mode, Refresh operations are explict using a new explicit **Refresh Preview (Ctrl-Shift-R)** option on the **Edit** menu. This allows you to edit your very large documents without preview refreshes slowing down editing, and only explicitly refreshing the document to see your changes, all while still being able to navigate the document and scroll the preview.

* **New Console Output Window for Addins to display Output**   
For addin developers there's now a new `Window.ConsolePanel` that is accessible through the Addin's `Model.Console` property. You can `WriteLine()` and `Write()` to the panel, and use `Clear()`, `Hide()` or `Show()` to display status messages from processing. Writing to the Console makes it visible. This can be useful for addins that want to do things like provide progress info or for provide messages for linting etc. 

* **Add *Open Document in new Tab* to Context Menu for Relative Markdown File Links**  
There's a new context menu option that lets you navigate relative Markdown file links in the editor by opening them in a new (or existing) editor tab. Supported file types are opened in an editor tab, everything else is opened the Windows default viewer. HTTP links are opened in the browser.

* **Clickable Links in Editor**  
Links are now clickable in the text editor. Links are underlined and are control click-able,  which displays the link in the appropriate editor. Hyperlinks are opened in the browser, supported documents are opened in the editor and any others are opened in the appropriate Windows associated shell editors.

* **[PreviewWebRootPath in Project](http://markdownmonster.west-wind.com/docs/_5fz0ozkln.htm)**  
Added a new project property which provides a WebRoot Path for translating site relative URLs that start with `/` when the preview HTML is rendered. This allows finding related resources from some site generators or simply when working on deeply nested projects where the rootpath may be buried many levels below the current document. This change is in addition to the `previewWebRootPath` YAML header that already existed, and now is complimented by a `PreviewWebRootPath` project setting that applies to any file rendered while a project is open.

* **Switch to 64-bit version of wkhtmltopdf**  
Switched our PDF generation helper to use the 64 bit version of wkhtmltopdf in order to support conversion of larger documents. Previously PDF conversions of very large documents would fail due to out of memory errors - this update should help with that.

* **[Updated Editor Key Binding Configuration](http://markdownmonster.west-wind.com/docs/_59l0izpoe.htm#editor-commands)**  
Refactored the command binding logic to make it easier to create custom bindings to currently unmapped editor operations. Also allow for key mappings to custom created template expansions through simple key mappings. (note: this may break some existing key bindings. To reset: delete `MarkdownMonster-KeyBindings.json` in config folder)

* **Add KeyBindings Button to Configuration Form**  
Added a button that opens the `MarkdownMonster-KeyBindings.json` file in the editor for editing. Note: Changes to this file require a restart in order to be applied.


* **Add Toolbar DropDown for Additional Editor Operations**  
The main toolbar now has a new dropdown at the end of the editor operations, that provide additional editor insertion operations that are less common but were not discoverable before. Added operations: `<small>`, `<mark>` and `<u>` (underscore) as well as inserting a Page Break (for PDF generation or printing).

* **Addins: Added `Westwind.Scripting` Component to the main Markdown Monster Project**   
The scripting library used in the Snippets and Commander addins has been moved into the main Markdown Monster property, which now makes it available to the main project and all addin projects. This was done to consolidate the 'dynamic scripting' code used in both **Snippets** and **Commander** Addins, which now use the same shared scripting library for dynamic code execution using the same exact Roslyn C# compiler and runtimes.

* **Fix: HTML Rendering and Preview Sync**  
Fix preview HTML editor wonkiness (#609). Refactor HTML document sync separately from the Markdown document sync for greatly improved HTML preview sync to the editor position when editing or scrolling HTML content.


* **Fix: Two-way Code Editor Preview Sync Jumpiness**  
Resolved issue in two-way sync preview mode that was causing the editor to jump when editing or pasting into large blocks of text or code at the top or bottom of the editor. Finally found a solution to separating actual scroll events from explicitly navigated scroll events and refresh operations.

* **Fix: Ctrl-+ Zooming Size Issues**  
Fixed issue where Ctrl-+ zooming would use native browser zooming while Ctrl+- would use the application zooming resulting in missized zoom control and huge browser controls on repeated zooming. Fixed.

* **Fix: Document Outline not closing when closing last document**  
Fix Document Outline issue when closing the last document where the Outline stayed active after the document was closed.

* **Fix: Code Badge Copy Code Linefeeds in Previewer**  
Previously the Code Badge copying would not properly handle line feeds in code snippets. It worked fine for external preview, but for the IE preview the line breaks were lost. Also remove code badges in Print CSS display mode so when printing to PDF the badges don't render. Removed for PDF and Print output.

* **Fix: Code Badge Horizontal Scolling**  
Fix issue with the code badge positioning when the code block is scrolled. Previously the code badge failed to stay pinned to the right in the scrolled content. This fix keeps it always pinned to the right of the code block.

* **Fix: Remove Depedendent ShimGen Exes from Chocolatey Distribution**   
Removed extra EXE files from the distribution for the Roslyn compiler and set up Chocolatey to not generate ShimGen files for the remaining non-MM exe files (pingo, wkhtmltopdf, vbcscompiler).

* **Fix: Edit->Allow Script Tags in Markdown Preview Updating**  
Fix behavior of the `Editor.MarkdownOptions.AllowScriptTags` flag when switched to immediately turn off script rendering and refresh the preview immediately. Use this flag if your preview is jittery due to rendering of script elements in your page (ie. Gists, Math expressions, Embbedded media scripts). These scripts can cause the preview to jump around alot as elements are dynamically inserted. By temporarily disabling script tags, the editor and preview are smooth while not rendering the executing the embedded script. You can now toggle with `Alt-E-T`

* **Fix: Don't render Math Script Expansion Math Expansion is turned off**  
Fixed issue where math code was still expanded even if `MarkdownOptions.UseMathematics` was still enabled.

* **Fix: Table Header Parsing**  
Fixed issue with table editing when selecting an existing table and importing into the table editor for re-editing or re-formatting. Fixed various edge case scenarios that previously crashed the importer.


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
