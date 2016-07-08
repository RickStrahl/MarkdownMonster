# Markdown Monster ToDo List

### Immediate
* Download WebLog posts (in preview)
* Refresh files if changed on disk
* Add cursor position to status bar

### Bugs
* <s>Untitled shouldn't show up in Recent List</s>
* Fix invalid Filename chars for new and downloaded Blog posts

### Consideration
* Drag and Drop Web Images into the editor
* Copy Image from Clipboard into editor and save to disk?
* Git Commit/Push Addin
* PngOut on Png Images captured with SnagIt or Inserted
* Page Templates (add-in?)
* Multiple configurations for blog posts (use post/blogid subitems?)

### Notes

#### Multiple Blog Configurations per post

Potential format:

```xml
<abstract>

</abstract>
<categories>

</categories>
<keywords>

</keywords>
<weblog>
Rick Strahl's Weblog
</weblog>
<blogs>
    <blog>
        <weblog></weblog>
        <postId></postId>
    </blog>
</blogs>   
```

or 

```xml
<blog>
    <weblog>
    Rick Strahl's Weblog
    </weblog>
    <postId>
    221
    </postId>
    <abstract>
    
    </abstract>
    <categories>
    
    </categories>
    <keywords>
    
    </keywords>
</blog>
<blog>
    <weblog>
    Rick Strahl's Weblog (local)
    </weblog>
    <postId>
    2333
    </postId>
    <abstract>
    
    </abstract>
    <categories>
    
    </categories>
    <keywords>
    
    </keywords>
</blog>
```