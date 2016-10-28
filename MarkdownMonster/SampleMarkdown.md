<img src="https://github.com/RickStrahl/MarkdownMonster/raw/master/Art/MarkdownMonster_Icon_128.png" align="right" style="height: 64px"/>

# Welcome to Markdown Monster
Thanks for checking out Markdown Monster. We hope it'll be to be your go-to tool for editing and viewing Markdown text, and perhaps even for your Weblog publishing needs. 

If you run into any problems or issues, **please** let us know so we can address and fix them right away. You can report issues on GitHub:

* <a href="https://github.com/RickStrahl/MarkdownMonster/issues" target="top">Markdown Monster Bug Reports and Feature Requests</a>

# Markdown Features
This topic is meant to give you a very basic overview of how Markdown works, showing some of the most frequently used operations.

### Bold and Italic
This text **is bold**.  
This text *is italic*.  
This text ~~is struck out~~.

### Header Text
# Header 1
## Header 2
### Header 3
#### Header 4
##### Header 5
###### Header 6


### Line Continuation
By default Markdown **adds paragraphs at double line breaks**. Single line breaks by themselves are simply wrapped together into a single line. If you want to have **soft returns** that break a single line, add **two spaces at the end of the line**.

---

This line has a paragraph break at the end (empty line after).

Theses two lines should display as a single
line because there's no double space at the end.

The following line has a soft break at the end (two spaces at end)  
This line should be following on the very next line.

---

### Links
[Help Builder Web Site](http://helpbuilder.west-wind.com/)

If you need additional image tags like targets or title attributes you can also embed HTML directly:

---

```markdown
Go the Help Builder sitest Wind site: 
<a href="http://west-wind.com/" target="top">Help Builder Site</a>.
```
---

### Images
![Help Builder Web Site](https://helpbuilder.west-wind.com/Images/wwhelp_128.png)


### Block Quotes
Block quotes are callouts that are great for adding notes or warnings into documentation.

> ### @icon-info-circle Headers break on their own
> Note that headers don't need line continuation characters as they are block elements and automatically break. Only text lines require the double spaces for single line breaks.

### Fontawesome Icons
Help Builder includes a custom syntax for FontAwesome icons in its templates. You can embed a `@ icon-` followed by a font-awesome icon name to automatically embed that icon without full HTML syntax.

@icon-gear Configuration

### HTML Markup
You can also embed plain HTML markup into the page if you like. For example, if you want full control over fontawesome icons you can use this:

This text can be **embedded** into Markdown:  

<i class="fa fa-refresh fa-spin fa-2x"></i> &nbsp;**Refresh Page**

### Unordered Lists
* Item 1
* Item 2
* Item 3  

This text is part of the third item. Use two spaces at end of the the list item to break the line.

A double line break, breaks out of the list.

### Ordered Lists
If you want lines to break using soft returns use two
spaces at the end of a line. 

---

1. **Item 1**  
Item 1 is really something
2. **Item 2**  
Item two is really something else

---

If you want to lines to break using soft returns use to spaces at the end of a line. 

Now a nested list:

---
1. First, get these ingredients:

      * carrots
      * celery
      * lentils

 2. Boil some water.

 3. Dump everything in the pot and follow  
    this algorithm:
---


### Inline Code
If you want to embed code in the middle of a paragraph of text to highlight a coding syntax or class/member name you can use inline code syntax:

Structured statements like `for x =1 to 10` loop structures 
can be codified using single back ticks.

### Code Blocks with Syntax Highlighting
Markdown supports code blocks syntax in a variety of ways:

The following code demonstrates:

---
Some rendered text

    // This is code by way of four leading spaces
    // or a leading tab

More text here

---

### Fenced Code Blocks
You can also use triple back ticks plus an optional coding language to support for syntax highlighting.

The following is C#:

```csharp
// this code will be syntax highlighted
for(var i=0; i++; i < 10)
{
    Console.WriteLine(i);
}
```    

Many languages are supported: html, xml, javascript, css, csharp, foxpro, vbnet, sql, python, ruby, php and many more. Use the Code drop down list to get a list of available languages.

You can also leave out the language to get no syntax coloring but the code box:

```
robocopy c:\temp\test d:\temp\test
```

### Footnotes
Footnotes can be embedded like this:

Here is some text that includes a Footnote [^1] in the middle of its text. And here's another footnote [^2]. The actual footnotes render on the very bottom of the page.

[^1]: Source: [Markdown Monster Web Site](http://markdownmonster.west-wind.com)
[^2]: Source: [Markdown Monster Web Site](http://markdownmonster.west-wind.com)

### Pipe Tables
Tables look like this:

|size | material     | color       |
|---- | ------------ | ------------|
|9    | leather      | brown  |
|10   | hemp canvas  | natural |
|11   | glass        | transparent |



