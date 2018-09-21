using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class MarkdownUtilitiesTests
    {

        [TestMethod]
        public void AddFirstLinkTest()
        {
            string markdown = @"
This is a link to my Web site.

And some more text

And here is some more text.

Here is link.

Done.
";

            var result = MarkdownUtilities.AddLinkReference(markdown, new SelectionRange { StartColumn = 0, StartRow = 3, EndColumn = 8, EndRow = 3 }, "https://markdownmonster.west-wind.com");

            Console.WriteLine(result);
            Assert.IsNotNull(result);
        }
    


    [TestMethod]
        public void ManyLinksTest()
        {
            string markdown = @"
This is a link to my [Web site][1].

And some more text

And here is some more [text][2].

Here is [another link][3].



Done.

  [1]: http://west-wind.com
  [2]: http://weblog.west-wind.com
  [3]: http://markdownmonster.west-wind.com";

            var result = MarkdownUtilities.AddLinkReference(markdown,new SelectionRange { StartColumn=0, StartRow = 3, EndColumn = 8, EndRow = 3},"https://markdownmonster.west-wind.com");

            Console.WriteLine(result);
            Assert.IsNotNull(result);
        }
    }
}
