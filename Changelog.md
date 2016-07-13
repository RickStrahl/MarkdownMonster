# Markdown Monster Change Log

### 0.39

* **Add Strikeout to the toolbar**  
Allow setting text to strike out mode where a line is going through text, which is frequently used with ToDo lists to indicate completed or no longer relevant content.

* **Minor Editor Theme tweaking**  
Minor tweaks to add additional hints like bold and italic to the markdown syntax highlighting for headings, bold and italic text.

* **Bug Fixes**  
Fix various file related download errors for Weblog Posts. Fix menu rendering bug. 

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
