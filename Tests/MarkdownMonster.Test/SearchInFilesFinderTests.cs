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
    public class SearchInFilesFinderTests
    {

        private string STR_Folder = Environment.ExpandEnvironmentVariables("%localappdata%\\Markdown Monster");

        [TestMethod]
        public void SearchInFilesTest()
        {

            var finder = new SearchInFilesFinder(STR_Folder,"*.*");

            var result = finder.SearchFiles("Markdown");

            Assert.IsTrue(result.Count > 0, "No matches - should have matched");
            WriteResult(result);
        }

        [TestMethod]
        public async Task SearchInFilesAsyncTest()
        {

            var finder = new SearchInFilesFinder(STR_Folder, "*.*");

            var result = await finder.SearchFilesAsync("Single line breaks");

            Assert.IsTrue(result.Count > 0, "No matches - should have matched");
            WriteResult(result);
        }


        public void WriteResult(List<SearchFileResult> results)
        {
            foreach (var res in results)
            {
                Console.WriteLine(res.Filename);
                foreach (var match in res.Matches)
                {
                    Console.WriteLine($"... Pos: {match.StartPos} End: {match.EndPos}");
                }
            }

        }
        
    }
}
