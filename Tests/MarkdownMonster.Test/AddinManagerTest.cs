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

        [TestMethod]
        public void DownloadAndInstall()
        {
            const string url =
                "https://github.com/RickStrahl/SaveToAzureBlob-MarkdownMonster-Addin/raw/master/Build/SaveImageToAzureBlob-MarkdownMonster-Addin.zip";

            var manager = new AddinManager();
            bool result = manager.DownloadAndInstallAddin(url, "c:\\program files\\Markdown Monster\\Addins\\Install\\");

            Assert.IsTrue(result);
        }

    }
}
