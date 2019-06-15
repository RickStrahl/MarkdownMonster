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
> asdaksldj alksdjalksdj laksdjalskd
> asdkljasdlkjasdkljasd

More text here
";

            var ext = new DocFxRenderExtension();

            markdown = StringUtils.NormalizeLineFeeds(markdown, LineFeedTypes.Lf);

            var args = new ModifyMarkdownArguments {Markdown = markdown};
            ext.ParseNoteTipWarningImportant(args);

            Assert.AreNotEqual(markdown, args.Markdown);

            Console.WriteLine(args.Markdown);
        }
        
    }
}
