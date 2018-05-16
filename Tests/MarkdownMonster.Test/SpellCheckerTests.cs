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
    public class SpellCheckerTests
    {
        [TestMethod]
        public void GetDictionaryListStringTest()
        {
            var code = SpellChecker.GetDictionaryListStringFromWebSite();
            Console.WriteLine(code);
            Clipboard.SetText(code);

        }
    }
}
