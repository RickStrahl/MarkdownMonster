<img src="https://github.com/RickStrahl/MarkdownMonster/raw/master/Art/MarkdownMonster_Icon_128.png" align="right"/>


# Markdown Monster Change Log

### 1.1.22
<i><small>not released yet</small></i>

* **Weblog Post Image Uploads Replace Folder Name Spaces with -**  
When uploading images to Weblogs the parent folder name now replaces spaces with dashes. We're finding that featured image links by Facebook and Twitter will often not find images where spaces are found in the path. Replacing spaces with dashes provides more accepted URLs.

* **ActiveDocument.Title Property**  
For addin developers there's now a ActiveDocument.Title property that pulls the title from a # header, a Front Matter Title header, or the de-camel cased filename.

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
Switched MM to run as a 32 bit application for interop stability and better performance. 32 bit is faster and more reliable in the COM Automation tasks used to interact with the WebBrowser controls used in MM. Editor performance should be slightly improved and preview syncing should be considerably more reliable.
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
We're happy to announce that Markdown Monster has gone to RTM. There have been no changes only a few minor bug fixes since 0.58.

### Version 0.58
<i><small>November 1st, 2016</small></i>

* **Updated built-in Screen Capture Utility**  
Made  a number of changes to the built-in screen capture utility which makes it easier to capture Windows and areas of the screen. You can now also capture the active desktop and paste images from the clipboard into the capture utility.

* **Searchbox Improvements**  
Updated search box styling to dark theme. Added menu buttons and shortcut notice to activate Find and Replace Functionality.

### Version 0.57
<i><small>October 26th, 2016</small></i>

* **Ctrl-Shift to force Preview Browser Refresh**  
By default MM updates the preview browser to refresh after about a second and a half of keyboard idle time. If you need quicker refresh you can now press `ctrl-shift` to force the browser to refresh immediately.

* **Update Github Theme**  
Updated Github theme to more closely match the new Github formatting. Github is now also the default theme as that's what many of you will be using it for (Github Readme files).

* **Open From HTML Update**  
Modified the opening behavior of HTML documents when opened as markdown. Documents now render immediately and show up as **untitled**, rather than with the same name as the `.html` file with a `.md` extension.

* **UI Cleanup**  
Cleaned up a number of small UI issues with invalid captions, toolbars, mispelled labels etc. Also added a number of editor shortcut keys to the menus so they are more discoverable.

* **Fix Image Relocation Save Errors**  
Fixed bug where saving an image into the document folder was not properly fixing up the relative path.

* **Rendering Bug Fixes**  
Fix rendering and preview issues with the new file location in the temp folder. Errors are more gracefully handled.

### Version 0.55
<i><small>October 17th, 2016</small></i>

* **Render HTML to Temp Folder**   
Changed all HTML output rendering to render to the Windows User %temp% folder rather than into the same folder as as the Markdown file. Rendered file uses `<base>` tag to point back at the original file folder in order to find relative images and other resources. This also avoids a host of potential file permissions issues where you may not be able to save a preview file.

* **Bug Fix: Handle File Permissions Issues**  
Opening and saving files in folder in which the user doesn't have permissions previously crashed. Read and write permissions errors are now trapped and provide more meaningful error information.

* **Better Save Failure Handling**  
Save failures due to permissions or locked files now direct you to the Save as... file dialog rather than... failing.

### Version 0.54
<i><small>October 15th, 2016</small></i>

* **Show Path for Multiple Files with Same Name**  
When multiple files with the same name are open, those files now show the last segment of the the path in addition to the file filename to allow differentiating the files.

* **Change Updater to use manual Download**  
To avoid security issues and AV triggering we've removed the auto-update feature that's built-in and instead direct updates to the download page for manual download and update install when new versions are available during update check or for explicit version checks.

* **Added MarkdownSample.md Page**  
When Markdown Monster starts now we display a sample page that displays markdown features.

### Version 0.53
<i><small>Oct. 11th, 2016</small></i>

