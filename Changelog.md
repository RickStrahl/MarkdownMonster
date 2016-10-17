<img src="https://github.com/RickStrahl/MarkdownMonster/raw/master/Art/MarkdownMonster_Icon_128.png" align="right"/>

# Markdown Monster Change Log

### Version 0.55
<i><small>October 18th, 2016</small></i>

* **Render HTML to Temp Folder**   
Changed all HTML output rendering to render to the Windows User %temp% folder rather than into the same folder as as the Markdown file. Rendered file uses `<base>` tag to point back at the original file folder in order to find relative images and other resources. This also avoids a host of potential file permissions issues where you may not be able to save a preview file.

* **Bug Fix: Handle File Permissions Issues**  
Opening and saving files in folder in which the user doesn't have permissions previously crashed. Read and write permissions errors are now trapped and provide more meaningful error information.

* **Better Save Failure Handling**  
Save failures due to permissions or locked files now direct you to the Save as... file dialog.

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
