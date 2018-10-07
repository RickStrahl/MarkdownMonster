using System;
using Markdig.Syntax;
using MarkdownMonster.Windows.DocumentOutlineSidebar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class DocumentOutlineTests
    {

        [TestMethod]
        public void MarkdownSyntaxTreeForHeaders()
        {

            string md = MarkdownText;
            var syntax = Markdig.Markdown.Parse(md);
            var lines = StringUtils.GetLines(md);
            bool inFrontMatter = false;

            foreach (var item in syntax)
            {
                var line = item.Line;
                var content = lines[line];

                if (line == 0 && content == "---")
                {
                    inFrontMatter = true;
                    continue;
                }

                if (inFrontMatter && content == "---")
                {
                    inFrontMatter = false;
                    continue;
                }

                if (item is HeadingBlock)
                {
                    var heading = item as HeadingBlock;
                    var indent = "".PadRight(heading.Level);

                    // underlined format
                    if (line > 0 && (content.StartsWith("---") || content.StartsWith("===")))
                    {
                        line--;
                        content = lines[line].TrimStart(' ', '#');
                    }

                    content = content.TrimStart(' ', '#');
                    Console.WriteLine($"{indent} {line} {content}");
                }
            }

            Assert.IsNotNull(syntax);
        }

        [TestMethod]
        public void CreateDocumentOutlineTest()
        {

            var appModel = new AppModel(null);
            mmApp.Model = appModel;

            string md = MarkdownText;

            var model = new DocumentOutlineModel();
            var outline = model.CreateDocumentOutline(md);

            Assert.IsNotNull(outline);

            foreach (var item in outline)
            {
                Console.WriteLine($"{StringUtils.Replicate('\t', item.Level)}{item.Text} ({item.Line})");
            }

            Assert.IsTrue(outline[0].Level == 1 && outline[1].Level == 2);
        }


        [TestMethod]
        public void CreateDocumentOutlineWithLinkReferenceBlock()
        {

            var appModel = new AppModel(null);
            mmApp.Model = appModel;

            string md = MarkdownText2;

            var model = new DocumentOutlineModel();
            var outline = model.CreateDocumentOutline(md);

            Assert.IsNotNull(outline);

            foreach (var item in outline)
            {
                Console.WriteLine($"{StringUtils.Replicate('\t', item.Level)}{item.Text} ({item.Line})");
            }

            Assert.IsTrue(outline[0].Level == 1 && outline[1].Level == 1 && outline[2].Level == 2);
        }



        [TestMethod]
        public void MarkdownOutlineTest()
        {
            var appModel = new AppModel(null);
            mmApp.Model = appModel;

            var doc = new MarkdownDocument();
            doc.CurrentText = MarkdownText;


            var model = new DocumentOutlineModel();
            var md = model.CreateMarkdownOutline(doc);

            Console.WriteLine(md);



        }


        const string MarkdownText = @"---
title: Name
---
# Heading

This is some text.

## Heading 2
More text aslkdjalskdjasd asdkljasdkjasd
asdkljadslkjasdlkj
asdkljasdlkjasdlkajsdlkasjd
asdkljasdlkjasd

## Heading 2.2
More text aslkdjalskdjasd asdkljasdkjasd
asdkljadslkjasdlkj
asdkljasdlkjasdlkajsdlkasjd
asdkljasdlkjasd

### Heading 3
asdlka;ls dkalsdk asdlkasdkasd
asdjkaksldjasd
kjasdlkjasdkajsdkasd
asdlkjasldkjasdkjl

asdklajsdlkajsdajksd
asdddasdddasdsda

#### Heading 4
asdlkasdlkasd;lkasd;laksd

## Heading 2.3
as;dlkja;sldkasd asdla;skdasd

Header
------

Header 1
========
";


        private string MarkdownText2 = @"---
title: 'Tip: Open Visual Studio Code from Visual Studio as an External Tool'
abstract: I use both Visual Studio and Visual Studio Code in my development. Although I tend to still default to the full version of Visual Studio, I tend to run Visual Studio Code side by side with Visual Studio and flip back and forth a lot. To make things a little easier and being able to jump directly to a document in VS Code from full VS you can create an External Tool entry and a shortcut mapping to quickly open documents and/or folders in VS Code.
categories: Visual Studio, Visual Studio Code
keywords: Visual Studio, Visual Studio Code, External Tools, ShortCut
weblogName: West Wind Web Log
postId: 962289
postDate: 2018-10-06T11:33:07.7553542-10:00
---
# Tip: Open Visual Studio Code from Visual Studio as an External Tool

[//]: # (TOC Begin)

* [Using Both Visual Studio and Visual Studio Code](#using-both-visual-studio-and-visual-studio-code)
* [Open in Visual Studio Code as an External Tool](#open-in-visual-studio-code-as-an-external-tool)
* [Switch Me](#switch-me)

[//]: # (TOC End)


When working on .NET and mostly server side Web development I tend to use the full version of Visual Studio. I also use Visual Studio Code separately quite often for client side development as well as a general purpose editor. 

Although Visual Studio Code is pretty awesome and getting more feature rich all the time (mostly via plug-ins), the full version of Visual Studio still has many editor enhancements - especially in the way of auto-completion for dependencies and navigation to dependencies - that are simply much better integrated than VS Code. CSS intellisense, library auto-completions, drag and drop of scripts and cross document (f12) navigation of code, css and script as well as the host of refactoring options (especially with Resharper) are just a few of the things that make full Visual Studio more productive to me that I often default to Visual Studio rather the VS Code.

Test Underline Level 1
======================

Until I'm heads down typing gobs of HTML or JavaScript... and **then** I prefer VS Code. Because lets face it the full Visual Studio is pretty laggy (even with my new 12 core beast of a laptop) especially if you have third party refactoring/code analysis tools installed (hint: Resharper which I turn off most of the time now due to the overhead it causes). The VS Code editor just feels buttery smooth especially compared to the full VS editor and that can make writing gobs of code much quicker in some situations. I also love the simple way you can create code snippets in VS code, compared to the more clunky and verbose Visual Studio snippet syntax let alone having to hunt down the snippets in an obscure buried folder. Finally many client side tool addons for modern libraries and snippets show up in Visual Studio Code and not in Visual Studio. For example, to get a decent set of Bootstrap 4 snippets I had to go to VS code.

Test Underline 2
----------------
More text goes here



## Using Both Visual Studio and Visual Studio Code
Long story short: 

* For most Web projects today I use both Visual Studio and Visual Studio Code **at the same time**
* Both have features that the other is lacking
* I switch back and forth between them quite frequently

Since most of my projects tend to be .NET based, I usually start in Visual Studio and then use VS Code as a fallback when I need things that VS Code tends to do better.

## Open in Visual Studio Code as an External Tool
So here's a little tip that makes things easier: A quick way to open VS Code from Visual Studio. Now I know [Mads Kristensen has a Open in Visual Studio Code Extension](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.OpeninVisualStudioCode), but I dunno - an extension seems a bit much when you can simply set up an external tool mapping.

![](https://weblog.west-wind.com/images/2018/Open-Visual-Studio-Code-from-Visual-Studio/ExternalToolOpenInVsCode.png)

For bonus points you can also add a hotkey which you can do by using **Tools->Options->Keyboard** and then searching for **ExternalCommand**. Command hot keys can be set in positional order so in the figure the third item is my VS Code opener that I assign *Ctrl-Alt-V* to.

![](https://weblog.west-wind.com/images/2018/Open-Visual-Studio-Code-from-Visual-Studio/AssignHotKey.png)


The one downside to External Tools is that they are not easily exportable. So if you re-install Visual Studio on another machine those same external applications have to be reconfigured. Also the hotkeys are only positional so you have to make sure you keep your hotkeys synced to the items you keep on this menu in the proper order. Not a big deal - I tend to have very few of these and the order rarely matters.

## Switch Me
So now when I'm working in Visual Studio and I quickly want to jump to Visual Studio code I can do so easily by pressing *Ctrl-Alt-V*. Visual Studio Code is also smart enough to know if a folder or file is already open and it jumps me right back to the already open 'project' and or file so if I flip back and forth I can even keep my place in the document where I left off.

It's the little things that make things easier...";
    }
}
