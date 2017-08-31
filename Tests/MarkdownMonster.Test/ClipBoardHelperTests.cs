using System;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{
    /// <summary>
    /// Summary description for ClipBoardHelper
    /// </summary>
    [TestClass]
    public class ClipBoardHelperTests
    {
      
        [TestMethod]
        public void CopyHtmlToClipboard()
        {
            string initialText = "<b>Hello World</b>";
            ClipboardHelper.CopyHtmlToClipboard(initialText,initialText);

            var htmlFragmentText = Clipboard.GetData(DataFormats.Html) as string;

            Assert.IsNotNull(htmlFragmentText);
            Console.WriteLine(htmlFragmentText);

            string html = StringUtils.ExtractString(htmlFragmentText, "<!--StartFragment-->", "<!--EndFragment-->");
            Assert.AreEqual(initialText,html);
        }

        [TestMethod]
        public void GetClipboardHtml()
        {
            var text = Clipboard.GetText();
            Console.WriteLine(text);

            var html = Clipboard.GetData(DataFormats.Html) as string;
            html = StringUtils.ExtractString(html, "<!--StartFragment-->", "<!--EndFragment-->");
            Console.WriteLine(html);
        }

        [TestMethod]
        public void GetClipboardHtmlText()
        {
            string html = ClipboardHelper.GetHtmlFromClipboard();
            Console.WriteLine(html);
        }
    }
}
