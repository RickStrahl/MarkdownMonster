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

            var result = MarkdownUtilities.AddLinkReference(markdown,
                new SelectionRange {StartColumn = 0, StartRow = 3, EndColumn = 8, EndRow = 3},
                "https://websurge.west-wind.com");

            Console.WriteLine(result.SelectionLength);
            Console.WriteLine(result.Markdown);
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

            var result = MarkdownUtilities.AddLinkReference(markdown,
                new SelectionRange {StartColumn = 0, StartRow = 3, EndColumn = 8, EndRow = 3},
                "https://websurge.west-wind.com");

            Console.WriteLine(result.Markdown);
            Assert.IsNotNull(result);
        }



        [TestMethod]
        public void HtmlToMarkdownReverseMarkdownFromClipboardTest()
        {
            var html =
                @"<span style=""color: rgb(36, 41, 46); font-family: -apple-system, BlinkMacSystemFont, &quot;Segoe UI&quot;, Helvetica, Arial, sans-serif, &quot;Apple Color Emoji&quot;, &quot;Segoe UI Emoji&quot;, &quot;Segoe UI Symbol&quot;; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-style: initial; text-decoration-color: initial; display: inline !important; float: none;"">Markdown Monster is an easy to use and extensible<span> </span></span><strong style=""box-sizing: border-box; font-weight: 600; color: rgb(36, 41, 46); font-family: -apple-system, BlinkMacSystemFont, &quot;Segoe UI&quot;, Helvetica, Arial, sans-serif, &quot;Apple Color Emoji&quot;, &quot;Segoe UI Emoji&quot;, &quot;Segoe UI Symbol&quot;; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-style: initial; text-decoration-color: initial;"">Markdown Editor</strong><span style=""color: rgb(36, 41, 46); font-family: -apple-system, BlinkMacSystemFont, &quot;Segoe UI&quot;, Helvetica, Arial, sans-serif, &quot;Apple Color Emoji&quot;, &quot;Segoe UI Emoji&quot;, &quot;Segoe UI Symbol&quot;; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-style: initial; text-decoration-color: initial; display: inline !important; float: none;"">,<span> </span></span><strong style=""box-sizing: border-box; font-weight: 600; color: rgb(36, 41, 46); font-family: -apple-system, BlinkMacSystemFont, &quot;Segoe UI&quot;, Helvetica, Arial, sans-serif, &quot;Apple Color Emoji&quot;, &quot;Segoe UI Emoji&quot;, &quot;Segoe UI Symbol&quot;; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-style: initial; text-decoration-color: initial;"">Viewer</strong><span style=""color: rgb(36, 41, 46); font-family: -apple-system, BlinkMacSystemFont, &quot;Segoe UI&quot;, Helvetica, Arial, sans-serif, &quot;Apple Color Emoji&quot;, &quot;Segoe UI Emoji&quot;, &quot;Segoe UI Symbol&quot;; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-style: initial; text-decoration-color: initial; display: inline !important; float: none;""><span> </span>and<span> </span></span><strong style=""box-sizing: border-box; font-weight: 600; color: rgb(36, 41, 46); font-family: -apple-system, BlinkMacSystemFont, &quot;Segoe UI&quot;, Helvetica, Arial, sans-serif, &quot;Apple Color Emoji&quot;, &quot;Segoe UI Emoji&quot;, &quot;Segoe UI Symbol&quot;; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-style: initial; text-decoration-color: initial;"">Weblog Publisher</strong><span style=""color: rgb(36, 41, 46); font-family: -apple-system, BlinkMacSystemFont, &quot;Segoe UI&quot;, Helvetica, Arial, sans-serif, &quot;Apple Color Emoji&quot;, &quot;Segoe UI Emoji&quot;, &quot;Segoe UI Symbol&quot;; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-style: initial; text-decoration-color: initial; display: inline !important; float: none;""><span> </span>for Windows. Our goal is to provide the best Markdown specific editor for Windows and make it as easy as possible to create Markdown documents. We provide a core editor and previewer, and a number of non-intrusive helpers to help embed content like images, links, tables, code and more into your documents with minimal effort.</span>";

            var config = new ReverseMarkdown.Config
            {
                GithubFlavored = true,
                UnknownTags =
                    ReverseMarkdown.Config.UnknownTagsOption
                        .PassThrough, // Include the unknown tag completely in the result (default as well)
                SmartHrefHandling = true // remove markdown output for links where appropriate
            };
            var converter = new ReverseMarkdown.Converter(config);
            string markdown = converter.Convert(html);

            Console.WriteLine(markdown);

            Assert.IsTrue(markdown.Contains("and **Weblog Publisher** for Windows"));

        }
    }

}
