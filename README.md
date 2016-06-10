# Markdown Monster
### An extensible Markdown Editor for Windows

![](Art/MarkdownMonster.png)

### Links
* **[Markdown Monster Site](http://markdownmonster.west-wind.com)**
* **[Download Markdown Monster Installer](http://markdownmonster.west-wind.com/download.aspx)**
* **[Create Addins with .NET](http://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm)**
* **[Markdown Monster Addin Registry](https://github.com/RickStrahl/MarkdownMonsterAddinsRegistry)** (coming soon)  

### Features
Markdown Monster provides many useful features:

* Syntax highlighted Markdown editing
* Live Markdown HTML preview 
* Local easily, customizable HTML templates for preview
* Gentle toolbar support for Markdown newbies and complex tasks
* Easily customizable preview templates
* Inline, as-you type spell checking
* Customizable editor and preview Themes
* Syntax colored code snippet preview support
* Paste HTML as Markdown
* Copy Markdown Section as Html

#### Extensibility
* Extensible: .NET Add-in model lets you extend with custom features
* Two useful plugins are provided:
* Capture screen images ([SnagIt](http://techsmith.com/snagit) only currently)
* Weblog publishing plug-in (MetaWebLog and Wordpress)

#### Non Markdown Features
* HTML file editing with live preview
* Many other file formats can also be edited:  
JSON, XML, CSS, JavaScript, Typescript, FoxPro, CSharp and more

Here's what Markdown Monster looks like:

![Markdown Monster Screen Shot](ScreenShot.png)

> #### Beta Notice
> This tool is still under construction and in pre-release stage. Most of the editing and document management features are implemented, but there are a still a few rough edges.

### Why another Markdown Editor?
Markdown is everywhere these days, and it's becoming a favorite for many developers, writers and documentation experts to create content in this format. Personally I use Markdown for my Blog, my MessageBoard, of course on GitHub and in a number of applications. Having an editor that gets out of your way, yet provides a few helpful features **and lets you add custom features** that make your content creation sessions more productive are important.

Markdown Monster is a Markdown editor for Windows, that provides basic editing functionality. It works, nothing revolutionary here. You get a responsive text editor that's got you covered with Markdown syntax highlighting, so it it's easy to navigate your Markdown text, an optional collapsible live preview, so you can see what your output looks like, in-line spellchecking and a handful of optimized menu options that help you embed and link content into your markdown.

### Customizable
Most features are optional and can be turned on and off. Want to work distraction free and see no preview or spell checking hints? You can turn things off. Want a different editor or preview theme, just switch it to one of the many editor themes and preview themes. 

The editor is HTML and JavaScript based, so you can also apply any custom styling and even hook up custom JavaScript code if you want to get fancy beyond the basic configurability.

### Extensibility with Add-ins
But the key feature and the main reason I built this tool is that it is **extensible**, so that you and I can plug additional functionality into it. Markdown Monster includes an add-in model that lets you add buttons to the UI, interact with the active document and get notifications of various events like when documents are opened and closed etc.

The Add-in interface is still in flux, so no documentation except for the provided sample add-ins.

### Provided Add-ins
Personally I needed couple of features - and I've added two add-ins for these features both for practical use as well as examples of what you can do with plug-ins:

* **SnagIt Screen Capture Addin**  
This plug-in use Techsmith's popular and super versatile [SnagIt](http://techsmith.com/snagit) Screen Capture utility (which i **highly** recommend!). Simply click the capture button (camera icon) and the main app minimizes and SnagIt pops up to let you select the object to capture. You can preview and edit your captures, and when finished the image is linked into content.

![SnagIt Screen Capture Add-in](SnagItCaptureAddin.png)

* **WebLog Addin**  
Writing long blog posts is one thing I do a lot of and this is one of the reasons I actually wanted an integrated solution in a Markdown editor. You can take any Markdown and turn it into a blog post by using the blog tool, setting up your blog (MetaWebLog or WordPress) and the add-in handles publishing your text and attached images to your blog. Currently you can't only write (not read) but managing and loading existing posts is one feature on the list.

![Weblog Publishing Addin](WebLogPublishingAddin.png)

I can think of a few others - a quick way to commit to Git and Push would be useful for documentation solutions so you can easily persist changes to say a GitHub repository. Embedding all sorts of content like reference links, amazon links etc. etc.

Or maybe you have custom applications that use Markdown text and provide an API that allows you to post the Markdown (or HTML) to the server. It's easy to build a custom add-in that lets you take either the Markdown text or rendered HTML and push it to a custom REST interface in your custom application.

## Acknowledgements
This application heavily leans several third party libraries without which this tool would not have been possible. Many thanks for the producers of these libraries:

* **[Ace Editor](https://ace.c9.io)**  
Ace Editor is a power HTML based editor platform that makes it easy to plug syntax highlighted software style editing possible in a browser. Markdown Monster uses Ace Editor for the main Markdown editing experience inside of a Web browser control that interacts with the WPF application.

* **[MahApps.Metro](http://mahapps.com/)**  
This library provides the Metro style window and theming support of the top level application shell.

* **[Dragablz](https://dragablz.net/)**  
This library provides the tab control support for the editor allowing for nicely styled tab reordering and overflow. Although not used in Markdown Monster the library also supports tab tear off tabs and layout docking.

* **[CommonMark.NET](https://github.com/Knagis/CommonMark.NET)**  
This is the markdonwn parser used to render markdown in the preview editor. CommonMark.NET is fast and easy to work with and has an excellent extensibility interface.

## License
Although we provide the source in the open, Markdown Monster is licensed software.

Markdown Monster can  be downloaded and evaluated for free, but a [reasonably priced license](http://store.west-wind.com/product/MARKDOWN_MONSTER) must be purchased for continued use. Licenses are per user, rather than per machine, so you can use Markdown Monster on as many computers you wish with your license. 

Thanks for playing fair.

## Warranty Disclaimer: No Warranty!
IN NO EVENT SHALL THE AUTHOR, OR ANY OTHER PARTY WHO MAY MODIFY AND/OR REDISTRIBUTE 
THIS PROGRAM AND DOCUMENTATION, BE LIABLE FOR ANY COMMERCIAL, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF THE USE OR INABILITY TO USE THE PROGRAM INCLUDING, BUT NOT LIMITED TO, LOSS OF DATA OR DATA BEING RENDERED INACCURATE OR LOSSES SUSTAINED BY YOU OR LOSSES SUSTAINED BY THIRD PARTIES OR A FAILURE OF THE PROGRAM TO OPERATE WITH ANY OTHER PROGRAMS, EVEN IF YOU OR OTHER PARTIES HAVE BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

&copy; Rick Strahl, West Wind Technologies, 2016