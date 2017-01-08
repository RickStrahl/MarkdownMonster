using System;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SnagItAddIn.Test
{
    /// <summary>
    /// Summary description for DpiDetectionTests
    /// </summary>
    [TestClass]
    public class DpiDetectionTests
    {
        public DpiDetectionTests()
        {

        }

       
        /// <summary>
        /// Can't get this to work.
        /// </summary>
        [TestMethod]
        public void GetDpiSettingsForMultipleScreens()
        {
            Screen s1 = Screen.AllScreens[0];
            Screen s2 = Screen.AllScreens[1];

            Console.WriteLine(s1.Bounds.Left);
            var hwnd = ScreenCaptureAddin.ScreenCapture.WindowFromPoint(new System.Drawing.Point(s1.Bounds.Left + 1, s1.Bounds.Top + 1));
            Console.WriteLine(hwnd);
            Console.WriteLine(MarkdownMonster.Windows.WindowUtilities.GetDpiRatio(hwnd));



            Console.WriteLine(s2.Bounds.Left);
            hwnd = ScreenCaptureAddin.ScreenCapture.WindowFromPoint(new System.Drawing.Point(s2.Bounds.Left + 1, s2.Bounds.Top + 1));
            Console.WriteLine(hwnd);
            Console.WriteLine(MarkdownMonster.Windows.WindowUtilities.GetDpiRatio(hwnd));

            Console.WriteLine(s2.Bounds.Left);
        }
    }
}
