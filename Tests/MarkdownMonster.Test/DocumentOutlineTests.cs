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
    }
}
