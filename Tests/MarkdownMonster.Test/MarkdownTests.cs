using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
