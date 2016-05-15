using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WeblogAddin;

namespace WeblogAddin.Test
{
    [TestClass]
    public class WeblogAddinTests
    {
        [TestMethod]
        public void SetConfigInMarkdown()
        {
            var meta = new WeblogPostMetadata()
            {
                Abstract = "THis is an abstract",
                Keywords = "Security,SSL,IIS",
                RawMarkdownBody = MarkdownWithoutPostId,
                PostId = "2",
                WeblogName = "Rick Strahl's Web Log"
            };

            var addin = new WeblogAddin.WebLogAddin();
            string markdown = addin.SetConfigInMarkdown(meta);

            Console.WriteLine(markdown);
            Assert.IsTrue(markdown.Contains("<postid>2</postid>"), "Post Id wasn't added");
        }

        [TestMethod]
        public void GetPostConfigFromMarkdown()
        {

            string markdown = MarkdownWithoutPostId;
            
            var addin = new WeblogAddin.WebLogAddin();
            var meta = addin.GetPostConfigFromMarkdown(markdown);

            Console.WriteLine(JsonConvert.SerializeObject(meta, Formatting.Indented));

            Assert.IsTrue(meta.Abstract == "Abstract");
            Assert.IsTrue(meta.Keywords == "Keywords");
            Assert.IsTrue(meta.WeblogName == "WebLogName");
        }


        string MarkdownWithoutPostId = @"### Summary

The time to look at moving your non-secure sites is now, before it's a do or die scenario like mine was. The all SSL based Web is coming without a doubt and it's time to start getting ready for it now.


<!-- Post Configuration -->
<!---
```xml
<abstract>
Abstract
</abstract>
<categories>
Categories
</categories>
<keywords>
Keywords
</keywords>
<weblog>
WebLogName
</weblog>
```
-->
<!-- End Post Configuration -->
";
    }
}