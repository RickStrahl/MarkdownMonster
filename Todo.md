# Markdown Monster ToDo List

### Immediate

* WebLog MetaWeblog Discovery with /rsd URL
* Add Front Matter Template to Weblog Posts optionally  
  `AddFrontMatterToNewPost` and `FrontMatterTemplate` config options
* ~~Add cursor position to status bar~~
* ~~Auto-Save files~~
* ~~Paste Images from Clipboard and Save To Disk (forward to WPF)~~
* <s>Integrate non-SnagIt screen capture natively</s>

### Bugs
* Screen Capture Weirdness with High DPI Font Scaling
* Weblog: Special Characters in Blog Publishing. Special attn to Image Posting


### Consideration
* Feature image with `wp_post_thumbnail` (also add support to my blog)
    *  First image that includes `feature` as part of the name is used
* Check out ReverseMarkdown C# source - needs adjustments (lists, spacing)
* Support arbitrary MetaWeblog API attributes in MetaData
* Drag and Drop Web Images into the editor (Not possible due to IE Security?)
* Copy Image from Clipboard into editor and save to disk? (See above)
* Git Commit/Push Addin
* PngOut on Png Images captured with SnagIt or Inserted
* Page Templates (add-in?)
* Multiple configurations for blog posts (use post/blogid subitems?)
* Research **R Markdown**, **AsciiDoc**
* Export/Copy/Weblog Publish to Clipboard with Images embedded

### Notes

* RSD format: (Live Writer Checks for this - so should Markdown Monster)
    * Check URL
    * If not XML check for /rsd link
```xml
<rsd version="1.0">
  <service>
    <engineName>CodePlex</engineName>
    <engineLink>http://www.codeplex.com</engineLink>
    <homePageLink>https://markdownmonstertest.codeplex.com/</homePageLink>
    <apis>
      <api name="MetaWeblog" blogID="markdownmonstertest" preferred="true" apiLink="https://www.codeplex.com/site/metaweblog" />
    </apis>
  </service>
</rsd>
```