# Markdown Monster ToDo List

### Bugs


### Immediate

* [ ] Redesign the Addin Manager Load Behavior
        * Addin Enabled/Disabled
        * Check for Updates
        * Check for incompatible versions
        * Auto-Update?
* [ ] Large, Large Document Performance  

### Mid Term
* [ ] Search for Blog Posts in Weblog Posts folder (walk tree and show titles)
* [ ] Add JavaScript {{Handlebars}} Processing to Snippets Addin


### Bugs


### Consideration
* [ ] Markua (Publishing) support
* [ ] Check out ReverseMarkdown C# source - needs adjustments (lists, spacing)
* [ ] reveal.js presentations
* [ ] Research **R Markdown**, **AsciiDoc**
* [ ] Switch to using CEFSharp (Chromium) instead of IE WebBrowser Control 
    * Turns out this is too slow for the editor at least
    * Lots of overhead even with optimizations enabled (noticable typing lag)
    * Re-examime - CefSharp is supposedly working on perf improvements
    * Perhaps look into using the WindowsFormsHost to use the WinForms version
      which doesn't use bitmap proxy rendering.
    * Very different and more limited Interop between WPF and Javascript
    * Adds significantly to size of package (~40megs)
        
### Markus's List of issues

#### Enhancements
* [ ] Commit/push all changes in the folder to Git
* [ ] Undocking tabs? :smiley:

#### Bugs 

### Table Editor
* [ ] Alignment in tables would be nice
* [ ] Would be nice to be able to re-sort tables somehow based on a certain column
* [ ] It takes quite a while to open the editor for a larger table
* [ ] And then trying to add another column to that table is even slower
