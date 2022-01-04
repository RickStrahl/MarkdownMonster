
# Welcome to Markdown Monster
Here are a few tips to get you started:

* To create a **New Document** press **Ctrl-N** or **click @icon-plus-circle** on the toolbar

* To toggle the **HTML Preview** press **F12** or click @icon-globe in the Window bar

* To open the **Folder Browser** press **Ctrl-Shift-B** or click **@icon-bars**

* The **@icon-bars SideBar** also holds **Favorites** and **Markdown Document Outline**

* To change **UI Themes**, click on the dropdown lists on the bottom right status bar:

![Image and Preview Themes on the toolbar](https://markdownmonster.west-wind.com/docs/images/EditorPreviewThemeUi.png) 
  
* For **light editor themes** look at `visualstudio`, `github` or `xcode`  

* For **dark editor themes** look at `vscodedark`, `twilight`, `monokai`, `terminal`

### Problems? Please let us know

If you run into any problems or issues, **please** let us know so we can address and fix them right away. You can report issues on GitHub:

* [Markdown Monster Bug Reports and Feature Requests](https://github.com/RickStrahl/MarkdownMonster/issues)

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

The following line has a soft break at the end (two spaces or a `\` at end)  
This line should be following on the very next line.

You can use **View -> Toggle Invisible Characters** to show all white space and returns.

---

### Links
You can easily link using `[text](link)` sytnax:

[Markdown Monster Web Site](http://MarkdownMonster.west-wind.com/)

If you need additional image tags like targets or title attributes you can also embed HTML directly using raw HTML markup:

```html
Go to the 
<a href="https://markdownmonster.west-wind.com" style="font-style: italic">
    Markdown Monster Web Site
</a>
```

renders:

Go to the
<a href="https://markdownmonster.west-wind.com" style="font-style: italic">
    Markdown Monster Web Site
</a>

---

### Images
Image Markdown Syntax is similar to links:

```markdown
![Markdown Monster](https://markdownmonster.west-wind.com/Images/MarkdownMonster_Icon_128.png)
```

which renders:

![Markdown Monster](https://markdownmonster.west-wind.com/Images/MarkdownMonster_Icon_128.png)

You can embed images by pasting from the Clipboard using the @icon-image Image Dialog (**ctrl-v**) or inline Markdown embedding (**ctrl-shift-v**), or by dragging and dropping into the document from the the Folder Browser,  Windows Explorer or most Web Browsers.

### Block Quotes
Block quotes are callouts that are great for adding notes or warnings into documentation.

````markdown
```markdown
> ### @icon—info-circle Headers break on their own
> Note that headers don't need line continuation characters as they are block elements and automatically break. Only text lines require the double spaces for single line breaks.
```
````

> ### @icon-info-circle Headers break on their own
> Note that headers don't need line continuation characters as they are block elements and automatically break. Only text lines require the double spaces for single line breaks.

You can also use simple block quotes:

```markdown
> **Note:** Block quotes can be used to highlight important ideas.
```

> **Note:** Block quotes can be used to highlight important ideas.

### Fontawesome Icons
Help Builder includes a custom syntax for FontAwesome icons in its templates. You can embed a `@icon—`  followed by a font-awesome icon name to automatically embed that icon without full HTML syntax.


```markdown
@icon—gear Configuration
```

which renders:

@icon-gear Configuration

### Emojis
You can also embed Emojis into your markdown using the Emoji dialog or common **emoji expressions**:

```markdown
:smile: :rage: :sweat: :point_down:

:-) :-( :-/ 
````

:smile: :rage: :sweat: :point_down:

:-) :-( :-/ 

> You can also embed raw emojis by using the `windows-.` key, or by pasting from the clipboard. You can also use an online [emoji reference](https://www.webfx.com/tools/emoji-cheat-sheet/).

### HTML Markup
You can also embed plain HTML markup into the page if you like. For example, if you want full control over fontawesome icons you can use this:

This text can be **embedded** into Markdown:  

```markdown
<i class="fa fa-refresh fa-spin fa-2x"></i> &nbsp;**Refresh Page**
```

<i class="fa fa-refresh fa-spin fa-2x"></i> &nbsp;**Refresh Page**

Note that blocks of raw HTML markup should be separated from text by empty lines above and below the HTML blocks.

### Unordered Lists

```markdown
* Item 1
* Item 2
* Item 3  
```

* Item 1
* Item 2
* Item 3  

This text is part of the third item. Use two spaces at end of the the list item to break the line.

A double line break, breaks out of the list.

### Ordered Lists
If you want lines to break using soft returns use two
spaces at the end of a line. 

```markdown
1. **Item 1**  
Item 1 is really something
2. **Item 2**  
Item two is really something else
```


1. **Item 1**  
Item 1 is really something
2. **Item 2**  
Item two is really something else


If you want to lines to break using soft returns use to spaces at the end of a line. 

> **Note**: Numbered lists order themselves base on order rather than the number you use. All numbers can be the same and the list will order itself.

Now a nested list:

```markdown
1. First, get these ingredients:

      * carrots
      * celery
      * lentils

2. Boil some water.

3. Dump everything in the pot and follow  
this algorithm:
```


1. First, get these ingredients:

      * carrots
      * celery
      * lentils

 2. Boil some water.

 3. Dump everything in the pot and follow  
    this algorithm:


### Inline Code
If you want to embed code in the middle of a paragraph of text to highlight a coding syntax or class/member name you can use inline code syntax:

```markdown
Inline code or member references  like `SomeMethod()` can be codified...
```
---

Inline code or member references  like `SomeMethod()` can be codified... You can use the `'{}'`** menu or **Ctrl-\`** to embed inline code.

---

### Indented Code Blocks 
Markdown supports code blocks syntax in a couple of ways:

Using and indented text block for code:


````markdown
Some rendered text...

    // This is code by way of four leading spaces
    // or a leading tab
    int x = 0;
    string text = null;
    for(int i; i < 10; i++;) {
        text += text + "Line " + i;
    }

More text here
````

renders:

Some rendered text...

    // This is code by way of four leading spaces
    // or a leading tab
    int x = 0;
    string text = null;
    for(int i; i < 10; i++;) {
        text += text + "Line " + i;
    }

More text here


### Fenced Code Blocks with Syntax Highlighting
You can also use triple back ticks plus an optional coding language to support for syntax highlighting.

The following is C# code.

````markdown
```csharp
// this code will be syntax highlighted
for(var i=0; i++; i < 10)
{
    Console.WriteLine(i);
}
```  
````

which renders syntax colored code:

```csharp
// this code will be syntax highlighted
for(var i=0; i++; i < 10)
{
    Console.WriteLine(i);
}
```    

Many languages are supported: html, xml, javascript, typescript, css, csharp, fsharp foxpro, vbnet, sql, python, ruby, php, powershell, dos, markdown, yaml and many more. Use the Code drop down list to get a list of available languages.

You can also leave out the language to attempt auto-detection or use `text` for plain text:

````markdown
```text
robocopy c:\temp\test d:\temp\test
```
````

renders plain, but formatted text:

```text
robocopy c:\temp\test d:\temp\test
```

> **Note**: Prefer using `text` for non-highlighted syntax over no syntax as no syntax tries to auto-discover the syntax which often is not correct. Always be specific with syntax specified.

### Footnotes
Footnotes can be embedded like this:

Here is some text that includes a Footnote [^1] in the middle of its text. And here's another footnote [^2]. The actual footnotes render on the very bottom of the page.

[^1]: Source: [Markdown Monster Web Site](http://markdownmonster.west-wind.com)
[^2]: Source: [Markdown Monster Web Site](http://markdownmonster.west-wind.com)

### Pipe Tables
[Pipe Tables](https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/PipeTableSpecs.md) can be used to create simple single line tables:


```markdown
|size | material     | color       |
|---- | ------------ | ------------|
|9    | leather      | brown **fox**  |
|10   | hemp canvas  | natural |
|11   | glass        | transparent |
```

---

|size | material     | color       |
|---- | ------------ | ------------|
|9    | leather      | brown **fox**  |
|10   | hemp canvas  | natural |
|11   | glass        | transparent |

> **Note:** Cell lines don't have to line up to render properly. Max columns in any row determines table columns for the entire table. Pipe tables also don't need leading and trailing pipes to render as tables, but make sure you check compatibility with your final rendering site.

### Grid Tables
[Grid Tables](https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/GridTableSpecs.md) are a bit more flexible than Pipe Tables in that they can have multiple lines of text per cell and handle multi-line embedded Markdown text.

```markdown
+---------+---------+
| Header  | Header  |
| Column1 | Column2 |
+=========+=========+
| 1. ab   | > This is a quote
| 2. cde  | > For the second column 
| 3. f    |
+---------+---------+
| Second row spanning
| on two columns
+---------+---------+
| Back    |         |
| to      |         |
| one     |         |
| column  |         | 
```

--- 
+---------+---------+
| Header  | Header  |
| Column1 | Column2 |
+=========+=========+
| 1. ab   | > This is a quote
| 2. cde  | > For the second column 
| 3. f    |
+---------+---------+
| Second row spanning
| on two columns
+---------+---------+
| Back    |         |
| to      |         |
| one     |         |
| column  |         | 


> ### @icon-info-circle Use the @icon-table Table Editor 
> For easier table data entry and pretty rendered tables you can use the table editor which provides grid based table data entry. You can use the table editor with **Pipe**, **Grid** and **HTML** tables.


