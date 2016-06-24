# Markdown Monster Change Log

### 0.33

* **Update preview display management**  
Updated the preview display so it's less jumpy by using a less aggressive refresh in the browser and refreshing content instead of the entire document. This leaves the scroll position intact and doesn't force reload of images which can be slow. Preview display should be much smoother than previously, have no more 'working' cursors.

* **Tooltip for Filename on Tab**
Thanks to an update in [Dragablz](https://github.com/ButchersBoy/Dragablz), tooltips showing the file name in the tab headers are now working again.

* **Download Blog Posts (Preview)**  
The Weblog Add-in can now download blog posts for editing. If the server supports an `mt_markdown` CustomField that value is used. This value is also posted to the server with posts. Otherwise MM will try to convert the post HTML to markdown for editing.

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
