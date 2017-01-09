using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using MarkdownMonster.Windows;
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

            //bool result =  WindowUtilities.SetPerMonitorDpiAwareness();
            Console.WriteLine("DPI Set to per monitor DPI");
           
        }


        ///// <summary>
        ///// Can't get this to work.
        ///// </summary>
        //[TestMethod]
        //public void GetDpiSettingsForMultipleScreens()
        //{
        //    Screen s1 = Screen.AllScreens[0];
        //    Screen s2 = Screen.AllScreens[1];

        //    Console.WriteLine(s1.Bounds.Left);
        //    var hwnd = ScreenCaptureAddin.ScreenCapture.WindowFromPoint(new System.Drawing.Point(s1.Bounds.Left + 1, s1.Bounds.Top + 1));
        //    Console.WriteLine(hwnd);
        //    Console.WriteLine(MarkdownMonster.Windows.WindowUtilities.GetDpiRatio(hwnd));



        //    Console.WriteLine(s2.Bounds.Left);
        //    hwnd = ScreenCaptureAddin.ScreenCapture.WindowFromPoint(new System.Drawing.Point(s2.Bounds.Left + 1, s2.Bounds.Top + 1));
        //    Console.WriteLine(hwnd);
        //    Console.WriteLine(MarkdownMonster.Windows.WindowUtilities.GetDpiRatio(hwnd));

        //    Console.WriteLine(s2.Bounds.Left);
        //}



        //[TestMethod]
        //public void GetDpiForMultipleScreensNativeLocal()
        //{
        //    var s1 = Screen.AllScreens[0];
        //    var s2 = Screen.AllScreens[1];

        //    var point = new System.Drawing.Point(s1.Bounds.Left, s1.Bounds.Top);
        //    var res = WindowUtilities.GetDpi(point,DpiType.Effective);
            
        //    Console.WriteLine($"Screen2: DPI: " + res );
        //}


        [TestMethod]
    public void GetDpiForMultipleScreensWindowUtilities()
    {
        var s1 = Screen.AllScreens[0];
        var s2 = Screen.AllScreens[1];

        var hmon = MonitorFromPoint(new System.Drawing.Point(s1.Bounds.Left, s1.Bounds.Top), 2 /* MONITOR_DEFAULTTONEAREST */);

        uint dpiX, dpiY;
        GetDpiForMonitor(hmon, DpiType.Effective, out dpiX, out dpiY);

        Console.WriteLine($"Screen1: DPI: " + dpiX + " " + hmon);


        hmon = MonitorFromPoint(new System.Drawing.Point(s2.Bounds.Left, s2.Bounds.Top), 2 /* MONITOR_DEFAULTTONEAREST */);

        dpiX = 0;
        dpiY = 0;
        GetDpiForMonitor(hmon, DpiType.Effective, out dpiX, out dpiY);

        Console.WriteLine($"Screen2: DPI: " + dpiX + " " + hmon);
    }


    [DllImport("User32.dll")]
    private static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);

    //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510(v=vs.85).aspx
    [DllImport("Shcore.dll")]
    private static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor, [In]DpiType dpiType, [Out]out uint dpiX, [Out]out uint dpiY);

        //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280511(v=vs.85).aspx
        public enum DpiType
        {
            Effective = 0,
            Angular = 1,
            Raw = 2,
        }
    }
}
