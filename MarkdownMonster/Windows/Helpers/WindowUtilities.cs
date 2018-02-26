using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// WPF Helpers for MM
    /// </summary>
    public class WindowUtilities
    {
        private delegate void EmptyDelegate();


        /// <summary>
        /// Idle loop to let events fire in the UI
        /// 
        /// Use SPARINGLY or not at all if there is a better way
        /// but there are a few places where this is required.
        /// </summary>
        public static void DoEvents()
        {
            try
            {
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new EmptyDelegate(delegate { }));
            }
            catch
            {                
            }
        }

        /// <summary>
        /// Forces lost focus on the active control in a Window so that the a toolbar click 
        /// works properly accepting the last controls value input.
        /// </summary>
        /// <param name="window"></param>
        public static void FixFocus(Window window, System.Windows.Controls.Control control)
        {
            var ctl = FocusManager.GetFocusedElement(window);
            if (ctl == null)
                return;

            control.Focus();
            DoEvents();
            ctl.Focus();
        }

        /// <summary>
        /// Finds a particular type of control in the children of a top level control
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    var child = VisualTreeHelper.GetChild(obj, i);
                    if (child is T)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }


        /// <summary>
        /// Finds a type of element in the parent chain of an element
        /// </summary>
        /// <typeparam name="T">Type of Element to find</typeparam>
        /// <param name="current">start element</param>
        /// <returns></returns>
        public static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T) return (T) current;

                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);

            return null;
        }

        /// <summary>
        /// Creates a keyboard shortcut from a 
        /// </summary>
        /// <param name="ksc"></param>
        /// <param name="command"></param>
        /// <returns>KeyBinding - Window.InputBindings.Add(keyBinding)</returns>
        public static KeyBinding CreateKeyboardShortcutBinding(string ksc, ICommand command)
        {
            if (string.IsNullOrEmpty(ksc))
                return null;

            try
            {
                KeyBinding kb = new KeyBinding();
                ksc = ksc.ToLower();

                if (ksc.Contains("alt"))
                    kb.Modifiers = ModifierKeys.Alt;
                if (ksc.Contains("shift"))
                    kb.Modifiers |= ModifierKeys.Shift;
                if (ksc.Contains("ctrl") || ksc.Contains("ctl"))
                    kb.Modifiers |= ModifierKeys.Control;
                if (ksc.Contains("win"))
                    kb.Modifiers |= ModifierKeys.Windows;

                string key =
                    ksc.Replace("+", "")
                        .Replace("-", "")
                        .Replace("_", "")
                        .Replace(" ", "")
                        .Replace("alt", "")
                        .Replace("shift", "")
                        .Replace("ctrl", "")
                        .Replace("ctl", "");

                key = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key);
                if (!string.IsNullOrEmpty(key))
                {
                    KeyConverter k = new KeyConverter();
                    kb.Key = (Key) k.ConvertFromString(key);
                }

                // Whatever command you need to bind to
                kb.Command = command;

                return kb;
            }
            // deal with invalid bindings - ignore them
            catch(Exception ex)
            {
                mmApp.Log("Unable to assign key binding: " + ksc, ex);
                return null; 
            }
        }

        #region Bitmap Conversions

		/// <summary>
		/// Converts a bitmap source to a bitmap
		/// Make sure to dispose the bitmap
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
        public static Bitmap BitmapSourceToBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppPArgb);

            BitmapData data = bmp.LockBits(
                new Rectangle(Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);

            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height*data.Stride,
                data.Stride);

            bmp.UnlockBits(data);
			
            return bmp;
        }


        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource BitmapToBitmapSource(Bitmap bmp)
        {
            var hBitmap = bmp.GetHbitmap();
            var imageSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap);
            return imageSource;
        }


        #endregion

        #region Make Window Transparent
        /// <summary>
        /// Call this to make a window completely click through including all controls
        /// on it.
        /// </summary>
        /// <example>
        /// ///
        /// protected override void OnSourceInitialized(EventArgs e)
        /// {
        ///    base.OnSourceInitialized(e);
        ///    var hwnd = new WindowInteropHelper(this).Handle;
        ///    WindowsServices.SetWindowExTransparent(hwnd);
        /// }
        ///</example>
        public static void MakeWindowCompletelyTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }

        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        #endregion


        #region GetDpiRatio

        public static decimal GetDpiRatio(Window window)
        {
            var dpi = WindowUtilities.GetDpi(window, DpiType.Effective);
            decimal ratio = 1;
            if (dpi > 96)
                ratio = (decimal)dpi / 96M;

            return ratio;
        }




        public static decimal GetDpiRatio(IntPtr hwnd)
        {            
            var dpi = GetDpi(hwnd, DpiType.Effective);            
            decimal ratio = 1;
            if (dpi > 96)
                ratio = (decimal)dpi / 96M;

            //Debug.WriteLine($"Scale: {factor} {ratio}");
            return ratio;
        }



        public static uint GetDpi(IntPtr hwnd, DpiType dpiType)
        {
            var screen = Screen.FromHandle(hwnd);            
            var pnt = new Point(screen.Bounds.Left, screen.Bounds.Top);

            var mon = MonitorFromPoint(pnt, 2 /*MONITOR_DEFAULTTONEAREST*/);

            try
            {
                uint dpiX, dpiY;
                GetDpiForMonitor(mon, dpiType, out dpiX, out dpiY);

                //Debug.WriteLine($"dpi: {dpiX} on mon {mon}");
                return dpiX;
            }
            catch
            {
                // fallback for Windows 7 and older - not 100% reliable
                Graphics graphics = Graphics.FromHwnd(hwnd);
                float dpiXX = graphics.DpiX;                
                return Convert.ToUInt32(dpiXX);
            }
        }

        public static uint GetDpi(System.Drawing.Point point, DpiType dpiType)
        {                       
            var mon = MonitorFromPoint(point, 2 /*MONITOR_DEFAULTTONEAREST*/);

            try
            {
                uint dpiX, dpiY;
                GetDpiForMonitor(mon, dpiType, out dpiX, out dpiY);
                return dpiX;
            }
            catch
            {
                // fallback for Windows 7 and older - not 100% reliable
                Graphics graphics = Graphics.FromHdc(mon);
                float dpiXX = graphics.DpiX;
                return Convert.ToUInt32(dpiXX);
            }
        }




        public static uint GetDpi(Window window, DpiType dpiType)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            return GetDpi(hwnd, dpiType);
        }

   
        //https://msdn.microsoft.com/en-us/library/windows/desktop/dd145062(v=vs.85).aspx
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);

        //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510(v=vs.85).aspx
        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor, [In]DpiType dpiType, [Out]out uint dpiX, [Out]out uint dpiY);

        #endregion

        #region SetDPIAwareness

        /// <summary>
        /// IMPORTANT: This only works if this is called in the immediate startup code
        /// of the application. For WPF this means `static App() { }`.
        /// </summary>
        public static bool SetPerMonitorDpiAwareness(ProcessDpiAwareness type = ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware)
        {
            try
            {
                // for this to work make sure [assembly: DisableDpiAwareness]
                ProcessDpiAwareness awarenessType;
                GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out awarenessType);
                var result = SetProcessDpiAwareness(type);
                GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out awarenessType);

                return awarenessType == type;
            }
            catch
            {
                return false;
            }            
        }
        
        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(ProcessDpiAwareness awareness);

        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern void GetProcessDpiAwareness(IntPtr hprocess, out ProcessDpiAwareness awareness);
        
        #endregion

    }

    public enum ProcessDpiAwareness
    {
        Process_DPI_Unaware = 0,
        Process_System_DPI_Aware = 1,
        Process_Per_Monitor_DPI_Aware = 2
    }

    //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280511(v=vs.85).aspx
    public enum DpiType
    {
        Effective = 0,
        Angular = 1,
        Raw = 2,
    }
}
