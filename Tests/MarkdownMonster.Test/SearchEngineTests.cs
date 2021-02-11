using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarkdownMonster.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class SearchEngineTests
    {
        [TestMethod]
        public void OpenSearchEngineTest()
        {
            var engine = new SearchEngine();
            engine.OpenSearchEngine("anti-trust Punk Rock");
        }


        // UI test doesn't work
        [TestMethod]
        public async Task GetSearchLinksTest()
        {
            var searchTerm = "Rider";
            var engine = new SearchEngine();
            var list = await engine.GetSearchLinks(searchTerm);

            Assert.IsNotNull(list);

            foreach (var link in list)
            {
                Console.WriteLine(link.Title + "\n\t" + link.Url);
            }

        }


    }
}
