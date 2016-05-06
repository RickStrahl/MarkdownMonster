# Markdown Monster
### An extensible Markdown editor, for your daily edit

> **Alpha Notice**:  
> This tool is still under construction and in pre-release stage. Basic editing and preview features work but some of the support features like link/image embedding and code highlights from the menus are not working yet. The add-ins are also experimental only. The tool is usable as is just be extra careful with keeping backups.

![](Art/MarkdownMonster.png)


<a href="http://markdownmonster.west-wind.com/" 
   class="btn btn-lg btn-primary" style="margin: 20px 0; font-size: 16pt;">
<svg style="width:25px;height:25px;" viewBox="0 0 25 25">
    <path fill="#ffffff" d="M5,20H19V18H5M19,9H15V3H9V9H5L12,16L19,9Z" />
</svg>  Download</a>


Markdown is everywhere these days, and having a dedicated Markdown editor that lets you easily create your markdown text with a light touch of help can make editing of content much more enjoyable and productive.

Monster provides you with many features that make your Markdown editing process more productive:

### Features
Markdown Monster provides many useful features:

* Syntax highlighted Markdown editing
* Live preview using local HTML templates
* Gentle toolbar support for Markdown newbies and complex tasks
* Easily customizable preview templates
* Inline, as-you type spell checking
* Customizable editor and preview Themes
* Capture screen images (native or using SnagIt)
* Extensible: .NET Add-in model lets you plug in custom features
* Addins provided: SnagIt Screen Capture and Blog Publisher

Here's what Markdown Monster looks like (Running the Dark Theme):
![Markdown Monster Screen Shot](ScreenShot.png)

Focus is on the editor and the preview is collapsible. The toolbar and a number of common control hotkeys provide gentle UI support for embedding Markdown expressions into your content.

Mostly it's about editing Markdown, and getting content created

### Extensibility
Markdown Monster was built to provide customization. It includes an add-in model that allows adding of new functionality to the tool. Addins can add their own menu choices to the menu and interact with the active and any open Markdown document as well as the user interface. 

We ship one example plug-in which is the screen capture plug-in that uses either a native screen capture utility, or [TechSmith's powerful SnagIt tool](https://www.techsmith.com/snagit.html).


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

**Personal use and use in small businesses with less than 5 employees is free**. All other uses require [a reasonably priced license](http://store.west-wind.com/product/MARKDOWN_MONSTER). Markdown Monster is licensed per user and you are free to use multiple copies on multiple machines as long as only licensed users are running the software.

Thanks for playing fair.