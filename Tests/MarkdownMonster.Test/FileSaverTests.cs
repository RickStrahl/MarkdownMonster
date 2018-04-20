using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class FileSaverTests
    {
        [TestMethod]
        public void GithubUrlFullFileFormatting()
        {
            var fs = new FileSaver();

            string mkdUrl = "https://github.com/RickStrahl/MarkdownMonster/blob/master/Readme.md";
            var url = fs.ParseMarkdownUrl(mkdUrl);
            Console.WriteLine(url);
            Assert.IsTrue(HttpUtils.HttpRequestString(url).Contains("# "), "Explicit readme: Not markdown");
        }

        [TestMethod]
        public void GithubRootUrlTest()
        {
            var fs = new FileSaver();

            var mkdUrl = "https://github.com/angular/angular";
            var url = fs.ParseMarkdownUrl(mkdUrl);
            Console.WriteLine(url);
            Assert.IsTrue(HttpUtils.HttpRequestString(url).Contains("# "), "Root path: Not markdown");

            mkdUrl = "https://github.com/rickstrahl/MarkdownMonster";
            url = fs.ParseMarkdownUrl(mkdUrl);
            Console.WriteLine(url);
            Assert.IsTrue(HttpUtils.HttpRequestString(url).Contains("# "), "Root path: Not markdown");
        }

        [TestMethod]
        public void GistTests()
        {
            var fs = new FileSaver();
            string mkdUrl = "https://gist.github.com/RickStrahl/6d8757cf45b8eff7d15914b9c62092b2";

            var url = fs.ParseMarkdownUrl(mkdUrl);
            Console.WriteLine(url);
            Assert.IsTrue(HttpUtils.HttpRequestString(url).Contains("# "), "Root path: Not markdown");
        }


        [TestMethod]
        public void MicrosoftDocsTest()
        {
            var fs = new FileSaver();
            string mkdUrl = "https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/working-with-linq";
            
            var url = fs.ParseMarkdownUrl(mkdUrl);
            Console.WriteLine(url);
            Assert.IsTrue(HttpUtils.HttpRequestString(url).Contains("# "), "Root path: Not markdown");
        }
    }
}
