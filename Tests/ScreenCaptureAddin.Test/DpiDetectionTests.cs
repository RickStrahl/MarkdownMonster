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
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

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
