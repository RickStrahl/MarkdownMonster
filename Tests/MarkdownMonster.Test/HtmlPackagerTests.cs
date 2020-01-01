using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using MarkdownMonster.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.HtmlPackager;
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
            InputFile = Path.Combine(Path.GetTempPath(), MarkdownDocument.PREVIEW_HTML_FILENAME);
        }

        [TestMethod]
        public void PackageFromFileTest()
        {
            var packager = new HtmlPackager();
            string packaged = packager.PackageHtml(InputFile);

            string outputFile = InputFile.Replace(".html", "_PACKAGED.html");
            File.WriteAllText(outputFile,packaged);

            ShellUtils.GoUrl(outputFile);

            Console.WriteLine(packaged);

            Assert.IsNotNull(packaged);

        }

        [TestMethod]
        public void PackageFromWebTest()
        {
            var packager = new HtmlPackager();
            string packaged = packager.PackageHtml("https://west-wind.com");

            string outputFile = InputFile.Replace(".html", "_PACKAGED.html");
            File.WriteAllText(outputFile, packaged);

            ShellUtils.GoUrl(outputFile);

            Console.WriteLine(packaged);

            Assert.IsNotNull(packaged);

        }

        [TestMethod]
        public void PackageLooseFilesLocalTest()
        {
            var packager = new HtmlPackager();
            string outputFile = @"c:\temp\GeneratedHtml\Output.html";
            bool result = packager.PackageHtmlToFolder(@"c:\temp\tmpFiles\_MarkdownMonster_Preview.html", outputFile,
                null, true);          
            Assert.IsTrue(result);

            ShellUtils.GoUrl(outputFile);

            
            
        }

        [TestMethod]
        public void PackageLooseFilesWebUrlTest()
        {
            var packager = new HtmlPackager();
            string outputFile = @"c:\temp\GeneratedHtml\Output.html";
            bool result = packager.PackageHtmlToFolder("http://west-wind.com/",outputFile,null,true);
            Assert.IsTrue(result);

            ShellUtils.GoUrl(outputFile);



        }
    }
}
