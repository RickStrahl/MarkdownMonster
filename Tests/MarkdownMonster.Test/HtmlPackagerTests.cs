using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using MarkdownMonster.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{

    /// <summary>
    /// To run these tests make sure an image is on the clipboard
    /// </summary>
    [TestClass]
    public class HtmlPackagerTests
    {
        private string InputFile;

        public HtmlPackagerTests()
        {
            InputFile = Path.Combine(Path.GetTempPath(), "_MarkdownMonster_Preview.html");
        }

        [TestMethod]
        public void Package()
        {
            var packager = new HtmlPackager();
            string packaged = packager.PackageLocalHtml(InputFile);

            string outputFile = InputFile.Replace(".html", "_PACKAGED.html");
            File.WriteAllText(outputFile,packaged);

            ShellUtils.GoUrl(outputFile);

            Console.WriteLine(packaged);

            Assert.IsNotNull(packaged);

        }
    }
}
