﻿# Markdown Monster ToDo List

### Immediate

* Add cursor position to status bar
* WebLog MetaWeblog Discovery with /rsd URL
* ~~Paste Images from Clipboard and Save To Disk (forward to WPF)~~
* <s>Integrate non-SnagIt screen capture natively</s>

### Bugs
* Weblog: Special Characters in Blog Publishing. Special attn to Image Posting


### Consideration
* Check out ReverseMarkdown C# source - needs adjustments (lists, spacing)
* Support arbitrary MetaWeblog API attributes in MetaData
* Drag and Drop Web Images into the editor (Not possible due to IE Security?)
* Copy Image from Clipboard into editor and save to disk? (See above)
* Git Commit/Push Addin
* PngOut on Png Images captured with SnagIt or Inserted
* Page Templates (add-in?)
* Multiple configurations for blog posts (use post/blogid subitems?)

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