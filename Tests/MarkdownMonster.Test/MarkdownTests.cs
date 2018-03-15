using System;
using Markdig.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class MarkdownTests
    {
        [TestMethod]
        public void TestStrikeOut()
        {
            string markdown = "This is **bold** and this is ~~strike out text~~ and this is ~~too~~. This ~~ is text \r\n that continues~~.";

            var parser = MarkdownParserFactory.GetParser(true);
            string html = parser.Parse(markdown);

            Console.WriteLine(html);
        }

        [TestMethod]
        public void PragmaLinesTest()
        {
            string markdown = @"# Item 1
This is some Markdown text that is **bold**.

http://west-wind.com

## header 2 
This is ~~strike out text~~ and this is ~~too~~. This ~~is text \r\n that continues~~. 

* Item 1   
askdjlaks jdalksdjalskdj

asdkljaslkdjalskdjasd

<b>This is more text</b>";

            var parser = MarkdownParserFactory.GetParser(usePragmaLines: true );
            string html = parser.Parse(markdown);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("pragma-line-13"));
        }

        [TestMethod]
        public void FontAwesomeTest()
        {
            string markdown = @"
this @icon-gear<span>Text</span>

I can see that this is working @icon-warning";

            var parser = MarkdownParserFactory.GetParser();
            string html = parser.Parse(markdown);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("fa-warning") && html.Contains("fa-gear"));
        }

        [TestMethod]
        public void MarkdownSyntaxTreeForHeaders()
        {

            string md = @"---
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

Header 2
========
";

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

                        // TODO: Need to deal with excluding 
                    }

                    content = content.TrimStart(' ', '#');
                    Console.WriteLine($"{indent} {line} {content}");
                }
            }
            Assert.IsNotNull(syntax);

        }
    }
}
