# Markdown Monster
### An extensible Markdown Editor for Windows

![](Art/MarkdownMonster.png)

### Features
Markdown Monster provides many useful features:

* Syntax highlighted Markdown editing
* Live Markdown HTML preview using local HTML templates
* Gentle toolbar support for Markdown newbies and complex tasks
* Easily customizable preview templates
* Inline, as-you type spell checking
* Customizable editor and preview Themes
* Syntax colored code snippet support
* Capture screen images (using SnagIt currently)
* Weblog Publishing plug-in (MetaWebLogAPI only at the moment)
* Extensible: .NET Add-in model lets you plug in custom features
* Addins provided: SnagIt Screen Capture and Blog Publisher

There also a number of non-Markdown related features:

* Also provides highlighted editing Html, CSS, Javascript, JSON, C# and FoxPro files
* Live Preview of HTML documents

### Get it from:
* **[Markdown Monster Site (under construction)](http://markdownmonster.west-wind.com)**
* **[Download Markdown Monster Installer](http://markdownmonster.west-wind.com/download.aspx)**

> #### Alpha Notice
> This tool is still under construction and in pre-release stage. Most of the editing and document management features are implemented, but there are a still a few rough edges. The WebLog plugin only works with MetaWebLog API and it's lacking blog administration (you can edit the config file manually for now to set up a blog).*

### Why another Markdown Editor?
Markdown is everywhere these days, and it's becoming a favorite for many developers, writers and documentation experts to create content in this format. Having an editor that gets out of your way, yet provides a few helpful features and lets you add custom features that make your content creation sessions more productive are important.

Markdown Monster is a Markdown editor for Windows, that provides basic editing functionality. It works, nothing revolutionary here. You get a responsive text editor that's got you covered with Markdown syntax highlighting, so it it's easy to navigate your Markdown text, an optional collapsible live preview, so you can see what your output looks like, in-line spellchecking and a handful of optimized menu options that help you embed and link content into your markdown. 

But the key feature and the reason I built this tool is that it is **extensible**, so that you and I can plug additional functionality into it. Personally I needed couple of features - built-in screen captures and an easy way to post text to my Blog - and Markdown Monster makes that possible via an add-in model that allows extending the base functionality with custom functionality via a .NET based add-in model. The SnagIt Screen Capture and WebLog publishing add-ins are included in this base release to demonstrate how the plug-in model works and integrates

Here's what Markdown Monster looks like (Running the Dark Theme):
![Markdown Monster Screen Shot](ScreenShot.png)

### Extensibility
Markdown Monster's core implementations is all about the editor and additional features are meant to be implemented as add-ins to provide additional features. It includes an add-in model that allows adding of new functionality that hooks into the menu system and has access to the editor's content. It's easy to create a plug  in that has a toolbar option that can read the Markdown text or selected text and fix up the current content. Or you can write entire sub-applications like the WebLog add-in for example, that take the Markdown content and do something with it externally.


## Acknowledgements
This application heavily leans several third party libraries without which this tool would not have been possible. Many thanks for the producers of these libraries:

* **[Ace Editor](https://ace.c9.io)**  
Ace Editor is a power HTML based editor platform that makes it easy to plug syntax highlighted software style editing possible in a browser. Markdown Monster uses Ace Editor for the main Markdown editing experience inside of a Web browser control that interacts with the WPF application.

* **[MahApps.Metro](http://mahapps.com/)**  
This library provides the Metro style window and theming support for the overall application shell.

* **[CommonMark.NET](https://github.com/Knagis/CommonMark.NET)**  
This is the markdonwn parser used to render markdown in the preview editor. CommonMark.NET is fast and easy to work with and has an excellent extensibility interface.

## License
Markdown Monster is open source, but it is a licensed product that may require a paid license.

**Personal use and use in small businesses with less than 5 employees is free**. All other uses require [a reasonably priced license](http://store.west-wind.com/product/MARKDOWN_MONSTER). Markdown Monster is licensed per user and you are free to use multiple copies on multiple machines as long as only licensed users are running the software. Thanks for playing fair.