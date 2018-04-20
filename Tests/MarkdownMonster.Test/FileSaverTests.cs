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
        public void GithubUrlFormatting()
        {
            var fs = new FileSaver();

            string mkdUrl = "https://github.com/RickStrahl/MarkdownMonster/blob/master/Readme.md";
            var url = fs.ParseMarkdownUrl(mkdUrl);
            Console.WriteLine(url);
            Assert.IsTrue(HttpUtils.HttpRequestString(url).Contains("# "), "Explicit readme: Not markdown");

            mkdUrl = "https://github.com/angular/angular";
            url = fs.ParseMarkdownUrl(mkdUrl);
            Console.WriteLine(url);
            Assert.IsTrue(HttpUtils.HttpRequestString(url).Contains("# "), "Root path: Not markdown");

        }
    }
}
