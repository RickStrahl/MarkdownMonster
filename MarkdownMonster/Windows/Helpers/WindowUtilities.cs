using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
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

        static readonly FieldInfo DisableProcessCountField =
            typeof(Dispatcher).GetField("_disableProcessingCount", BindingFlags.Instance | BindingFlags.NonPublic);

        private static void EmptyMethod()
        {
        }

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
                // This can fail if the dispatcher is disabled by another process
                if (!IsDispatcherDisabled())
                    Dispatcher.CurrentDispatcher.Invoke(EmptyMethod, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                mmApp.Log("DoEvents failed", ex);
            }
        }

        public static bool IsDispatcherDisabled(Dispatcher dispatcher = null)
        {
            if (dispatcher == null)
                dispatcher = Dispatcher.CurrentDispatcher;

            // This can fail if the dispatcher is disabled by another process
            if (DisableProcessCountField == null)
                return true;

            return (int) DisableProcessCountField.GetValue(dispatcher) != 0;
        }

        /// <summary>
        /// Forces lost focus on the active control in a Window to force the selected control
        /// to databind.
        /// Typical scenario: Toolbar clicks (which don't cause a focus change) don't see
        /// latest control state of the active control because it doesn't know focus has
        /// changed. This forces the active control to unbind       
        /// </summary>
        /// <param name="window">Active window</param>
        /// <param name="control">Control to force focus to briefly to force active control to bind</param>
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
                        return (T) child;
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
            } while (current != null);

            return null;
        }


        /// <summary>
        /// Retrieves a nested TreeViewItem by walking the hierarchy.
        /// Specify a root treeview or treeviewitem and it then walks
        /// the hierarchy to find the item
        /// </summary>
        /// <param name="item">Item to find</param>
        /// <param name="treeItem">Parent item to start search from</param>
        /// <returns></returns>
        public static TreeViewItem GetNestedTreeviewItem(object item, ItemsControl treeItem)
        {
            var titem = treeItem
                .ItemContainerGenerator
                .ContainerFromItem(item) as TreeViewItem;

            if (titem != null)
                return titem;

            foreach (var childItem in treeItem.Items)
            {
                titem = treeItem
                    .ItemContainerGenerator
                    .ContainerFromItem(childItem) as TreeViewItem;

                if (titem == null) continue;

                titem = GetNestedTreeviewItem(item, titem);
                if (titem != null)
                    return titem;
            }

            return null;
        }


        /// <summary>
        /// Creates a keyboard shortcut from a 
        /// </summary>
        /// <param name="ksc"></param>
        /// <param name="command"></param>
        /// <returns>KeyBinding - Window.InputBindings.Add(keyBinding)</returns>
        public static KeyBinding xCreateKeyboardShortcutBinding(string ksc, ICommand command,
            object commandParameter = null)
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
                if (commandParameter != null)
                    kb.CommandParameter = commandParameter;

                return kb;
            }
            // deal with invalid bindings - ignore them
            catch (Exception ex)
            {
                mmApp.Log("Unable to assign key binding: " + ksc, ex);
                return null;
            }
        }

        #region Menus

        /// <summary>
        /// Invalidates a menu control and all of its subitems
        /// by checking Command.IsEnabled property if a command exists
        /// </summary>
        /// <param name="menu"></param>
        public static void InvalidateMenuCommands(System.Windows.Controls.Menu menu)
        {

            foreach (var item in menu.Items)
            {
                var mi = item as System.Windows.Controls.MenuItem;
                if (mi == null)
                    continue;

                InvalidateSubmenuCommands(mi);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuItem"></param>
        public static void InvalidateSubmenuCommands(System.Windows.Controls.MenuItem menuItem)
        {
            if (menuItem == null)
                return;

            foreach (var item in menuItem.Items)
            {
                var mi = item as System.Windows.Controls.MenuItem;
                if (mi == null)
                    continue;

                if (mi.Command != null)
                    mi.IsEnabled = mi.Command.CanExecute(mi.CommandParameter);

                if (mi.HasItems)
                    InvalidateSubmenuCommands(mi);
            }
        }

        #endregion

        #region Bitmap Conversions

        /// <summary>
        /// Converts a bitmap source to a bitmap
        /// Make sure to dispose the bitmap
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap BitmapSourceToBitmap(BitmapSource source)
        {
            if (source == null)
                return null;

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
                data.Height * data.Stride,
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
            var imageSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero,
                System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
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
                ratio = (decimal) dpi / 96M;

            return ratio;
        }




        public static decimal GetDpiRatio(IntPtr hwnd)
        {
            var dpi = GetDpi(hwnd, DpiType.Effective);
            decimal ratio = 1;
            if (dpi > 96)
                ratio = (decimal) dpi / 96M;

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
        private static extern IntPtr MonitorFromPoint([In] System.Drawing.Point pt, [In] uint dwFlags);

        //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510(v=vs.85).aspx
        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX,
            [Out] out uint dpiY);

        #endregion

        #region SetDPIAwareness

        /// <summary>
        /// IMPORTANT: This only works if this is called in the immediate startup code
        /// of the application. For WPF this means `static App() { }`.
        /// </summary>
        public static bool SetPerMonitorDpiAwareness(
            ProcessDpiAwareness type = ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware)
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

        #region Window Placement

        /// <summary>
        ///  Centers a WPF window on the screen. Considers DPI settings
        /// </summary>
        /// <param name="window"></param>
        public static void CenterWindow(Window window)
        {
            var hwnd = WindowToHwnd(window);

            var screen = Screen.FromHandle(hwnd);
            var ratio = Convert.ToDouble(GetDpiRatio(hwnd));
            var windowWidth = screen.Bounds.Width / ratio;
            var windowHeight = screen.Bounds.Height / ratio;

            
            var offsetWidth = (windowWidth - window.Width) /  2 + (screen.Bounds.X * ratio);
            var offsetHeight = (windowHeight -  window.Height) / 2 + (screen.Bounds.Y * ratio);

            window.Left = Convert.ToDouble(offsetWidth);
            window.Top = Convert.ToDouble(offsetHeight);
        }

        /// <summary>
        /// Ensures that the window rendered is visible and fitting
        /// on the currently active screen.
        /// </summary>
        /// <param name="window"></param>
        public static void EnsureWindowIsVisible(Window window)
        {
            var hwnd = WindowToHwnd(window);
            var screen = Screen.FromHandle(hwnd);
            var ratio = Convert.ToDouble(GetDpiRatio(hwnd));

            var screenWidth = screen.Bounds.Width / ratio;
            var screenHeight = screen.Bounds.Height / ratio;
            var screenX = screen.Bounds.X / ratio;
            var screenY = screen.Bounds.Y / ratio;

            var windowLeftAbsolute = window.Left + screenX; // absolute Left
            var windowTopAbsolute = window.Top + screenY; // absolute Top

            if (window.Left + window.Width  > screenWidth)
                //if (window.Left + window.Width - screenX > screenWidth)
            {
                // move window into visible space
                window.Left = screenX + screenWidth - window.Width;
                windowLeftAbsolute = window.Left;
            }
            if (windowLeftAbsolute < screenX)
            {
                window.Left = 20 + screenX;
                if (window.Width + 20 > screenWidth)
                    window.Width = screenWidth - 40;
            }


            if (window.Top + window.Height > screenHeight - 40)
            {
                window.Top = screenY + screenHeight - window.Height - 40;
                windowTopAbsolute = window.Top;

            }
            if (windowTopAbsolute < screenY)
            {
                window.Top = 20 + screenY;
                if (window.Height + 20 > screenHeight)
                    window.Height = screenHeight - 60;
            }
        }
        #endregion

        #region Conversions and screen sizes

        /// <summary>
        /// Returns the active screen's size in pixels
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static Rectangle GetScreenDimensions(Window window)
        {
            var screen = Screen.FromHandle(new WindowInteropHelper(window).Handle);
            return screen.Bounds;
        }


        /// <summary>
        /// Returns IntPtr for an HWND from  WPF Window object
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static IntPtr WindowToHwnd(Window window)
        {
            return new WindowInteropHelper(window).EnsureHandle();
        }

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

    [DebuggerDisplay("L:{Left} T:{Top}  {Width}x{Height} - C:{CenteredInMainWindow}")]
    public class WindowPosition
    {
        public double Left {get; set; }
        public double Top {get; set; }

        public double Width {get; set; }

        public double Height {get; set; }

        public bool CenterInMainWindow { get; set; } = false;
    }

}
