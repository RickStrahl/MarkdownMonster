using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.RenderExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            string resultMarkdown = ext.ParseNoteTipWarningImportant(markdown);

            Assert.AreNotEqual(markdown, resultMarkdown);

            Console.WriteLine(resultMarkdown);
        }
        
    }
}
