using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.AddIns;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class AddinManagerTest
    {
        [TestMethod]
        public void GetAddinListTest()
        {
            var manager = new AddinManager();
            var list = manager.GetAddinList();

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod()]
        public async Task GetAddinListAsyncTest()
        {
            var manager = new AddinManager();
            var list = await manager.GetAddinListAsync();

            foreach (var ai in list)
            {
                Console.WriteLine(ai.name + "  " + ai.updated);
            }

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod]
        public void DownloadAndInstall()
        {
            const string url =
                "https://github.com/RickStrahl/SaveToAzureBlob-MarkdownMonster-Addin/raw/master/Build/SaveImageToAzureBlob-MarkdownMonster-Addin.zip";

            var addin = new AddinItem
            {
                id = "SaveImageToAzureBlob"
            };

            var manager = new AddinManager();
            var result = manager.DownloadAndInstallAddin(url, "c:\\program files (x86)\\Markdown Monster\\Addins",addin);

            Assert.IsTrue(!result.IsError);
        }

        [TestMethod]
        public void InstallAddinFiles()
        {
            var manager = new AddinManager();
            manager.InstallAddinFiles("c:\\program files\\Markdown Monster\\Addins\\Install");

            Assert.IsTrue(true);
        }

    }
}
