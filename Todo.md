# Markdown Monster ToDo List

### Immediate
* [ ] Addin Enabled/Disabled
* [ ] Master Configuration Form
* [ ] Markdown Document Outline


### Mid Term
* [ ] Add JavaScript {{Handlebars}} Processing to Snippets Addin
* [ ] Image Previews and file preview in a **Preview Tab**
* [ ] Spell checker Dictionary Downloads
    * Store dictionaries online
    * Show what's installed
    * Show whats available
    * Link to Open Office for missing stuff

### Bugs
* [ ] Fix Folder Browser Rename/Delete file occasional locking issues.
* [ ] Fix Markdown List display where new bullet auto creates

### Consideration
* [ ] Check out ReverseMarkdown C# source - needs adjustments (lists, spacing)
* [ ] reveal.js presentations
* [ ] Research **R Markdown**, **AsciiDoc**
* [ ] Switch to using CEFSharp (Chromium) instead of IE WebBrowser Control 
    * Turns out this is too slow for the editor at least
    * Lots of overhead even with optimizations enabled (noticable typing lag)
    * Reexamime - CefSharp is supposedly working on perf improvements
    * Perhaps look into using the WindowsFormsHost to use the WinForms version
      which doesn't use bitmap proxy rendering.
    * Very different and more limited Interop between WPF and Javascript
    * Adds significantly to size of package (~40megs)
        
### Markus's List of issues

#### Enhancements
    

* [ ] Commit/push all changes in the folder to Git
* [ ] FileSystemWatcher to update the tree automatically
* [ ] Undocking tabs? :smiley:
* [x] Allow me to search in the tree
* [x] Ctrl-Tab to switch documents
* [x] Can't the save-as dialog stay open on the same folder rather than always defaulting back to something
* [x] ~~Could you somehow support Grammarly?~~

#### Bugs 

* [x] Drag & Drop of images does not work when one creates a new file and that file hasn't been saved yet
* [x] When adding a new file to the tree, it should show the right icon when setting the file extension
* [x] Can I somehow see what the current zoom level is?
* [x] When right-clicking to delete in the tree, it should delete the item I clicked on, even if a different one is selected (right now, you can accidently delete a different one if an item in the tree is selected, and then you right-click on a DIFFERENT one and pick "Delete")
* [x] Table editing in TextBox Input Masks topic (and it also has trouble with the escaped | character when escaped with a backslash)
* [ ] It's not particularly happy on very large topics (such as the "Understanding Layout" one I have)


* [x] Creating new files with ctrl-N is still kind of a pain and the behavior is quite unpredictable
* [x] CTRL-S saving often seems to be slow. (Does a Git Push include a save automatically?)

### Table Editor
* [ ] Alignment in tables would be nice
* [ ] Would be nice to be able to re-sort tables somehow based on a certain column
* [x] Would be nice to paste entire tables somehow from HTML or Word
* [x] It takes quite a while to open the editor for a larger table
* [x] And then trying to add another column to that table is even slower