* **New Markdown Parser**  
Switched Markdown parsing to the [MarkDig parser](https://github.com/lunet-io/markdig). This parser supports richer subset of **Github flavored Markdown** including support for ~~strikeout~~ parsing and table rendering support (pipe tables).

* **Open in new Window option from Paste Link Dialog**  
The Paste Link menu option and dialog can now paste links that open in a new window. When using this option the link is created as an HTML link.

### Version 0.51
<i><small>Oct. 3th, 2016</small></i>

* **Code Signed Exe's and Installer**  
The Markdown Monster installer file and main binary EXE are now code-signed to verify original compilation source from West Wind Technologies.

### 0.50
<i><small>Sept. 28th, 2016</small></i>

* **Open from Html**  
You can now open HTML documents and have them converted to Markdown - as best as possible. Please realize that not all HTML will cleanly convert to Markdown and nonconvertible HTML will simply end up as HTML (which is legal Markdown) in your new document.

* **mm from Command Line to open Markdown Monster**  
Added batch file to allow `mm readme.md` type syntax to open files from the command line. Both the `mm` and `markdownmonster` commands are globally on the path to allow access from anywhere on the command line.

* **Allow multiple files to be opened from Command Line**  
You can now open multiple files simultaneously in the UI. Local non-fully-pathed files now also work properly from the command line. 

* **Search Downloaded Post List**  
Add search box to Download Posts form to search the list of posts to select a download from. Since blog APIs don't allow for searching through the API you have to download post titles and abstracts first and then search the list.

* **Encrypt Stored Weblog Passwords**  
Passwords that are stored with a Weblog configuration are now encrypted when saved to disk in the Weblog configuration file. You can also edit the config file and store a plain text password which is converted to an encrypted one when saved.

* **New Weblog Post option on menu**  
In order to drive more attention to the Weblog features, we've added a *New Weblog Post* menu option to the main menu. The option takes you to the New Weblog Post page that lets you specify title and blog to post to (can be changed later).

* **Switch to Inno Installer due to AV Issues**  
We've been using InstallMate for our installer, but due to some weird combination of binaries MarkdownMonster and InstallMate in combination were flagging several AV solutions. Rebuilt install with InnoSetup to get clean AV bill (based on VirusTotal results).

### 0.49
* **Add Tab Context Menu**   
Tab tab context menu now allows for: Close documents, Close all documents, Close all but this document, Open folder, and Open Terminal options.

* **Updated hunspell libraries**  
The hunspell spell checking libraries unfortunately have been triggering virus warnings with a couple of virus scanners. hunspell is widely used and open source, so it is safe but unfortunately triggers the AV. Switched versions for binaries to see if this eases the AV triggers.

### 0.48
* **Add Save As Html Menu Option**  
You can now directly save markdown documents as rendered HTML. This is addition to the original feature that allows selected Markdown text to be copied as HTML to the clipboard.

* **Check for file changes on disk**
You'll now get a notification if the file you are editing has been changed on disk while you are editing the file and before it has been saved. A prompt lets you know and allows you accept or reject the changes from disk.

* **Fix Editor Scaling on High DPI Displays**  
Added custom trapping of browser events for zooming the editor font size via scroll wheel and Ctrl-+ and Ctrl--. These operation now intercept the browser's native behavior and increment the configuration font-size instead providing a smoother and more consistent resizing setup

### 0.45
* **Updated Preview Themes**  
Preview themes have been tweaked for better default sizing on scaled displays.

* **Miscellaneous UI and Performance Fixes**  
Fixed timing issue in the preview editor. Changed spellchecking frequency to reduce overhead for larger documents.

### 0.44
* **Add Document Stats to Statusbar**  
Add Line and Word count to the status bar to give basic document stats.

* **Fix Preview Pane Resize**  
Fix the preview pane resizing routine to ensure that the preview window size is preserved when the preview is updated, also when Monster is shutdown and restarted.

### 0.43
* **Font Resizing via Ctrl-Scroll or Ctrl+/-**  
Font resizing via Ctrl-Scrollwheel or Ctrl-+/- now captures any font size changes and applies them to all open windows. The value now also updates the global default font size.

* **Preview Themes Updates**  
Change preview theme to use slightly larger font-sizes by default in order to allow for better default settings when running on scaled displays. Still looking for better workaround to handle browser control scaling issues.

### 0.42
* **Add non-SnagIt Window Capture**  
If you don't have the excellent SnagIt tool installed you can now do rudimentary screen captures directly into your document using an external form that captures desktop windows/objects. MM will detect if SnagIt is installed and automatically choose the right capture mode.

* **New Button on the Toolbar**  
There's now a new document button on the toolbar menu.

### 0.41
* **Modify Preview File Lifetime**  
The preview file (__YourFile.htm) is now deleted shortly after rendering to avoid the file showing up in GIT checkins.

* **Reduce memory Footprint**  
Added logic to reduce memory footprint after initial load by resetting the working set. Memory usage is high in general due to the WebBrowser controls that are used for rendering the editor and preview browser, but footprint reduces nicely when the application is deactivated.

### 0.40
* **Markdown Post Download now downloads Images**  
When you download Weblog posts that include images the images are downloaded locally into the post folder, so they display properly. Note Markdown can be 'published' and retrieved using the special `mt_markdown` custom variable in MetaWebLog API, which is sent and received.

* **Fix Blog List dialog Sizing**  
The blog download tab in the blog Add-in now properly displays the message list downloaded from the server. Downloads are much faster and entries can be filtered.

### 0.39
* **Add Strikeout to the toolbar**  
Allow setting text to strike out mode where a line is going through text, which is frequently used with ToDo lists to indicate completed or no longer relevant content.

* **Minor Editor Theme tweaking**  
Minor tweaks to add additional hints like bold and italic to the markdown syntax highlighting for headings, bold and italic text.

* **Bug Fixes**  
Fix various file related download errors for Weblog Posts. Fix menu rendering bug. Fix Draft Status flag on Post uploads. 

* **Markdown Basics Link**  
Link to Markdown Basics from Help menu to provide a basic introduction to Markdown in the online help file.

### 0.37

* **Update preview display management**  
Updated the preview display so it's less jumpy by using a less aggressive refresh with a fixed timeout and injecting content instead of refreshing the entire document. This leaves the scroll position intact and doesn't force reload of images which can be slow. Preview display should be much smoother, no jumping and work with annoying wait cursors.

* **Add Markdown Monster to the System Path**   
Markdown Monster is now added to the System Path when installed which makes it easy to launch it from the Command Window or Powershell with `MarkdownMonster` or simple `mm`.

* **Tooltip for Filename on Tab**
Thanks to an update in [Dragablz](https://github.com/ButchersBoy/Dragablz), tooltips showing the file name in the tab headers are now working again.

* **Download Blog Posts (Experimental Preview)**  
The Weblog Add-in can now download blog posts for editing. If the server supports an `mt_markdown` *CustomField* that value is used for editing. If not then the HTML is converted as best as possible into Markdown for editing.

### 0.35
* **Update preview refresh**  
Add additional preview update logic to improve re-rendering of the previewer.

* **Fix: SnagIt Options not Saving**  
Fixed bug that caused SnagIt Addin options to not save reliably and resetting. Save now properly persists the configuration data into the `SnagitConfiguration.json` file in the `%appdata%` folder.

* **Remote Error Logging**  
Fatal errors are now optionally logged to the Markdown Monster Web site. The information logged is the same information written to the local error log. Remote error logging can be disabled via configuration setting but is enabled by default.

### 0.32
* **Update preview refresh**  
Keyboard refresh now waits for keyboard to be idle for 1 second after typing before refreshing. This is a single check which is much more efficient and should result in less jitter and faster keyboard entry.

### 0.31
* **Fixes to MetaWebLog API Blog Publishing**  
Switched to object type and string defaults for Post and Blog ids which is more compatible with server side libraries and compatible with what Live Writer sends. Thanks to Thorsten Weggen for his help in tracking some of these issues down.
 
* **Slightly adjusted Preview Themes**  
Updated the Dharkan and Blackout themes with slightly bigger fonts and wider line spacing.

### 0.30 
* **Added better Application Updater Error Handling**  
If updates can't be download the errors are no longer blowing up MM on exit. Also there's a pre-check now that checks for connection availability.

* **Image Embedding now prompts for local path**  
When embedding a local image from a non-relative path you are now prompted if you want to copy the image to a local folder relative to the document.