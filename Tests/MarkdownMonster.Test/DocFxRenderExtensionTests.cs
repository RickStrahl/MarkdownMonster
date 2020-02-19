using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.RenderExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class DocFxRenderExtensionTests
    {

        [TestMethod]
        public void TipNoteWarningTest()
        {
            string markdown = @"
> [!WARNING]
> This is some Note Text
> that spreads across two lines

> [!NOTE]
> Singe line note.

Some other text

> [!WARNING]
> asdaksldj alksdjalksdj laksdjalskd
> asdkljasdlkjasdkljasd



> [!TIP]
> This is a tip that is
> shown on two lines
>
> More text here

";

            var ext = new DocFxRenderExtension();

            markdown = StringUtils.NormalizeLineFeeds(markdown, LineFeedTypes.Lf);

            var args = new ModifyMarkdownArguments {Markdown = markdown};
            ext.ParseNoteTipWarningImportant(args);

            Assert.AreNotEqual(markdown, args.Markdown);

            Console.WriteLine(args.Markdown);
        }



        [TestMethod]
        public void EndOfStringTipTest()
        {
            string markdown = @"
Before Text

> [!TIP]
> This is a tip that is
> shown on two lines
>
> More text here";

            var ext = new DocFxRenderExtension();

            markdown = StringUtils.NormalizeLineFeeds(markdown, LineFeedTypes.Lf);

            var args = new ModifyMarkdownArguments {Markdown = markdown};
            ext.ParseNoteTipWarningImportant(args);

            var result = args.Markdown.TrimEnd();
            Assert.AreNotEqual(markdown, result);
            Assert.IsTrue(result.EndsWith("</div>"));

            Console.WriteLine(result);

        }


        [TestMethod]
        public void XrefTagTest()
        {
            string markdown = @"
Before Text

<xref:subfolder/page>


<xref:subfolder/page2/>

> [!TIP]
> This is a tip that is
> shown on two lines
>
> More text here";

            var ext = new DocFxRenderExtension();

            markdown = StringUtils.NormalizeLineFeeds(markdown, LineFeedTypes.Lf);

            var args = new ModifyMarkdownArguments {Markdown = markdown};
            ext.ParseXrefTags(args);

            var result = args.Markdown.TrimEnd();
            Assert.AreNotEqual(markdown, result);
            Assert.IsTrue(result.Contains("href=\"subfolder/page\"") &&
                          result.Contains("href=\"subfolder/page2\""));
            Console.WriteLine(result);
        }

    }
}
